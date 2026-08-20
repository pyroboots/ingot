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
        [JsonProperty("title")] public string? Title;
        
        [JsonProperty("maximum")] public float? Max;
        [JsonProperty("minimum")] public float? Min;
        
        [JsonProperty("items")] public ComponentSchema? Items;
        [JsonProperty("oneOf")] public ComponentSchemaOneOfNode[]? OneOf;
    }

    public class ComponentSchemaOneOfNode
    {
        [JsonProperty("type")] public string Type;
        [JsonProperty("title")] public string? Title;
        [JsonProperty("properties")] public Dictionary<string, ComponentSchemaProperty>? Properties;
        [JsonProperty("required")] public string[]? Required;
        [JsonProperty("items")] public ComponentSchema? Items;
        [JsonProperty("oneOf")] public ComponentSchemaOneOfNode[]? OneOf;
        [JsonProperty("additionalProperties")] public ComponentSchemaProperty? AdditionalProperties;
    }
    
    [JsonProperty("description")] public string Description;
    [JsonProperty("title")] public string Title;
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
    private static readonly Dictionary<string, string> TypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["integer"] = "int",
        ["array"] = "string[]",
        ["string"] = "string",
        ["boolean"] = "bool",
        ["number"] = "float",
        ["BlockDescriptorProxy"] = "BlockTypeDescriptor",
        ["Block Descriptor Proxy"] = "BlockTypeDescriptor",
        ["Reference"] = "Identifier",
        ["Molang string"] = "Molang",
        ["IntRange"] = "Range",
        ["FloatRange"] = "Range",
        ["Fraction"] = "Fraction"
    };

    private static bool TryMapTitle(string? title, out string mapped)
    {
        if (title is not null && TypeMap.TryGetValue(title, out mapped!))
            return true;
        mapped = null!;
        return false;
    }

    private static string ConvertType(string? type)
    {
        if (type is null) return "dynamic";
        return TypeMap.TryGetValue(type, out string? mapped) ? mapped : "dynamic";
    }

    private static string ToTypeName(string? title, string fallback)
    {
        string t = string.IsNullOrWhiteSpace(title) ? fallback : title.Trim();
        if (t.Contains(":")) t = t.Split(':')[1];
        if (string.IsNullOrWhiteSpace(t)) return fallback;
        if (t.Contains(' ') || t.Contains('-') || t.Contains('_'))
            return Formatting.SnakeToPascalCase(t.Replace(' ', '_').Replace('-', '_'));
        return char.ToUpperInvariant(t[0]) + t[1..];
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
            Formatting.SnakeToPascalCase(schema.Title.Split(':')[1]), new(schema.Title),
            type,
            new(schema.FormatVer), traitInterface, ns,
            [
                "ingot.Core",
                "ingot.Core.Common", 
                "ingot.Core.TraitSystem", 
                "ingot.Core.TraitSystem.Traits", 
                "ingot.Core.Common.SharedConstructs",
                "Newtonsoft.Json"
            ]);

        List<string> logs = new();
        
        // resolve oneOf / primitive / empty markers into a properties map
        if (schema.Properties is null)
        {
            if (schema.OneOf is not null && schema.OneOf.Any(node => node.Properties is not null))
            {
                // prefer oneOf branches that expose object properties so the trait
                // gets named fields
                ComponentSchema.ComponentSchemaOneOfNode best = schema.OneOf
                    .Where(node => node.Properties is not null)
                    .OrderByDescending(node => node.Properties!.Count)
                    .First();
                schema.Properties = best.Properties!;
                if (schema.Required is null && best.Required is not null)
                    schema.Required = best.Required;
            }
            else if (schema.OneOf is { Length: > 0 })
            {
                // oneOf of bare types (e.g. boolean or string) - value
                // keeps the full oneOf so it can become Either<T1, T2>
                schema.Properties = new()
                {
                    ["value"] = new()
                    {
                        Default = null,
                        Description = schema.Description ?? "Value of the component",
                        OneOf = schema.OneOf
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
            string propType = ResolvePropertyType(prop, iface, logs, name);

            List<TraitPropertyConstraintAttribute> constraints = new();
            if (prop.Enum is not null)
            {
                // i was lucky with items, turns out block component enums can have spaces in
                iface.Enum(Formatting.SnakeToPascalCase((prop.Title ?? name).Replace(" ", "_")), prop.Enum);
                constraints.Add(new(TraitPropertyConstraintAttribute.Constraint.OneOf, prop.Enum));
            }
            else if (prop is { Type: "integer", Max: not null, Min: not null })
                constraints.Add(new(TraitPropertyConstraintAttribute.Constraint.Range, prop.Min, prop.Max));
            else if (prop is { Type: "integer", Min: not null })
                constraints.Add(new(TraitPropertyConstraintAttribute.Constraint.GreaterThanEq, prop.Min));
            else if (prop is { Type: "integer", Max: not null })
                constraints.Add(new(TraitPropertyConstraintAttribute.Constraint.LessThanEq, prop.Max));

            if (propType == "dynamic")
                logs.Add($"log: unable to resolve type {prop.Type} on property {name}");
            iface.AddProperty(prop.Description, name,
                prop.Default is null || prop.Default is string && (string)prop.Default == "", propType,
                prop.Default, constraints.ToArray());
        }

        iface.ExtraHeaders = logs.ToArray();
        return iface.Generate();
    }

    private static string ResolvePropertyType(
        ComponentSchema.ComponentSchemaProperty prop,
        TraitInterfaceBuilder iface,
        List<string> logs,
        string fallbackName)
    {
        if (prop.OneOf is { Length: > 0 })
            return ResolveOneOf(prop.OneOf, iface, logs, prop.Title ?? fallbackName);

        if (TryMapTitle(prop.Title, out string titled))
            return titled;

        if (prop.Type == "array")
        {
            if (prop.Items is { } items)
            {
                if (items.Properties is not null)
                {
                    string structName = ToTypeName(items.Title, fallbackName);
                    EnsureStruct(structName, items, iface, logs);
                    return $"{structName}[]";
                }

                return $"{ResolveSchemaType(items, iface, logs, fallbackName)}[]";
            }

            return "object[]";
        }

        return ConvertType(prop.Type);
    }

    private static string ResolveSchemaType(
        ComponentSchema schema,
        TraitInterfaceBuilder iface,
        List<string> logs,
        string fallbackName)
    {
        if (schema.OneOf is { Length: > 0 })
            return ResolveOneOf(schema.OneOf, iface, logs, schema.Title ?? fallbackName);

        if (TryMapTitle(schema.Title, out string titled))
            return titled;

        if (schema.Properties is not null)
        {
            string structName = ToTypeName(schema.Title, fallbackName);
            EnsureStruct(structName, schema, iface, logs);
            return structName;
        }

        return ConvertType(schema.Type);
    }

    private static string ResolveOneOf(
        ComponentSchema.ComponentSchemaOneOfNode[] branches,
        TraitInterfaceBuilder iface,
        List<string> logs,
        string fallbackName)
    {
        List<string> types = new();
        foreach (ComponentSchema.ComponentSchemaOneOfNode branch in FlattenOneOf(branches))
        {
            string type = ResolveOneOfBranch(branch, iface, logs, fallbackName);
            if (type != "dynamic" && types.Contains(type) == false)
                types.Add(type);
        }

        if (types.Count == 0)
        {
            logs.Add($"log: unresolved oneOf on {fallbackName}");
            return "dynamic";
        }

        if (types.Count == 1)
            return types[0];

        if (types.Count > 4)
        {
            logs.Add($"log: oneOf on {fallbackName} has {types.Count} types, Either only supports 4");
            return "dynamic";
        }

        return $"Either<{string.Join(", ", types)}>";
    }

    private static IEnumerable<ComponentSchema.ComponentSchemaOneOfNode> FlattenOneOf(
        ComponentSchema.ComponentSchemaOneOfNode[] branches)
    {
        foreach (ComponentSchema.ComponentSchemaOneOfNode branch in branches)
        {
            // known titles (Molang string, BlockDescriptorProxy stuff) stay as one thing
            if (TryMapTitle(branch.Title, out _))
            {
                // IEnumerable<T> with yield is sooo useful wow
                yield return branch;
                continue;
            }

            if (branch.OneOf is { Length: > 0 })
            {
                foreach (ComponentSchema.ComponentSchemaOneOfNode inner in FlattenOneOf(branch.OneOf))
                    yield return inner;
            }
            else yield return branch;
        }
    }

    private static string ResolveOneOfBranch(
        ComponentSchema.ComponentSchemaOneOfNode node,
        TraitInterfaceBuilder iface,
        List<string> logs,
        string fallbackName)
    {
        if (TryMapTitle(node.Title, out string titled))
            return titled;

        if (node.OneOf is { Length: > 0 })
            return ResolveOneOf(node.OneOf, iface, logs, node.Title ?? fallbackName);

        if (node.Type == "array")
        {
            if (node.Items is { } items)
                return $"{ResolveSchemaType(items, iface, logs, fallbackName)}[]";
            return "object[]";
        }

        if (node.Type == "object" || node.Properties is not null)
        {
            if (node.Properties is not null)
            {
                ComponentSchema objectSchema = new()
                {
                    Title = node.Title ?? fallbackName,
                    Properties = node.Properties,
                    Required = node.Required ?? [],
                    Type = "object"
                };
                string structName = ToTypeName(node.Title, fallbackName);
                EnsureStruct(structName, objectSchema, iface, logs);
                return structName;
            }

            if (node.AdditionalProperties is { } additional)
                return $"Dictionary<string, {ResolvePropertyType(additional, iface, logs, fallbackName + "Value")}>";

            return "object";
        }

        return ConvertType(node.Type);
    }

    private static void EnsureStruct(
        string structName,
        ComponentSchema schema,
        TraitInterfaceBuilder iface,
        List<string> logs)
    {
        string[] required = schema.Required ?? [];
        List<TraitInterfaceBuilder.StructProperty> fields = new();

        foreach (var kvp in schema.Properties!)
        {
            ComponentSchema.ComponentSchemaProperty field = kvp.Value;
            string fieldName = Formatting.SnakeToPascalCase(kvp.Key);
            string fieldType = ResolvePropertyType(field, iface, logs, fieldName);

            fields.Add(new(
                Name: fieldName,
                Description: field.Description,
                Type: fieldType,
                Required: required.Contains(kvp.Key),
                JsonName: kvp.Key
            ));
        }

        iface.Struct(structName, fields.ToArray());
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
            bool isComponent = schema.Title.Contains("minecraft:") && !schema.Title.Contains(" ");
            if (isComponent == false)
            {
                Console.WriteLine($"({c}/{files.Length}) skipped {schema.Title} - not a component");
                continue;
            }
            if (schema.Deprecated == true)
            {
                Console.WriteLine($"({c}/{files.Length}) skipped {schema.Title} - deprecated");
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
            string friendlyName = schema.Title;
            schema.Title = kvp.Key;
            
            bool isComponent = schema.Title.Contains("minecraft:") && !schema.Title.Contains(" ");
            if (isComponent == false)
            {
                Console.WriteLine($"({c}/{components.Count}) skipped {schema.Title} - not a component");
                continue;
            }
            if (schema.Deprecated == true)
            {
                Console.WriteLine($"({c}/{components.Count}) skipped {schema.Title} - deprecated");
                continue;
            }

            string iface = GenerateTraitFromSchema(schema, "ingot.Core.TraitSystem.Traits.Block", TraitSystem.TraitType.Block, nameof(Core.TraitSystem.Traits.IBlockTrait));
            string fileName = $"I{Path.GetFileNameWithoutExtension(Formatting.SnakeToPascalCase(friendlyName.Replace(" ", "_")))}.cs";

            Console.WriteLine($"({c}/{components.Count}) generated {fileName}");
            await File.WriteAllTextAsync(Path.Combine(outputDir, fileName), iface);
        }
    }
}
