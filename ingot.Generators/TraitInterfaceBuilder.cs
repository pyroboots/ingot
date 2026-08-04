using ingot.Core.Common;
using ingot.Core.TraitSystem;

using Newtonsoft.Json.Linq;

using Version = ingot.Core.Common.Version;

namespace ingot.Generators;

public class TraitInterfaceBuilder
{
    private readonly string[] _usings;
    private readonly string _baseInterface;
    public string Description;
    public string Name;
    public Identifier Component;
    public TraitSystem.TraitType Type;
    public Version FormatVersion;
    public string Namespace;
    public string[] ExtraHeaders = [];

    /// <param name="description">Description of the component</param>
    /// <param name="name">Name of interface without the "I" prefix</param>
    /// <param name="componentId">Identifier of the component this interface is for</param>
    /// <param name="type">Type of trait interface</param>
    /// <param name="formatVer">Minimum format version for this trait</param>
    /// <param name="ns">Namespace</param>
    /// <param name="usings">Array of usings to insert</param>
    /// <param name="baseInterface">Interface this trait extends (IItemTrait / IBlockTrait)</param>
    public TraitInterfaceBuilder(string description, string name, Identifier componentId, TraitSystem.TraitType type,
        Version formatVer, string ns, string[]? usings = null, string baseInterface = "IItemTrait")
    {
        _usings = usings ?? [];
        _baseInterface = baseInterface;
        Description = description;
        Name = name;
        Component = componentId;
        Type = type;
        FormatVersion = formatVer;
        Namespace = ns;
    }

    private readonly List<InterfaceProperty> _properties = new();
    private readonly Dictionary<string, string[]> _enums = new();
    private readonly List<SubtypeDefinition> _subtypes = new();
    private readonly HashSet<string> _subtypeNames = new(StringComparer.Ordinal);

    private record InterfaceProperty(string Description, string Name, bool Required, string Type, object? DefaultValue,
        TraitPropertyConstraintAttribute[] Constraints, TraitPropertyWarningAttribute[] Warnings);

    public record SubtypeField(string Description, string Name, string Type, bool Required, object? DefaultValue);

    private record SubtypeDefinition(string Description, string Name, List<SubtypeField> Fields);

    public void AddProperty(string desc, string name, bool required, string type, object? defaultValue = null,
        TraitPropertyConstraintAttribute[]? constraints = null, TraitPropertyWarningAttribute[]? warnings = null)
        => _properties.Add(new(desc, name, required, type, defaultValue, constraints ?? [], warnings ?? []));

    public bool HasSubtype(string name) => _subtypeNames.Contains(name);

    public void AddSubtype(string description, string name, List<SubtypeField> fields)
    {
        if (!_subtypeNames.Add(name))
            return;
        _subtypes.Add(new(description, name, fields));
    }

    public void Enum(string name, params string[] values)
    {
        if (_enums.ContainsKey(name)) return;

        string[] sanitisedValues = values.Select(v => v.Replace(".", "_")).ToArray();
        _enums.Add(name, sanitisedValues);
    }

    // in case things ever get renamed, generator stays up to date
    private static string TraitAttribute => nameof(Core.TraitSystem.TraitAttribute);
    private static string FmtVerAttribute => nameof(TraitFormatVersionAttribute);
    private static string TraitPropertyAttribute => nameof(Core.TraitSystem.TraitPropertyAttribute);
    private static string TraitPropertyConstraintAttribute => nameof(Core.TraitSystem.TraitPropertyConstraintAttribute);
    private static string TraitPropertyWarningAttribute => nameof(Core.TraitSystem.TraitPropertyWarningAttribute);

    private string FormatValue(object? value, string type)
    {
        string bare = type.TrimEnd('?');
        if (value == null) return "null";
        if (bare is "object" or "dynamic") return "null";
        // SharedConstructs + common reference types default to null when optional
        if (bare is "Range" or "IntRange" or "FloatRange"
            or "Identifier" or "ItemTypeDescriptor" or "BlockTypeDescriptor"
            or "BlockPermutationDescriptor" or "ItemTagsDescriptor" or "Fraction"
            || _subtypeNames.Contains(bare))
            return "null";
        if (bare.StartsWith("Dictionary<", StringComparison.Ordinal))
            return "null";

        // json arrays may arrive as JArray, object[], List<object>, long[], etc.
        if (TryAsObjectList(value, out List<object?>? list) && list is not null)
        {
            // only emit a collection expression when the C# type is actually an array
            if (bare.EndsWith("[]", StringComparison.Ordinal))
                return FormatArrayLiteral(list, bare[..^2]);

            // Color255RGB-style: schema default is [r,g,b] but property is exposed as string
            if (bare == "string" && TryFormatRgbHex(list, out string hex))
                return hex;

            // default shape doesn't match the property type - treat as unset
            return "null";
        }

        if (value is JObject)
            return "null";

        // json likes int64/double
        if (bare == "float" && value is IConvertible && value is not string)
            return $"{Convert.ToSingle(value)}f";
        if (bare == "int" && value is IConvertible && value is not string)
            return Convert.ToInt32(value).ToString();
        if (bare == "bool" && value is IConvertible && value is not string)
            return Convert.ToBoolean(value) ? "true" : "false";

        Type t = value.GetType();
        if (t == typeof(float)) return $"{value}f";
        if (t == typeof(double)) return $"{(float)(double)value}f";
        if (t == typeof(string)) return $"\"{value.ToString()!.ToLower()}\"";
        if (t == typeof(bool)) return value.ToString()!.ToLower();
        if (t == typeof(Int64)) return value.ToString()!;
        if (t == typeof(int)) return value.ToString()!;

        // last resort for array-typed properties
        if (bare.EndsWith("[]", StringComparison.Ordinal))
            return "[]";

        throw new Exception($"could not evaluate type {t.FullName} for property type {type}");
    }
    
    private static bool TryAsObjectList(object value, out List<object?>? list)
    {
        switch (value)
        {
            case JArray jArray:
                list = jArray.Select(t => (object?)t).ToList();
                return true;
            case string:
                list = null;
                return false;
            case System.Collections.IEnumerable enumerable and not JObject:
            {
                list = new List<object?>();
                foreach (object? item in enumerable)
                    list.Add(item);
                return true;
            }
            default:
                list = null;
                return false;
        }
    }

    private static object? UnwrapToken(object? value) =>
        value is JValue jv ? jv.Value : value;
    
    private static string FormatArrayLiteral(List<object?> items, string elementType)
    {
        if (items.Count == 0)
            return "[]";

        try
        {
            string[] parts = new string[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                object? item = UnwrapToken(items[i]);
                parts[i] = elementType switch
                {
                    "int" => Convert.ToInt32(item).ToString(),
                    "float" => $"{Convert.ToSingle(item)}f",
                    "bool" => Convert.ToBoolean(item) ? "true" : "false",
                    "string" => $"\"{item?.ToString()!.ToLower()}\"",
                    _ => throw new InvalidOperationException(),
                };
            }

            return $"[{string.Join(", ", parts)}]";
        }
        catch
        {
            return "[]";
        }
    }
    
    private static bool TryFormatRgbHex(List<object?> items, out string literal)
    {
        literal = "null";
        if (items.Count != 3)
            return false;

        try
        {
            int r = Convert.ToInt32(UnwrapToken(items[0]));
            int g = Convert.ToInt32(UnwrapToken(items[1]));
            int b = Convert.ToInt32(UnwrapToken(items[2]));
            if (r is < 0 or > 255 || g is < 0 or > 255 || b is < 0 or > 255)
                return false;
            literal = $"\"#{r:x2}{g:x2}{b:x2}\"";
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private static string NullableIfNeeded(string type, bool required)
    {
        if (required) return type;
        if (type is "int" or "float" or "bool") return type;
        if (type.EndsWith("[]", StringComparison.Ordinal)) return type;
        if (type.EndsWith('?')) return type;
        // object / reference types get ?
        return type + "?";
    }

    private static string EscapeXml(string? text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text
            .Replace("&", "&amp;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\n", " ")
            .Replace("\r", "");
    }

    public string Generate()
    {
        StringWriter sw = new();

        sw.WriteLine("// autogenerated by ingot");
        foreach (string header in ExtraHeaders)
            sw.WriteLine($"// {header}");
        sw.WriteLine();
        sw.WriteLine($"namespace {Namespace};");
        foreach (string import in _usings)
            sw.WriteLine($"using {import};");
        if (_subtypes.Count > 0)
        {
            sw.WriteLine("using Newtonsoft.Json;");
            sw.WriteLine("using Formatting = ingot.Core.Common.Formatting;");
        }

        sw.WriteLine();
        sw.WriteLine("/// <summary>");
        sw.WriteLine($"/// {EscapeXml(Description)}");
        sw.WriteLine("/// </summary>");
        sw.WriteLine(
            $"[{TraitAttribute.Replace("Attribute", "")}(\"{Component}\", TraitSystem.TraitType.{System.Enum.GetName(Type)})]");
        sw.WriteLine($"[{FmtVerAttribute.Replace("Attribute", "")}(\"{FormatVersion}\")]");
        sw.WriteLine($"public interface I{Name} : {_baseInterface}");
        sw.WriteLine("{");

        foreach (var enumName in _enums)
        foreach (string enumValue in enumName.Value)
        {
            sw.WriteLine($"    /// <summary>Equivalent to <c>{enumValue}</c></summary>");
            sw.WriteLine(
                $"    public const string {enumName.Key}_{Formatting.SnakeToPascalCase(enumValue)} = \"{enumValue}\";");
        }

        foreach (InterfaceProperty prop in _properties)
        {
            sw.WriteLine("    /// <summary>");
            sw.WriteLine($"    /// {EscapeXml(prop.Description)}");
            sw.WriteLine("    /// </summary>");

            sw.WriteLine($"    [{TraitPropertyAttribute.Replace("Attribute", "")}]");
            foreach (TraitPropertyConstraintAttribute constraint in prop.Constraints)
            {
                string values = string.Join(',',
                    constraint.Values.Select(v => v is string ? $"\"{v}\"" : v.ToString()));

                // [TraitPropertyConstraint(TraitPropertyConstraint.Constraint.OneOf, "use", "attack")]
                sw.WriteLine(
                    $"    [{TraitPropertyConstraintAttribute.Replace("Attribute", "")}({TraitPropertyConstraintAttribute}.Constraint.{System.Enum.GetName(constraint.Operation)}, {values})]");
            }

            foreach (TraitPropertyWarningAttribute warning in prop.Warnings)
            {
                string values = string.Join(',',
                    warning.Values.Select(v => v is string ? $"\"{v}\"" : v.ToString()));

                // [TraitPropertyWarning("warning", TraitPropertyConstraint.Constraint.Equal, 1)]
                sw.WriteLine(
                    $"    [{TraitPropertyWarningAttribute.Replace("Attribute", "")}(\"{warning.Warning}\", {TraitPropertyConstraintAttribute}.Constraint.{System.Enum.GetName(warning.Operation)}, {values})]");
            }

            string emitType = NullableIfNeeded(prop.Type, prop.Required);
            // public abstract float Duration { get; }
            if (prop.Required) sw.WriteLine($"    public abstract {emitType} {prop.Name} {{ get; }}");
            // public virtual object? Type => null;
            else sw.WriteLine($"    public virtual {emitType} {prop.Name} => {FormatValue(prop.DefaultValue, prop.Type)};");
        }

        sw.WriteLine("}");

        // nested object shapes from the schema emitted as ICompilableFragment classes
        foreach (SubtypeDefinition subtype in _subtypes)
        {
            sw.WriteLine();
            sw.WriteLine("/// <summary>");
            sw.WriteLine($"/// {EscapeXml(subtype.Description)}");
            sw.WriteLine("/// </summary>");
            sw.WriteLine($"public class {subtype.Name} : ICompilableFragment");
            sw.WriteLine("{");

            foreach (SubtypeField field in subtype.Fields)
            {
                if (!string.IsNullOrWhiteSpace(field.Description))
                {
                    sw.WriteLine("    /// <summary>");
                    sw.WriteLine($"    /// {EscapeXml(field.Description)}");
                    sw.WriteLine("    /// </summary>");
                }

                if (field.Required)
                    sw.WriteLine($"    public required {field.Type} {field.Name};");
                else
                {
                    string fieldType = NullableIfNeeded(field.Type, required: false);
                    string defaultExpr = FormatSubtypeDefault(field);
                    sw.WriteLine($"    public {fieldType} {field.Name}{defaultExpr};");
                }
            }

            sw.WriteLine();
            sw.WriteLine("    /// <inheritdoc/>");
            sw.WriteLine("    public void Compile(ref JsonTextWriter writer)");
            sw.WriteLine("    {");
            sw.WriteLine("        JsonHelper json = new(ref writer);");
            sw.WriteLine("        json.Object(\"\", () =>");
            sw.WriteLine("        {");
            foreach (SubtypeField field in subtype.Fields)
            {
                // PascalToSnakeCase so Block -> block, SearchInventory -> search_inventory
                sw.WriteLine(
                    $"            json.Property(Formatting.PascalToSnakeCase(nameof({field.Name})), {field.Name});");
            }

            sw.WriteLine("        });");
            sw.WriteLine("    }");
            sw.WriteLine("}");
        }

        string s = sw.ToString();
        sw.Dispose();
        return s;
    }

    private string FormatSubtypeDefault(SubtypeField field)
    {
        // ref / complex -> leave uninitialized only if required - optionals get defaults
        if (field.DefaultValue is null)
        {
            return field.Type switch
            {
                "bool" => " = false",
                "int" => " = 0",
                "float" => " = 0f",
                "string" => " = \"\"",
                "object" or "object?" or "dynamic" => " = null",
                _ when field.Type.EndsWith("[]", StringComparison.Ordinal) => " = []",
                _ => " = null!",
            };
        }

        try
        {
            return $" = {FormatValue(field.DefaultValue, field.Type)}";
        }
        catch
        {
            return " = null!";
        }
    }
}
