using System.Text.RegularExpressions;

using ingot.Core.Common;
using ingot.Core.TraitSystem;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Formatting = ingot.Core.Common.Formatting;
using Version = ingot.Core.Common.Version;

namespace ingot.Generators;

/// <summary>
/// JSON schema node used while generating trait interfaces. Supports nested
/// <c>properties</c>, <c>items</c>, and <c>oneOf</c> after <see cref="JsonResolver"/> expansion.
/// </summary>
public class SchemaNode
{
    [JsonProperty("description")] public string? Description;
    [JsonProperty("type")] public string? Type;
    [JsonProperty("default")] public object? Default;
    [JsonProperty("enum")] public string[]? Enum;
    [JsonProperty("title")] public string? Title;
    [JsonProperty("maximum")] public float? Max;
    [JsonProperty("minimum")] public float? Min;
    [JsonProperty("items")] public SchemaNode? Items;
    [JsonProperty("properties")] public Dictionary<string, SchemaNode>? Properties;
    [JsonProperty("oneOf")] public SchemaNode[]? OneOf;
    [JsonProperty("additionalProperties")] public SchemaNode? AdditionalProperties;
    [JsonProperty("required")] public string[]? Required;
}

public class ComponentSchema
{
    [JsonProperty("description")] public string Description = "";
    [JsonProperty("title")] public string Component = "";
    [JsonProperty("x-format-version")] public string FormatVer = "1.0.0";
    [JsonProperty("required")] public string[]? Required;
    [JsonProperty("properties")] public Dictionary<string, SchemaNode>? Properties;
    [JsonProperty("oneOf")] public SchemaNode[]? OneOf;
    [JsonProperty("type")] public string? Type;
}

public static class TraitGeneratorV2
{
    private static readonly Dictionary<string, string> TypeMap = new()
    {
        ["integer"] = "int",
        ["string"] = "string",
        ["boolean"] = "bool",
        ["number"] = "float",
    };

    // schema titles that map to SharedConstructs / common types instead of generated subtypes
    private static readonly Dictionary<string, string> KnownTitles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["IntRange"] = "Range",
        ["FloatRange"] = "Range",
        ["Item Descriptor"] = "ItemTypeDescriptor",
        ["Block Descriptor"] = "BlockTypeDescriptor",
        ["Block Descriptor Proxy"] = "BlockTypeDescriptor",
        ["BlockDescriptorProxy"] = "BlockTypeDescriptor",
        ["Reference"] = "Identifier",
        ["SoundEventName"] = "string",
        ["compound_proxy"] = "object",
        ["Molang expression"] = "object",
        ["Molang string"] = "object",
        ["Color255RGB"] = "string",
        ["Fraction"] = "Fraction",
    };

    private static string ConvertPrimitive(string? type)
    {
        if (type is null) return "object";
        return TypeMap.TryGetValue(type, out string? mapped) ? mapped : "object";
    }

    /// <summary>
    /// Pull a clean component identifier out of schema titles like
    /// <c>minecraft:icon v1.21.80</c> or <c>minecraft:use_modifiers (beta)</c>
    /// </summary>
    private static (string ComponentId, string TypeName) ParseComponentTitle(string title)
    {
        string cleaned = Regex.Replace(title, @"\s*\([^)]*\)", "").Trim();
        // drop trailing version fragments: "minecraft:icon v1.21.80"
        cleaned = Regex.Replace(cleaned, @"\s+v?\d+(\.\d+)*$", "", RegexOptions.IgnoreCase).Trim();

        string[] parts = cleaned.Split(':', 2, StringSplitOptions.TrimEntries);
        string componentId = parts.Length == 2 ? $"{parts[0]}:{parts[1].Split(' ')[0]}" : cleaned.Split(' ')[0];
        string namePart = componentId.Contains(':') ? componentId.Split(':')[1] : componentId;
        return (componentId, Formatting.SnakeToPascalCase(namePart));
    }

    /// <summary>
    /// Turn a schema title into a legal C# type name
    /// </summary>
    private static string SanitizeTypeName(string? title, string fallback)
    {
        if (string.IsNullOrWhiteSpace(title))
            return fallback;

        string t = Regex.Replace(title, @"\([^)]*\)", "").Trim();
        if (t.Contains(':'))
            t = t.Split(':').Last().Trim();

        string[] words = t.Split([' ', '-', '.'], StringSplitOptions.RemoveEmptyEntries);
        t = words.Length == 0 ? fallback : words[^1];

        if (t.Contains('_'))
            return Formatting.SnakeToPascalCase(t);

        // already PascalCase-ish
        if (t.Length == 0) return fallback;
        return char.ToUpperInvariant(t[0]) + t[1..];
    }

    public static string GenerateItemTraitFromSchema(string json, string ns)
    {
        // get rid of $refs for clean serialization
        json = JsonResolver.Resolve(json);

        ComponentSchema schema = JsonConvert.DeserializeObject<ComponentSchema>(json)!;
        return GenerateTraitFromSchema(schema, ns, TraitSystem.TraitType.Item);
    }

    public static string GenerateItemTraitFromSchema(ComponentSchema schema, string ns) =>
        GenerateTraitFromSchema(schema, ns, TraitSystem.TraitType.Item);

    public static string GenerateBlockTraitFromSchema(string json, string ns)
    {
        json = JsonResolver.Resolve(json);
        ComponentSchema schema = JsonConvert.DeserializeObject<ComponentSchema>(json)!;
        return GenerateTraitFromSchema(schema, ns, TraitSystem.TraitType.Block);
    }

    public static string GenerateBlockTraitFromSchema(ComponentSchema schema, string ns) =>
        GenerateTraitFromSchema(schema, ns, TraitSystem.TraitType.Block);

    public static string GenerateTraitFromSchema(ComponentSchema schema, string ns, TraitSystem.TraitType traitType)
    {
        (string componentId, string typeName) = ParseComponentTitle(schema.Component);

        string itemOrBlockInterface = traitType == TraitSystem.TraitType.Block
            ? "IBlockTrait"
            : "IItemTrait";

        TraitInterfaceBuilder iface = new(schema.Description, typeName, new(componentId), traitType,
            ParseFormatVersion(schema.FormatVer), ns,
            [
                "ingot.Core.Common",
                "ingot.Core.Common.SharedConstructs",
                "ingot.Core.TraitSystem",
                "ingot.Core.TraitSystem.Traits",
            ],
            itemOrBlockInterface);

        List<string> logs = new();

        // resolve root-level oneOf into a property bag
        Dictionary<string, SchemaNode> properties = ResolveRootProperties(schema, logs);

        string[] required = schema.Required ?? [];
        // if we fabricated properties from an object branch, inherit its required list when present
        if (schema.Properties is null && schema.OneOf is not null)
        {
            SchemaNode? objectBranch = schema.OneOf.FirstOrDefault(n => n.Properties is not null);
            if (objectBranch?.Required is not null)
                required = objectBranch.Required;
        }

        foreach ((string key, SchemaNode prop) in properties)
        {
            string name = Formatting.SnakeToPascalCase(key);
            ResolvedType resolved = ResolveType(prop, name, iface, logs, parentTypeName: typeName);

            List<TraitPropertyConstraintAttribute> constraints = BuildConstraints(prop, resolved, iface, name);

            // multi-primitive oneOf intentionally stays object (with a Type constraint)
            if (resolved.AllowedTypes is null &&
                (resolved.CsType is "object" or "object?" ||
                 resolved.CsType.StartsWith("object", StringComparison.Ordinal)))
                logs.Add($"log: unable to fully resolve type on property {name}" +
                         (prop.Title is null ? "" : $" (schema title: {prop.Title})"));

            bool requiredProp = IsRequired(prop, key, required);
            iface.AddProperty(prop.Description ?? "", name, requiredProp, resolved.CsType, prop.Default,
                constraints.ToArray());
        }

        iface.ExtraHeaders = logs.ToArray();
        return iface.Generate();
    }

    /// <summary>
    /// Root schemas are often <c>oneOf: [ primitive, { properties } ]</c>. Prefer the object
    /// form so we get named properties; fall back to a synthetic <c>value</c> property.
    /// </summary>
    private static Dictionary<string, SchemaNode> ResolveRootProperties(ComponentSchema schema, List<string> logs)
    {
        if (schema.Properties is not null)
            return schema.Properties;

        if (schema.OneOf is null || schema.OneOf.Length == 0)
        {
            logs.Add("log: schema has no properties and no oneOf - empty trait");
            return new();
        }

        // prefer object branches that declare properties (damage, max_stack_size, icon, ...)
        SchemaNode[] objectBranches = schema.OneOf.Where(n => n.Properties is not null).ToArray();
        if (objectBranches.Length > 0)
        {
            // if multiple object shapes exist, take the richest (most properties)
            SchemaNode best = objectBranches.OrderByDescending(n => n.Properties!.Count).First();
            logs.Add($"log: resolved root oneOf -> object form ({best.Properties!.Count} properties)");
            return best.Properties!;
        }

        // pure primitive (or multi-primitive) oneOf, e.g. interact_button: boolean | string
        // keep the original oneOf on the synthetic property so Type constraints are emitted
        logs.Add("log: resolved root oneOf -> synthetic value property");
        return new()
        {
            ["value"] = new SchemaNode
            {
                Description = schema.Description ?? "Value of the component",
                OneOf = schema.OneOf,
            }
        };
    }

    private record ResolvedType(
        string CsType,
        string? SchemaTypeHint = null,
        string[]? Enum = null,
        string? EnumTitle = null,
        // json schema type names for multi-primitive oneOf -> Constraint.Type
        string[]? AllowedTypes = null);

    /// <summary>
    /// Resolve a schema node to a C# type, generating subtypes when the node is a structured object.
    /// </summary>
    private static ResolvedType ResolveType(SchemaNode node, string propName, TraitInterfaceBuilder iface,
        List<string> logs, string? parentTypeName)
    {
        // flatten oneOf first
        if (node.OneOf is { Length: > 0 })
            return ResolveOneOfAsType(node.OneOf, node.Title, propName, iface, logs, parentTypeName);

        // known title shortcuts (Range, ItemTypeDescriptor, BlockTypeDescriptor, ...)
        if (node.Title is not null && KnownTitles.TryGetValue(node.Title, out string? known))
            return new(known, node.Type, node.Enum, node.Title);

        // min/max-only objects are SharedConstructs.Range even without a title
        if (node.Properties is not null && IsMinMaxRangeShape(node.Properties))
            return new("Range", "object");

        // object with fixed properties -> subtype (or known type)
        if (node.Properties is not null &&
            (node.Type is null or "object" || node.Type == "object"))
        {
            if (node.Title is not null && KnownTitles.TryGetValue(node.Title, out string? knownObj))
                return new(knownObj, "object");

            string subtypeName = SanitizeTypeName(node.Title, propName);
            // avoid clashing with the trait interface name
            if (parentTypeName is not null &&
                subtypeName.Equals(parentTypeName, StringComparison.OrdinalIgnoreCase))
                subtypeName = parentTypeName + "Data";

            string generated = EnsureSubtype(subtypeName, node, iface, logs, parentTypeName);
            return new(generated, "object");
        }

        // free-form object map
        if (node.Type == "object" && node.AdditionalProperties is not null)
        {
            ResolvedType valueType = ResolveType(node.AdditionalProperties, propName + "Value", iface, logs,
                parentTypeName);
            if (valueType.CsType is "string" or "int" or "float" or "bool")
                return new($"Dictionary<string, {valueType.CsType}>", "object");
            return new("object", "object");
        }

        if (node.Type == "object")
            return new("object", "object");

        // arrays
        if (node.Type == "array")
        {
            if (node.Items is null)
                return new("object[]", "array");

            ResolvedType item = ResolveType(node.Items, Singularize(propName), iface, logs, parentTypeName);
            // strip trailing ? from element types so we get object[] not object?[]
            string element = item.CsType.TrimEnd('?');
            return new($"{element}[]", "array", item.Enum, item.EnumTitle);
        }

        // enum on a plain string
        if (node.Enum is not null)
            return new("string", "string", node.Enum, node.Title ?? propName);

        return new(ConvertPrimitive(node.Type), node.Type, node.Enum, node.Title);
    }

    /// <summary>
    /// Collapse a oneOf list into a single C# type. Heuristics (in order):
    /// <list type="number">
    /// <item>known titles (Item Descriptor, IntRange, ...)</item>
    /// <item>string | free-form object → string/Identifier (descriptor pattern)</item>
    /// <item>array | structured object → structured object (prefer named fields)</item>
    /// <item>structured object present → generate/use that subtype</item>
    /// <item>single primitive → that primitive</item>
    /// <item>multiple primitives → object (with Type constraint)</item>
    /// </list>
    /// </summary>
    private static ResolvedType ResolveOneOfAsType(SchemaNode[] branches, string? title, string propName,
        TraitInterfaceBuilder? builder, List<string> logs, string? parentTypeName)
    {
        if (title is not null && KnownTitles.TryGetValue(title, out string? known))
            return new(known, null);

        // expand nested oneOf branches (rare, but molang does this)
        List<SchemaNode> flat = new();
        foreach (SchemaNode branch in branches)
        {
            if (branch.OneOf is { Length: > 0 })
                flat.AddRange(branch.OneOf);
            else
                flat.Add(branch);
        }

        SchemaNode[] objectBranches = flat.Where(b => b.Properties is not null).ToArray();
        SchemaNode[] arrayBranches = flat.Where(b => b.Type == "array").ToArray();
        SchemaNode[] primitiveBranches = flat
            .Where(b => b.Properties is null && b.Type is "string" or "integer" or "number" or "boolean")
            .ToArray();
        bool hasFreeFormObject = flat.Any(b =>
            b.Type == "object" && b.Properties is null && b.AdditionalProperties is not null);

        // descriptor pattern: string | { additionalProperties } -> SharedConstructs descriptor
        if (primitiveBranches.Any(b => b.Type == "string") && hasFreeFormObject && objectBranches.Length == 0)
        {
            if (title is not null && title.Contains("Item", StringComparison.OrdinalIgnoreCase))
                return new("ItemTypeDescriptor", "string");
            if (title is not null && title.Contains("Block", StringComparison.OrdinalIgnoreCase))
                return new("BlockTypeDescriptor", "string");
            return new("string", "string");
        }

        // string | structured object with properties - prefer string for simple dual forms
        // only when the object is a descriptor-like alternate, not when object is the main payload
        // (root icon is handled at ResolveRootProperties which prefers object)

        // array | object with properties -> prefer object (Repair entries)
        if (objectBranches.Length > 0 && arrayBranches.Length > 0 && builder is not null)
        {
            SchemaNode best = PickRichestObject(objectBranches, title);
            return ResolveType(best, propName, builder, logs, parentTypeName);
        }

        // pure structured object branch(es)
        if (objectBranches.Length > 0 && builder is not null)
        {
            SchemaNode best = PickRichestObject(objectBranches, title);
            // if there's also a string primitive, still prefer object when it has real fields
            // (caller at root already peeled object forms off)
            return ResolveType(best, propName, builder, logs, parentTypeName);
        }

        // array only
        if (arrayBranches.Length > 0 && builder is not null)
            return ResolveType(arrayBranches[0], propName, builder, logs, parentTypeName);

        // primitives only
        string[] primTypes = primitiveBranches.Select(b => b.Type!).Distinct().ToArray();
        if (primTypes.Length == 1)
        {
            // carry enum if any branch has one
            string[]? enumVals = flat.FirstOrDefault(b => b.Enum is not null)?.Enum;
            string? enumTitle = flat.FirstOrDefault(b => b.Enum is not null)?.Title;
            return new(ConvertPrimitive(primTypes[0]), primTypes[0], enumVals, enumTitle ?? title);
        }

        if (primTypes.Length > 1)
        {
            // boolean | string (interact_button) and similar - too wide for a single CLR type;
            // keep object and emit a Type constraint so reflection can reject wrong CLR types
            logs.Add(
                $"log: multi-primitive oneOf on {propName} [{string.Join('|', primTypes)}] -> object + Type constraint");
            return new("object", AllowedTypes: primTypes);
        }

        // last resort: first branch
        if (flat.Count > 0 && builder is not null)
            return ResolveType(flat[0], propName, builder, logs, parentTypeName);

        logs.Add($"log: unresolved oneOf on {propName}");
        return new("object", null);
    }

    /// <summary>
    /// Prefer the object branch with the most properties; carry the parent oneOf title
    /// down so generated subtypes get a stable name (e.g. Repair).
    /// </summary>
    private static SchemaNode PickRichestObject(SchemaNode[] objectBranches, string? parentTitle)
    {
        SchemaNode best = objectBranches.OrderByDescending(b => b.Properties!.Count).First();
        if (best.Title is null && parentTitle is not null)
            best.Title = parentTitle;
        return best;
    }

    /// <summary>
    /// True when the object is only a min/max pair (IntRange / FloatRange schema shape).
    /// </summary>
    private static bool IsMinMaxRangeShape(Dictionary<string, SchemaNode> properties)
    {
        if (properties.Count is < 1 or > 2)
            return false;

        string[] keys = properties.Keys.Select(k => k.ToLowerInvariant()).OrderBy(k => k).ToArray();
        if (keys is not (["max", "min"] or ["max"] or ["min"]))
            return false;

        return properties.Values.All(p =>
            p.Type is null or "number" or "integer"
            && p.Properties is null
            && p.OneOf is null);
    }

    private static string EnsureSubtype(string name, SchemaNode node, TraitInterfaceBuilder iface,
        List<string> logs, string? parentTypeName)
    {
        if (iface.HasSubtype(name))
            return name;

        string[] required = node.Required ?? [];
        List<TraitInterfaceBuilder.SubtypeField> fields = new();

        foreach ((string key, SchemaNode prop) in node.Properties!)
        {
            string fieldName = Formatting.SnakeToPascalCase(key);
            ResolvedType fieldType = ResolveType(prop, fieldName, iface, logs, parentTypeName ?? name);

            if (prop.Enum is not null)
                iface.Enum(prop.Title ?? fieldName, prop.Enum);

            bool isRequired = required.Contains(key);

            fields.Add(new TraitInterfaceBuilder.SubtypeField(
                prop.Description ?? "",
                fieldName,
                fieldType.CsType,
                isRequired,
                prop.Default));
        }

        iface.AddSubtype(node.Description ?? node.Title ?? name, name, fields);
        logs.Add($"log: generated subtype {name} ({fields.Count} fields)");
        return name;
    }

    private static List<TraitPropertyConstraintAttribute> BuildConstraints(SchemaNode prop,
        ResolvedType resolved, TraitInterfaceBuilder iface, string name)
    {
        List<TraitPropertyConstraintAttribute> constraints = new();

        // multi-primitive oneOf -> [TraitPropertyConstraint(Type, "boolean", "string")]
        if (resolved.AllowedTypes is { Length: > 0 })
        {
            constraints.Add(new TraitPropertyConstraintAttribute(
                TraitPropertyConstraintAttribute.Constraint.Type,
                resolved.AllowedTypes.Cast<object>().ToArray()));
        }

        string[]? enumVals = prop.Enum ?? resolved.Enum;
        if (enumVals is not null)
        {
            iface.Enum(prop.Title ?? resolved.EnumTitle ?? name, enumVals);
            constraints.Add(new(TraitPropertyConstraintAttribute.Constraint.OneOf, enumVals));
        }
        else if (prop is { Type: "integer", Max: not null, Min: not null } ||
                 resolved.SchemaTypeHint is "integer" && prop is { Max: not null, Min: not null })
            constraints.Add(new(TraitPropertyConstraintAttribute.Constraint.Range, prop.Min!, prop.Max!));
        else if (prop is { Type: "integer", Min: not null } ||
                 prop is { Type: null, Min: not null, Max: null } && resolved.CsType == "int")
            constraints.Add(new(TraitPropertyConstraintAttribute.Constraint.GreaterThanEq, prop.Min!));
        else if (prop is { Type: "integer", Max: not null })
            constraints.Add(new(TraitPropertyConstraintAttribute.Constraint.LessThanEq, prop.Max!));
        else if (prop is { Type: "number", Min: not null } && prop.Max is null)
            constraints.Add(new(TraitPropertyConstraintAttribute.Constraint.GreaterThanEq, prop.Min!));
        else if (prop is { Type: "number", Max: not null, Min: not null })
            constraints.Add(new(TraitPropertyConstraintAttribute.Constraint.Range, prop.Min!, prop.Max!));

        return constraints;
    }

    private static bool IsRequired(SchemaNode prop, string key, string[] required)
    {
        if (required.Contains(key))
            return true;

        // structured / collection properties are optional unless explicitly required -
        // bedrock omits them when unset, even if the schema forgot a default
        bool structured = prop.Properties is not null
                          || prop.Type is "object" or "array"
                          || prop.Items is not null
                          || prop.OneOf is { Length: > 0 } && prop.OneOf.Any(b => b.Properties is not null);
        if (structured)
            return false;

        // match previous generator behaviour for primitives: no default (or empty string) => required
        if (prop.Default is null)
            return true;
        if (prop.Default is string s && s == "")
            return true;
        return false;
    }

    private static string Singularize(string name)
    {
        if (name.EndsWith("ies", StringComparison.Ordinal))
            return name[..^3] + "y";
        if (name.EndsWith("ses", StringComparison.Ordinal))
            return name[..^2];
        if (name.EndsWith("s", StringComparison.Ordinal) && name.Length > 1)
            return name[..^1];
        return name + "Item";
    }

    public static async Task GenerateItemTraits(string outputDir, string token)
    {
        await GenerateTraits(outputDir, token, "metadata/json_schemas/server/item_components",
            "ingot.Core.TraitSystem.Traits.Item", TraitSystem.TraitType.Item);
    }

    public static async Task GenerateBlockTraits(string outputDir, string token)
    {
        await GenerateTraits(outputDir, token, "metadata/json_schemas/server/block_components",
            "ingot.Core.TraitSystem.Traits.Block", TraitSystem.TraitType.Block);
    }

    private static async Task GenerateTraits(string outputDir, string token, string repoPath, string ns,
        TraitSystem.TraitType traitType)
    {
        string treeJson = await RepoTreeCrawler.GetTree("mojang", "bedrock-samples", repoPath, token: token);
        (string path, string content)[] files = await RepoTreeCrawler.GetFileContentsWithPaths(treeJson, token);

        // keep newest schema per component id (Storage.json vs "Storage Item.json" are the same trait)
        (string path, string content, ComponentSchema schema)[] latest = SelectLatestComponents(files);

        if (Directory.Exists(outputDir))
            Directory.Delete(outputDir, true);
        Directory.CreateDirectory(outputDir);

        int c = 0;
        foreach ((string path, string content, ComponentSchema schema) in latest)
        {
            c++;
            string iface = GenerateTraitFromSchema(schema, ns, traitType);
            // prefer the minecraft:name for the file so renames don't produce IStorage.cs + IStorageItem.cs
            (string componentId, string typeName) = ParseComponentTitle(schema.Component);
            string fileName = $"I{typeName}.cs";

            Console.WriteLine($"({c}/{latest.Length}) generated {fileName} from {path}");
            await File.WriteAllTextAsync(Path.Combine(outputDir, fileName), iface);
        }
    }

    /// <summary>
    /// Schemas live under versioned folders and occasionally get renamed
    /// (<c>Storage Item.json</c> → <c>Storage.json</c>). Resolve $refs, drop non-components,
    /// and keep the newest entry per component identifier.
    /// </summary>
    private static (string path, string content, ComponentSchema schema)[] SelectLatestComponents(
        (string path, string content)[] files)
    {
        List<(string path, string content, ComponentSchema schema, VersionKey ver)> candidates = new();

        foreach ((string path, string content) in files)
        {
            string resolved;
            try
            {
                resolved = JsonResolver.Resolve(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"skipped {path} - $ref resolve failed: {ex.Message}");
                continue;
            }

            ComponentSchema? schema;
            try
            {
                schema = JsonConvert.DeserializeObject<ComponentSchema>(resolved);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"skipped {path} - deserialize failed: {ex.Message}");
                continue;
            }

            if (schema is null || !IsComponentSchema(schema.Component))
            {
                Console.WriteLine($"skipped {schema?.Component ?? path} - not a component");
                continue;
            }

            candidates.Add((path, resolved, schema, ParseVersionKey(path)));
        }

        return candidates
            .GroupBy(c => ParseComponentTitle(c.schema.Component).ComponentId, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderByDescending(c => c.ver).First())
            .OrderBy(c => c.path, StringComparer.OrdinalIgnoreCase)
            .Select(c => (c.path, c.content, c.schema))
            .ToArray();
    }

    private static VersionKey ParseVersionKey(string path)
    {
        string[] parts = path.Replace('\\', '/').Split('/');
        // path relative to component root: "1.20.50/Food.json" or just "Food.json"
        string? ver = parts.Length >= 2 ? parts[^2] : null;
        return new VersionKey(ver);
    }

    private static Version ParseFormatVersion(string? raw)
    {
        // beta schemas track unreleased APIs - treat as a high 1.x so stable packs still compile
        if (string.IsNullOrWhiteSpace(raw) || raw.Equals("beta", StringComparison.OrdinalIgnoreCase))
            return new Version(1, 21, 0);

        try
        {
            return new Version(raw);
        }
        catch
        {
            return new Version(0, 0, 0);
        }
    }

    /// <summary>
    /// Component schemas have titles that are a single minecraft identifier,
    /// optionally with a version suffix or beta marker. Definition helper schemas
    /// usually have multi-word titles like <c>minecraft:foo bar_baz</c>.
    /// </summary>
    private static bool IsComponentSchema(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;

        // strip (beta) / (experimental) markers
        string cleaned = Regex.Replace(title, @"\s*\([^)]*\)", "").Trim();
        // strip trailing version: "v1.21.80" or "1.21.80"
        cleaned = Regex.Replace(cleaned, @"\s+v?\d+(\.\d+)*$", "", RegexOptions.IgnoreCase).Trim();

        return Regex.IsMatch(cleaned, @"^minecraft:[a-z0-9_]+$", RegexOptions.IgnoreCase);
    }

    private readonly record struct VersionKey(string? Raw) : IComparable<VersionKey>
    {
        public int CompareTo(VersionKey other)
        {
            if (Raw is null && other.Raw is null) return 0;
            if (Raw is null) return -1;
            if (other.Raw is null) return 1;
            if (Raw.Equals("beta", StringComparison.OrdinalIgnoreCase)) return 1;
            if (other.Raw.Equals("beta", StringComparison.OrdinalIgnoreCase)) return -1;

            try
            {
                return new Version(Raw).CompareTo(new Version(other.Raw));
            }
            catch
            {
                return string.Compare(Raw, other.Raw, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
