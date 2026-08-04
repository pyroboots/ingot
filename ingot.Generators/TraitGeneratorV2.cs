using System.Text;

using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.TraitSystem;

using Newtonsoft.Json;

using Formatting = ingot.Core.Common.Formatting;
using Version = ingot.Core.Common.Version;

namespace ingot.Generators;

public struct ComponentSchema
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
    [JsonProperty("required")] public string[] Required;
    [JsonProperty("properties")] public Dictionary<string, ComponentSchemaProperty> Properties;
    [JsonProperty("oneOf")] public ComponentSchemaOneOfNode[] OneOf;
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

    public static string GenerateItemFromSchema(string json, string ns)
    {
        // get rid of $refs for clean serialization
        json = JsonResolver.Resolve(json);

        ComponentSchema schema = JsonConvert.DeserializeObject<ComponentSchema>(json);
        return GenerateItemFromSchema(schema, ns);
    }

    public static string GenerateItemFromSchema(ComponentSchema schema, string ns)
    {
        TraitInterfaceBuilder iface = new(schema.Description,
            Formatting.SnakeToPascalCase(schema.Component.Split(':')[1]), new(schema.Component),
            TraitSystem.TraitType.Item,
            new(schema.FormatVer), ns,
            ["ingot.Core.Common", "ingot.Core.TraitSystem", "ingot.Core.TraitSystem.Traits"]);

        List<string> logs = new();
        
        // really fragile system to resolve oneOf nodes
        if (schema.Properties is null)
        {
            // prefer paths that have properties
            if (schema.OneOf.Any((node) => node.Properties is not null))
            {
                ComponentSchema.ComponentSchemaOneOfNode node =
                    schema.OneOf.First((node) => node.Properties is not null);
                schema.Properties = node.Properties;
            }
            else
            {
                // and if we cant fine one, we'll just make one that will likely work
                string type = schema.OneOf[0].Type;
                schema.Properties = new()
                {
                    ["value"] = new() { Default = null, Description = "Value of the component", Type = type }
                };
            }
        }

        foreach (var kvp in schema.Properties)
        {
            ComponentSchema.ComponentSchemaProperty prop = kvp.Value;
            string name = Formatting.SnakeToPascalCase(kvp.Key);

            List<TraitPropertyConstraintAttribute> constraints = new();
            if (prop.Enum is not null)
            {
                iface.Enum(prop.EnumTitle ?? name, prop.Enum);
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

        Directory.Delete(outputDir, true);
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

            string iface = GenerateItemFromSchema(schema, "ingot.Core.TraitSystem.Traits.Item");
            string fileName = $"I{Path.GetFileNameWithoutExtension(file.path).Replace(" ", "")}.cs";

            Console.WriteLine($"({c}/{files.Length}) generated {fileName}");
            await File.WriteAllTextAsync(Path.Combine(outputDir, fileName), iface);
        }
    }
}
