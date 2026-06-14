using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ingot.Core.Common;

namespace ingot.Generators;

public record TraitProperty(
    string Name,
    string DefaultValue,
    string RawType,
    string Description
);

public class TraitGenerator
{
    private readonly HttpClient _httpClient = new HttpClient();

    // convert ms doc types to c# types
    // NOTE: works on a contains basis, order of presidence.
    private static readonly Dictionary<string, string> TypeMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["x, y, z coordinate array"] = "Vector3",
        ["Array of numbers"] = "int[]",
        ["Array of strings"] = "string[]",
        ["Boolean"] = "bool",
        ["bool"] = "bool",
        ["Integer"] = "int",
        ["integer"] = "int",
        ["Decimal"] = "float",
        ["decimal"] = "float",
        ["String"] = "string",
        ["string"] = "string",
        ["Object"] = "dynamic",
        ["keyed set of strings"] = "Dictionary<string, string>",
    };

    // formatters
    private static readonly Dictionary<string, Func<string, string>> ValueTransformers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["bool"] = v => bool.TryParse(v, out bool b) ? b.ToString().ToLowerInvariant() : "false",
        ["int"] = v => int.TryParse(v, out int i) ? i.ToString() : "0",
        ["float"] = v =>
        {
            if (string.IsNullOrEmpty(v)) return "0f";
            if (!v.EndsWith("f", StringComparison.OrdinalIgnoreCase) && float.TryParse(v, out _))
                return v + "f";
            return v;
        },
        ["string"] = v =>
        {
            if (string.IsNullOrEmpty(v) || v == "{}") return "\"\"";
            return $"\"{v.Replace("\"", "\\\"")}\"";
        },
        ["string[]"] = v =>
        {
            if (string.IsNullOrWhiteSpace(v) || v == "[]") return "Array.Empty<string>()";
            string[] parts = v.Split(',');
            string joined = string.Join(", ", parts.Select(s => $"\"{s.Trim()}\""));
            return $"new[] {{ {joined} }}";
        },
        ["int[]"] = v => v,
        ["Vector3"] = v => $"new Vector3({v})",
        ["Dictionary<string, string>"] = v => string.IsNullOrWhiteSpace(v) ? "new Dictionary<string, string>()" : v,
        
        ["dynamic?"] = v => "null",
    };

    public static string GenerateTraitInterfaceFromMsDoc(string html, string interfaceName, string componentName, string constraint, string? @namespace = null)
    {
        List<TraitProperty> properties = ParseHtmlToProperties(html);
        return GenerateInterfaceCode(interfaceName, componentName, constraint, properties, @namespace);
    }

    private static List<TraitProperty> ParseHtmlToProperties(string html)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);

        HtmlNode? table = doc.DocumentNode.SelectSingleNode("//table");
        if (table == null) 
            return new List<TraitProperty>();

        List<TraitProperty> properties = new List<TraitProperty>();
        HtmlNodeCollection? rows = table.SelectNodes(".//tr");

        if (rows == null) 
            return properties;

        bool isHeader = true;

        foreach (HtmlNode row in rows)
        {
            HtmlNodeCollection? cells = row.SelectNodes("th|td");
            if (cells == null || cells.Count < 4) 
                continue;

            string name = cells[0].InnerText.Trim();
            string defaultVal = cells[1].InnerText.Trim();
            string type = cells[2].InnerText.Trim();
            string desc = cells[3].InnerText.Trim();

            if (isHeader && name.Equals("Name", StringComparison.OrdinalIgnoreCase))
            {
                isHeader = false;
                continue;
            }
            isHeader = false;

            if (desc.Contains("Deprecated", StringComparison.OrdinalIgnoreCase))
                continue;

            string cleanName = CleanPropertyName(name);
            string cleanDesc = Regex.Replace(desc, @"\s+", " ").Trim();
            string cleanDefault = defaultVal.Equals("not set", StringComparison.OrdinalIgnoreCase) 
                || string.IsNullOrEmpty(defaultVal) 
                ? "" : defaultVal;

            properties.Add(new TraitProperty(cleanName, cleanDefault, type, cleanDesc));
        }

        return properties;
    }

    private static string CleanPropertyName(string name)
    {
        name = Regex.Replace(name, @"\s*\(Use On\)", "", RegexOptions.IgnoreCase);
        name = Regex.Replace(name, @"\s*\(as .*?\)", "", RegexOptions.IgnoreCase);
        name = name.Trim().ToLowerInvariant().Replace(" ", "_");
        return name;
    }

    private static string GenerateInterfaceCode(string interfaceName, string componentName, string constraint, List<TraitProperty> properties, string? nspace)
    {
        nspace ??= $"namespace ingot.Core.TraitSystem.Traits.{constraint};";

        StringBuilder sb = new StringBuilder();
        HashSet<string> seen = new HashSet<string>();

        sb.AppendLine(nspace);
        sb.AppendLine("using System.Numerics;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();
        sb.AppendLine($"[Trait(\"{componentName}\", TraitSystem.TraitType.{constraint})]");
        sb.AppendLine($"public interface {interfaceName} : I{constraint}Trait");
        sb.AppendLine("{");

        foreach (TraitProperty prop in properties)
        {
            string pascalName = Formatting.SnakeToPascalCase(prop.Name);
            if (!seen.Add(pascalName)) 
                continue;

            string csharpType = MapType(prop.RawType);
            bool isAbstract = string.IsNullOrEmpty(prop.DefaultValue);
            string defaultExpr = isAbstract ? "" : FormatDefaultValue(csharpType, prop.DefaultValue);

            if (!string.IsNullOrEmpty(prop.Description))
            {
                sb.AppendLine($"    /// <summary>");
                sb.AppendLine($"    /// {prop.Description}");
                sb.AppendLine($"    /// </summary>");
            }
            sb.AppendLine("    [TraitProperty]");

            if (isAbstract)
            {
                if (pascalName.Contains("Identifier")) csharpType = "Identifier";
                sb.AppendLine($"    public abstract {csharpType} {pascalName} {{ get; }}");
            }
            else
                sb.AppendLine($"    public virtual {csharpType} {pascalName} => {defaultExpr};");

            sb.AppendLine();
        }

        // trim
        if (properties.Count > 0)
            sb.Length -= Environment.NewLine.Length;

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string MapType(string rawType)
    {
        foreach (KeyValuePair<string, string> mapping in TypeMappings)
        {
            if (rawType.Contains(mapping.Key, StringComparison.OrdinalIgnoreCase))
                return mapping.Value;
        }
        Console.WriteLine($"/!\\ could not map type '{rawType}', fell back to nullable dynamic");
        return "dynamic?"; // fallback
    }

    private static string FormatDefaultValue(string csharpType, string rawValue)
    {
        if (ValueTransformers.TryGetValue(csharpType, out Func<string, string>? transformer))
            return transformer(rawValue);

        // fallback
        if (csharpType == "string")
            return $"\"{rawValue.Replace("\"", "\\\"")}\"";

        return rawValue;
    }

    public void GenerateAllItemTraits(string outputDir)
    {
        string[] components =
        {
            //"minecraft:allow_off_hand", // handled in item
            "minecraft:block_placer",
            "minecraft:bundle_interaction",
            "minecraft:can_destroy_in_creative",
            "minecraft:compostable",
            "minecraft:cooldown",
            "minecraft:damage",
            "minecraft:damage_absorption",
            "minecraft:digger",
            //"minecraft:display_name", // handled in item
            "minecraft:durability",
            "minecraft:durability_sensor",
            "minecraft:dyeable",
            "minecraft:enchantable",
            "minecraft:fire_resistant",
            "minecraft:food",
            "minecraft:fuel",
            "minecraft:glint",
            "minecraft:hand_equipped",
            "minecraft:hover_text_color",
            //"minecraft:icon", // handled in item
            "minecraft:interact_button",
            "minecraft:kinetic_weapon",
            "minecraft:liquid_clipped",
            //"minecraft:max_stack_size", // handled in item
            "minecraft:piercing_weapon",
            "minecraft:projectile",
            "minecraft:rarity",
            "minecraft:record",
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
            "minecraft:use_modifiers",
            "minecraft:wearable",
        };
        
        GenerateTraitsForComponents(components, outputDir, "Item", "minecraft_");
    }

    public void GenerateAllBlockTraits(string outputDir)
    {
        string[] components =
        {
            "minecraft:chest_obstruction",
            "minecraft:collision_box",
            "minecraft:connection_rule",
            "minecraft:crafting_table",          // from "Crafting Table" link
            //"minecraft:destroy_time",          // use destructible_by_mining instead
            "minecraft:destructible_by_explosion",
            "minecraft:destructible_by_mining",
            "minecraft:destruction_particles",   // from "Destruction Particles"
            //"minecraft:display_name",          // handled in block
            "minecraft:entity_fall_on",
            //"minecraft:explosion_resistance",
            "minecraft:flammable",
            //"minecraft:flower_pottable",       // who needs flower pots anyway
            //"minecraft:friction",              // handled in block
            "minecraft:geometry",
            "minecraft:instrument_sound",
            //"minecraft:light_dampening",       // handled in block
            //"minecraft:light_emission",        // handled in block
            "minecraft:liquid_detection",        // from "Liquid Detection"
            //"minecraft:loot",                  // handled in block
            "minecraft:map_color",
            //"minecraft:material_instances",    // handled in block
            "minecraft:movable",
            "minecraft:placement_filter",        // from "Placement Filter"
            "minecraft:precipitation_interactions",
            "minecraft:random_offset",
            "minecraft:redstone_conductivity",   // from "Redstone Conductivity"
            "minecraft:redstone_consumer",
            "minecraft:redstone_producer",
            //"minecraft:replaceable",           // handled in block
            "minecraft:selection_box",
            "minecraft:support",
            "minecraft:tick",
            //"minecraft:transformation",        // from "Transformation"
        };

        GenerateTraitsForComponents(components, outputDir, "Block", "minecraftblock_");
    }

    private void GenerateTraitsForComponents(string[] components, string outputDir, string constraint, string urlPrefix)
    {
        Stopwatch sw = Stopwatch.StartNew();
        int success = 0;

        foreach (string component in components)
        {
            Console.WriteLine($"generating '{component}' interface...");

            try
            {
                string[] parts = component.Split(':');
                string pageName = parts[1];
                string url = $"https://learn.microsoft.com/en-us/minecraft/creator/reference/content/{constraint.ToLowerInvariant()}reference/examples/{constraint.ToLowerInvariant()}components/{urlPrefix}{pageName}?view=minecraft-bedrock-stable";
                
                string html = _httpClient.GetStringAsync(url).Result;

                string pascalName = Formatting.SnakeToPascalCase(pageName);
                string ifaceName = $"I{pascalName}";

                string code = GenerateTraitInterfaceFromMsDoc(html, ifaceName, component, constraint);
                string fullCode = $"// autogenerated by ingot trait generator from\n// {url}\n\n{code}";

                string path = Path.Combine(outputDir, $"{ifaceName}.cs");
                File.WriteAllText(path, fullCode);

                success++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"failed to generate {component}: {ex.Message}");
            }
        }

        sw.Stop();
        Console.WriteLine($"completed {success}/{components.Length} traits in {sw.ElapsedMilliseconds}ms");
    }
}