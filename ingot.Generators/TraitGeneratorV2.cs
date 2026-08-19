using ingot.Core.TraitSystem;

using Newtonsoft.Json;

using Formatting = ingot.Core.Common.Formatting;
using Version = ingot.Core.Common.Version;

namespace ingot.Generators;

// record so i can use with
public record struct ComponentSchema
{
    public struct ComponentSchemaProperty
    {
        [JsonProperty("description")] public string Description;
        [JsonProperty("type")] public string Type;
        
        [JsonProperty("default")] public object? Default;
        [JsonProperty("enum")] public string[]? Enum;
        [JsonProperty("title")] public string? EnumTitle;
        
        [JsonProperty("maximum")] public float? Max;
        [JsonProperty("minimum")] public float? Min;
        
        [JsonProperty("items")] public dynamic? Items;
    }

    public class ComponentSchemaOneOfNode
    {
        [JsonProperty("type")] public string Type;
        [JsonProperty("properties")] public Dictionary<string, ComponentSchemaProperty> Properties;
    }
    
    [JsonProperty("description")] public string Description;
    [JsonProperty("title")] public string Component;
    [JsonProperty("x-format-version")] public string FormatVer;
    // needed for block components with no oneOf props or vals
    [JsonProperty("type")] public string Type;
    [JsonProperty("required")] public string[] Required;
    [JsonProperty("properties")] public Dictionary<string, ComponentSchemaProperty> Properties;
    [JsonProperty("oneOf")] public ComponentSchemaOneOfNode[] OneOf;
    // some block components can be deprecated, so we skip if they are
    [JsonProperty("deprecated")] public bool? Deprecated;
}

public static class TraitGeneratorV2
{
    private static readonly Dictionary<string, string> TypeMap = new()
    {
        ["integer"] = "int",
        ["array"] = "string[]",
        ["string"] = "string",
        ["boolean"] = "bool",
        ["number"] = "float",
    };

    private static string ConvertType(string type)
    {
        if (type is null) return "dynamic";
        return TypeMap.ContainsKey(type) ? TypeMap[type] : "dynamic";
    }
    
    public static string GenerateTraitFromSchema(string json, string ns, TraitSystem.TraitType type, string traitInterface)
    {
        // get rid of $refs for clean serialization
        json = JsonResolver.Resolve(json);

        ComponentSchema schema = JsonConvert.DeserializeObject<ComponentSchema>(json);
        return GenerateTraitFromSchema(schema, ns, type, traitInterface);
    }

    public static string GenerateTraitFromSchema(ComponentSchema schema, string ns, TraitSystem.TraitType type, string traitInterface)
    {
        TraitInterfaceBuilder iface = new(schema.Description,
            Formatting.SnakeToPascalCase(schema.Component.Split(':')[1]), new(schema.Component),
            type,
            new(schema.FormatVer), traitInterface, ns,
            ["ingot.Core.Common", "ingot.Core.TraitSystem", "ingot.Core.TraitSystem.Traits"]);

        List<string> logs = new();
        
        // resolve oneOf / primitive-root / empty marker schemas into a properties map
        if (schema.Properties is null)
        {
            if (schema.OneOf is not null && schema.OneOf.Any(node => node.Properties is not null))
            {
                // prefer oneOf branches that expose object properties
                schema.Properties = schema.OneOf.First(node => node.Properties is not null).Properties;
            }
            else if (schema.OneOf is { Length: > 0 } && schema.OneOf[0].Type is not null)
            {
                // oneOf is bare types only - invent a synthetic value property
                schema.Properties = new()
                {
                    ["value"] = new()
                    {
                        Default = null, Description = "Value of the component", Type = schema.OneOf[0].Type
                    }
                };
            }
            else if (schema.Type is not null)
            {
                // single's type
                schema.Properties = new()
                {
                    ["value"] = new()
                    {
                        Default = null,
                        Description = schema.Description ?? "Value of the component",
                        Type = schema.Type
                    }
                };
            }
            else
            {
                // some components are markers with no props, like flower_pottable
                schema.Properties = new();
            }
        }

        foreach (var kvp in schema.Properties)
        {
            ComponentSchema.ComponentSchemaProperty prop = kvp.Value;
            string name = Formatting.SnakeToPascalCase(kvp.Key);

            List<TraitPropertyConstraintAttribute> constraints = new();
            if (prop.Enum is not null)
            {
                // i was lucky with items, turns out block component enums can have spaces in
                iface.Enum(Formatting.SnakeToPascalCase((prop.EnumTitle ?? name).Replace(" ", "_")), prop.Enum);
                constraints.Add(new(TraitPropertyConstraintAttribute.Constraint.OneOf, prop.Enum));
            }
            else if (prop is { Type: "integer", Max: not null, Min: not null })
                constraints.Add(new(TraitPropertyConstraintAttribute.Constraint.Range, prop.Min, prop.Max));
            else if (prop is { Type: "integer", Min: not null })
                constraints.Add(new(TraitPropertyConstraintAttribute.Constraint.GreaterThanEq, prop.Min));
            else if (prop is { Type: "integer", Max: not null })
                constraints.Add(new(TraitPropertyConstraintAttribute.Constraint.LessThanEq, prop.Max));

            if (ConvertType(prop.Type) == "dynamic")
                logs.Add($"log: unable to resolve type {prop.Type} on property {name}");
            iface.AddProperty(prop.Description, name,
                prop.Default is null || prop.Default is string && (string)prop.Default == "", ConvertType(prop.Type),
                prop.Default, constraints.ToArray());
        }

        iface.ExtraHeaders = logs.ToArray();
        return iface.Generate();
    }

    public static async Task GenerateItemTraits(string outputDir, string token)
    {
        string treeJson = await RepoTreeCrawler.GetTree("mojang", "bedrock-samples",
            "metadata/json_schemas/server/item_components", token: token);
        (string path, string content)[] files = await RepoTreeCrawler.GetFileContentsWithPaths(treeJson, token);

        //Directory.Delete(outputDir, true);
        Directory.CreateDirectory(outputDir);

        int c = 0;
        foreach (var file in files)
        {
            c++;
            string resolved = JsonResolver.Resolve(file.content);
            ComponentSchema schema = JsonConvert.DeserializeObject<ComponentSchema>(resolved);
            bool isComponent = schema.Component.Contains("minecraft:") && !schema.Component.Contains(" ");
            if (isComponent == false)
            {
                Console.WriteLine($"({c}/{files.Length}) skipped {schema.Component} - not a component");
                continue;
            }
            if (schema.Deprecated == true)
            {
                Console.WriteLine($"({c}/{files.Length}) skipped {schema.Component} - deprecated");
                continue;
            }

            string iface = GenerateTraitFromSchema(schema, "ingot.Core.TraitSystem.Traits.Item", TraitSystem.TraitType.Item, nameof(Core.TraitSystem.Traits.IItemTrait));
            string fileName = $"I{Path.GetFileNameWithoutExtension(file.path).Replace(" ", "")}.cs";

            string path = Path.Combine(outputDir, fileName);
            if (File.Exists(path))
            {
                if ((await File.ReadAllTextAsync(path)).Contains(TraitInterfaceBuilder.AutogeneratedWatermark) == false)
                {
                    Console.WriteLine($"({c}/{files.Length}) skipped {fileName} - dont overwrite custom files");
                    continue;
                }
            }
            
            Console.WriteLine($"({c}/{files.Length}) generated {fileName}");
            await File.WriteAllTextAsync(Path.Combine(outputDir, fileName), iface);
        }
    }

    // inline attributes on properties what?!
    record BlockComponentsJson(Dictionary<string, ComponentSchema> properties, [JsonProperty("x-format-version")] string fmtVer);
    public static async Task GenerateBlockTraits(string outputDir)
    {
        // all in a single file
        string json = await new HttpClient().GetStringAsync(
            "https://raw.githubusercontent.com/Mojang/bedrock-samples/refs/heads/main/metadata/json_schemas/server/block_components/1.26.20/Block%20Components.json");
        json = JsonResolver.Resolve(json);
        
        if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
        Directory.CreateDirectory(outputDir);
        
        BlockComponentsJson componentsJson = JsonConvert.DeserializeObject<BlockComponentsJson>(json)!;
        Dictionary<string, ComponentSchema> components = componentsJson.properties
            .Select(kvp =>
            {
                KeyValuePair<string, ComponentSchema> newKvp = new(kvp.Key,
                    kvp.Value with { FormatVer = componentsJson.fmtVer });
                return newKvp;
            }).ToDictionary();

        int c = 0;
        foreach (var kvp in components)
        {
            c++;

            ComponentSchema schema = kvp.Value;
            // we swap here because TraitInterfaceBuilder relies on Component being the id, not
            // the human readable name with spaces
            string friendlyName = schema.Component;
            schema.Component = kvp.Key;
            
            bool isComponent = schema.Component.Contains("minecraft:") && !schema.Component.Contains(" ");
            if (isComponent == false)
            {
                Console.WriteLine($"({c}/{components.Count}) skipped {schema.Component} - not a component");
                continue;
            }
            if (schema.Deprecated == true)
            {
                Console.WriteLine($"({c}/{components.Count}) skipped {schema.Component} - deprecated");
                continue;
            }

            string iface = GenerateTraitFromSchema(schema, "ingot.Core.TraitSystem.Traits.Block", TraitSystem.TraitType.Block, nameof(Core.TraitSystem.Traits.IBlockTrait));
            string fileName = $"I{Path.GetFileNameWithoutExtension(Formatting.SnakeToPascalCase(friendlyName.Replace(" ", "_")))}.cs";

            Console.WriteLine($"({c}/{components.Count}) generated {fileName}");
            await File.WriteAllTextAsync(Path.Combine(outputDir, fileName), iface);
        }
    }
}
