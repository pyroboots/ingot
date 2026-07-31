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
        
        [JsonProperty("maximum")] public int? Max;
        [JsonProperty("minimum")] public int? Min;
        
        [JsonProperty("items")] public dynamic? Items;
    }
    
    [JsonProperty("description")] public string Description;
    [JsonProperty("title")] public string Component;
    [JsonProperty("x-format-version")] public string FormatVer;
    [JsonProperty("required")] public string[] Required;
    [JsonProperty("properties")] public Dictionary<string, ComponentSchemaProperty> Properties;
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

    private static string ConvertType(string type) => TypeMap.ContainsKey(type) ? TypeMap[type] : "dynamic";
    
    public static string GenerateItemFromSchema(string json, string ns)
    {
        // get rid of $refs for clean serialization
        json = JsonResolver.Resolve(json);
        
        ComponentSchema schema = JsonConvert.DeserializeObject<ComponentSchema>(json);
        TraitInterfaceBuilder iface = new(schema.Description, Formatting.SnakeToPascalCase(schema.Component.Split(':')[1]), new(schema.Component), TraitSystem.TraitType.Item,
            new(schema.FormatVer), ns, ["ingot.Core.Common"]);

        List<string> logs = new();
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
            iface.AddProperty(prop.Description, name, prop.Default is null || prop.Default is string && (string)prop.Default == "", ConvertType(prop.Type), prop.Default, constraints.ToArray());
        }
        
        iface.ExtraHeaders = logs.ToArray();
        return iface.Generate();
    }
}