using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace ingot.Generators;

public class TraitGenerator
{
    private static string GenerateCsv(string html)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(html);
    
        // find the table in the msdoc
        HtmlNode? table = doc.DocumentNode.SelectSingleNode("//table");
    
        if (table == null)
            return "Table not found!";
    
        StringBuilder csvBuilder = new();
        HtmlNodeCollection? rows = table.SelectNodes(".//tr");
    
        if (rows == null) return string.Empty;
    
        foreach (HtmlNode row in rows)
        {
            HtmlNodeCollection? cells = row.SelectNodes("th|td");
            if (cells == null || cells.Count < 4) continue;
    
            string name = cells[0].InnerText.Trim();
            string defaultVal = cells[1].InnerText.Trim();
            string type = cells[2].InnerText.Trim();
            string desc = cells[3].InnerText.Trim();
            
            if (name == "Name")
            {
                AppendCsvLine(csvBuilder, new[] { "name", "default", "type", "desc" });
                continue;
            }
    
            // convert types to c#, rough, isnt perfect but will work most of the time
            if (type.Contains("Boolean")) type = "bool";
            else if (type.Contains("Array")) type = "string[]";
            else if (type.Contains("Object")) type = "dynamic";
            else if (type.Contains("integer")) type = "int";
            else if (type.Contains("decimal")) type = "float";
            else if (type.Contains("keyed")) type = "Dictionary<string, string>";
            else if (type == "String") type = "string";
    
            // clean default val
            if (defaultVal.Equals("not set", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(defaultVal)) 
                defaultVal = "";
    
            // sometimes names have extra info in brackets, we dont need those
            string cleanName = name;
            cleanName = Regex.Replace(cleanName, @"\s*\(Use On\)", "", RegexOptions.IgnoreCase);
            cleanName = Regex.Replace(cleanName, @"\s*\(as .*?\)", "", RegexOptions.IgnoreCase);
            cleanName = cleanName.Trim().ToLower().Replace(" ", "_");
    
            // trim
            string cleanDesc = Regex.Replace(desc, @"\s+", " ").Trim();
    
            // add the row
            AppendCsvLine(csvBuilder, new[] { 
                cleanName, 
                defaultVal, 
                type.ToLower(), 
                cleanDesc 
            });
        }
    
        return csvBuilder.ToString().TrimEnd();
    }

    private static void AppendCsvLine(StringBuilder sb, string[] fields)
    {
        // escape content
        string[] formattedFields = fields.Select(f => $"\"{f.Replace("\"", "\"\"")}\"").ToArray();
        sb.AppendLine(string.Join(",", formattedFields));
    }
    
    private static string GenerateTraitInterface(string ifaceName, string componentName, string constraint, string csv, string? nspace = null)
    {
        StringBuilder sb = new();
        HashSet<string> seenProperties = new();

        // class header
        nspace ??= $"namespace ingot.Core.TraitSystem.Traits.{constraint};";
        sb.AppendLine(nspace);
        sb.AppendLine();
        sb.AppendLine($"[Trait(\"{componentName}\", TraitSystem.TraitType.{constraint})]");
        sb.AppendLine($"public interface {ifaceName}");
        sb.AppendLine("{");

        List<string[]> rows = ParseCsv(csv);
        bool isFirstRow = true;

        foreach (string[] row in rows)
        {
            if (row.Length < 4) continue; // skip rows if theyre broken

            string rawName = row[0].Trim();
            string rawDefault = row[1].Trim();
            string rawType = row[2].Trim();
            string desc = row[3].Trim();

            // skip header, not a property
            if (isFirstRow && rawName.Equals("name", StringComparison.OrdinalIgnoreCase))
            {
                isFirstRow = false;
                continue;
            }
            isFirstRow = false;

            // if its deprecated, get rid of it
            // was thinking of adding the Deprecated attribute, but i decided against it
            if (desc.Contains("Deprecated", StringComparison.OrdinalIgnoreCase)) continue;
            
            string propName = ToPascalCase(rawName);
            if (!seenProperties.Add(propName)) continue;
            
            string mappedType = rawType switch
            {
                "integer number" => "int",
                "integer" => "int",
                "decimal number" => "float",
                "decimal" => "float",
                "bool" => "bool",
                "string" => "string",
                "string[]" => "string[]",
                "keyed set of strings" => "Dictionary<string, string>",
                _ => "dynamic"
            };

            // if it has no default value, then it must be implemented
            bool isAbstract = string.IsNullOrEmpty(rawDefault);
            string formattedDefault = rawDefault;

            if (!isAbstract)
            {
                if (mappedType == "float" && !formattedDefault.EndsWith("f", StringComparison.OrdinalIgnoreCase))
                    formattedDefault += "f";
                else if (mappedType == "string")
                {
                    // edge cases like {} for empty strings
                    if (formattedDefault == "{}") formattedDefault = "\"\"";
                    else formattedDefault = $"\"{formattedDefault}\"";
                }
            }

            // add property
            sb.AppendLine("    [TraitProperty]");
            if (!string.IsNullOrEmpty(desc))
                sb.AppendLine($"    /* {desc} */");
            if (isAbstract)
                sb.AppendLine($"    public abstract {mappedType} {propName} {{ get; }}");
            else
                sb.AppendLine($"    public virtual {mappedType} {propName} => {formattedDefault};");
            sb.AppendLine();
        }
        
        if (seenProperties.Count > 0) sb.Length -= Environment.NewLine.Length;
        sb.AppendLine("}");
        return sb.ToString();
    }
    
    public static string GenerateTraitInterfaceFromMsDoc(string html, string ifaceName, string componentName, string constraint)
    {
        string csv = GenerateCsv(html);
        string iface = GenerateTraitInterface(ifaceName, componentName, constraint, csv);
        
        return iface;
    }

    private static string ToPascalCase(string snakeCase)
    {
        if (string.IsNullOrEmpty(snakeCase)) return snakeCase;
        return string.Join("", snakeCase
            .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }

    private static List<string[]> ParseCsv(string csv)
    {
        var result = new List<string[]>();
        using var reader = new StringReader(csv);
        string line;

        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var row = new List<string>();
            bool inQuotes = false;
            var currentField = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '\"')
                {
                    // escaped quotes ("")
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                    {
                        currentField.Append('\"');
                        i++; 
                    }
                    else
                        inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    row.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                    currentField.Append(c);
            }
            row.Add(currentField.ToString());
            result.Add(row.ToArray());
        }

        return result;
    }

    public static void GenerateAllItemTraits(string outputDir)
    {
        string[] components =
        {
            "minecraft:allow_off_hand",
            "minecraft:block_placer",
            "minecraft:bundle_interaction",
            "minecraft:can_destroy_in_creative",
            "minecraft:compostable",
            //"minecraft:cooldown",
            "minecraft:damage",
            "minecraft:damage_absorption",
            "minecraft:digger",
            "minecraft:display_name",
            //"minecraft:durability",
            "minecraft:durability_sensor",
            "minecraft:dyeable",
            "minecraft:enchantable",
            //"minecraft:entity_placer",
            "minecraft:fire_resistant",
            "minecraft:food",
            "minecraft:fuel",
            "minecraft:glint",
            "minecraft:hand_equipped",
            "minecraft:hover_text_color",
            "minecraft:icon",
            "minecraft:interact_button",
            //"minecraft:kinetic_weapon",
            "minecraft:liquid_clipped",
            "minecraft:max_stack_size",
            //"minecraft:piercing_weapon",
            "minecraft:projectile",
            "minecraft:rarity",
            //"minecraft:record",
            //"minecraft:repairable",
            "minecraft:seed",
            "minecraft:shooter",
            "minecraft:should_despawn",
            "minecraft:stacked_by_data",
            "minecraft:storage_item",
            "minecraft:storage_weight_limit",
            "minecraft:storage_weight_modifier",
            "minecraft:swing_duration",
            "minecraft:swing_sounds",
            "minecraft:tags",
            "minecraft:throwable",
            "minecraft:use_animation",
            //"minecraft:use_modifiers",
            "minecraft:wearable",
        };

        Stopwatch sw = Stopwatch.StartNew();
        HttpClient hc = new();
        int i = 0;
        foreach (string component in components)
        {
            i++;
            Console.WriteLine($"({i}/{components.Length}) generating '{component}' iface");
            
            string page = component.Replace(":", "_"); // minecraft_allow_off_hand
            string url = $"https://learn.microsoft.com/en-us/minecraft/creator/reference/content/itemreference/examples/itemcomponents/{page}?view=minecraft-bedrock-stable";
            string html = hc.GetStringAsync(url).Result;

            string pascalName = ToPascalCase(component.Split(":")[1]);
            string iface = GenerateTraitInterfaceFromMsDoc(html, $"I{pascalName}", component, "Item");
            string path = Path.Combine(outputDir, $"I{pascalName}.cs");

            iface = $"// autogenerated from \n//{url}\n\n" + iface;
            File.WriteAllText(path, iface);
        }
        sw.Stop();
        
        Console.WriteLine($"done ({sw.ElapsedMilliseconds}ms | {sw.ElapsedMilliseconds / components.Length}ms/c avg)");
    }
}