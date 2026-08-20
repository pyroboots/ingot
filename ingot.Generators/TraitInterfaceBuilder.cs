using ingot.Core.Common;
using ingot.Core.TraitSystem;

using Newtonsoft.Json.Linq;

using Version = System.Version;

namespace ingot.Generators;

public class TraitInterfaceBuilder
{
    private readonly string[] _usings;
    public string Description;
    public string Name;
    public Identifier Component;
    public TraitSystem.TraitType Type;
    public Version FormatVersion;
    public string Namespace;
    public string[] ExtraHeaders = [];
    public string TraitInterface;

    /// <param name="description">Description of the component</param>
    /// <param name="name">Name of interface without the "I" prefix</param>
    /// <param name="componentId">Identifier of the component this interface is for</param>
    /// <param name="type">Type of trait interface</param>
    /// <param name="formatVer">Minimum format version for this trait</param>
    /// <param name="traitInterface">The marker interface for the trait type</param>
    /// <param name="ns">Namespace</param>
    /// <param name="usings">Array of usings to insert</param>
    public TraitInterfaceBuilder(string description, string name, Identifier componentId, TraitSystem.TraitType type,
        Version formatVer, string traitInterface, string ns, string[]? usings = null)
    {
        _usings = usings ?? [];
        Description = description;
        Name = name;
        Component = componentId;
        Type = type;
        FormatVersion = formatVer;
        Namespace = ns;
        TraitInterface = traitInterface;
    }

    private readonly List<InterfaceProperty> _properties = new();
    private readonly Dictionary<string, string[]> _enums = new();
    private readonly Dictionary<string, StructProperty[]> _structs = new();
    
    private record InterfaceProperty(string Description, string Name, bool Required, string Type, object? DefaultValue, 
        TraitPropertyConstraintAttribute[] Constraints, TraitPropertyWarningAttribute[] Warnings);
    public void AddProperty(string desc, string name, bool required, string type, object? defaultValue = null,
        TraitPropertyConstraintAttribute[]? constraints = null, TraitPropertyWarningAttribute[]? warnings = null) 
        => _properties.Add(new(desc, name, required, type, defaultValue, constraints ?? [], warnings ?? []));

    public void Enum(string name, params string[] values)
    {
        if (_enums.ContainsKey(name)) return;

        string[] sanitisedValues = values.Select((v) => v.Replace(".", "_")).ToArray();
        _enums.Add(name, sanitisedValues);
    }

    
    public record StructProperty(string Name, string Description, string Type, bool Required, string? JsonName = null);
    public void Struct(string name, StructProperty[] props)
    {
        if (_structs.ContainsKey(name)) return;
        _structs.Add(name, props);
    }
    
    // in case things ever get renamed, generator stays up to date
    private static string TraitAttribute => nameof(Core.TraitSystem.TraitAttribute);
    private static string FmtVerAttribute => nameof(TraitFormatVersionAttribute);
    private static string TraitPropertyAttribute => nameof(Core.TraitSystem.TraitPropertyAttribute);
    private static string TraitPropertyConstraintAttribute => nameof(Core.TraitSystem.TraitPropertyConstraintAttribute);
    private static string TraitPropertyWarningAttribute => nameof(Core.TraitSystem.TraitPropertyConstraintAttribute);

    private string FormatValue(object? value, string type)
    {
        if (value == null) return "null";
        if (type == "dynamic") return "null";
        
        Type t = value.GetType();
        if (t == typeof(float)) return $"{value}f";
        if (t == typeof(double)) return $"{value}f";
        if (t == typeof(string)) return $"\"{value.ToString()!.ToLower()}\"";
        if (t == typeof(bool)) return value.ToString()!.ToLower();
        if (t == typeof(Int64) || t == typeof(int)) return value.ToString()!;
        if (t == typeof(JArray))
        {
            // `[]` is a collection expression - fine for T[] (including Either<...>[]) but
            // not for a non-array Either<string, int[]>
            if (type.StartsWith("Either", StringComparison.Ordinal) && type.EndsWith("[]") == false)
                return FormatEitherArray((JArray)value, type);
            return "[]";
        }
        if (t == typeof(JObject)) return "null";

        throw new Exception($"could not evaluate type {t.FullName}");
    }

    private static string FormatEitherArray(JArray array, string type)
    {
        if (array.Count == 0) return "null";

        JTokenType first = array[0].Type;
        if (first is JTokenType.Integer or JTokenType.Float)
        {
            if (type.Contains("float[]") || first == JTokenType.Float)
                return $"new float[] {{ {string.Join("f, ", array.Select(t => t.ToString()))}f }}";
            return $"new int[] {{ {string.Join(", ", array.Select(t => t.ToString()))} }}";
        }

        return first switch
        {
            JTokenType.String => $"new string[] {{ {string.Join(", ", array.Select(t => $"\"{t}\""))} }}",
            JTokenType.Boolean => $"new bool[] {{ {string.Join(", ", array.Select(t => t.ToString()!.ToLower()))} }}",
            _ => "null"
        };
    }

    public const string AutogeneratedWatermark = "// autogenerated by ingot";
    
    public string Generate()
    {
        StringWriter sw = new();
        
        sw.WriteLine(AutogeneratedWatermark);
        foreach (string header in ExtraHeaders)
            sw.WriteLine($"// {header}");
        sw.WriteLine();
        sw.WriteLine($"namespace {Namespace};");
        foreach (string import in _usings)
            sw.WriteLine($"using {import};");
        sw.WriteLine();
        sw.WriteLine("/// <summary>");
        sw.WriteLine($"/// {(Description is null ? "" : Description.Replace("\n", ""))}");
        sw.WriteLine("/// </summary>");
        sw.WriteLine($"[{TraitAttribute.Replace("Attribute", "")}(\"{Component}\", TraitSystem.TraitType.{System.Enum.GetName(Type)})]");
        sw.WriteLine($"[{FmtVerAttribute.Replace("Attribute", "")}(\"{FormatVersion}\")]");
        sw.WriteLine($"public interface I{Name} : {TraitInterface}");
        sw.WriteLine("{");

        foreach (var structure in _structs)
        {
            string structName = structure.Key;
            
            sw.WriteLine("    /// <summary>");
            sw.WriteLine($"    /// Representation of the {structName} JSON object");
            sw.WriteLine("    /// </summary>");
            sw.WriteLine($"    public struct {structName}");
            sw.WriteLine("    {");
            foreach (StructProperty prop in structure.Value)
            {
                sw.WriteLine("        /// <summary>");
                sw.WriteLine($"        /// {prop.Description}");
                sw.WriteLine("        /// </summary>");
                if (prop.JsonName is not null) sw.WriteLine($"        [JsonProperty(\"{prop.JsonName}\")]");
                sw.WriteLine($"        public {(prop.Required ? "required " : "")}{prop.Type} {prop.Name};");
            }
            sw.WriteLine("    }");
        }
        
        foreach (var enumName in _enums) foreach (string enumValue in enumName.Value)
        {
            sw.WriteLine($"    /// <summary>Equivalent to <c>{enumValue}</c></summary>");
            sw.WriteLine($"    public const string {enumName.Key}_{Formatting.SnakeToPascalCase(enumValue)} = \"{enumValue}\";");
        }
        
        foreach (InterfaceProperty prop in _properties)
        {
            sw.WriteLine("    /// <summary>");
            sw.WriteLine($"    /// {(prop.Description is null ? "" : prop.Description.Replace("\n", ""))}");
            sw.WriteLine("    /// </summary>");
            
            sw.WriteLine($"    [{TraitPropertyAttribute.Replace("Attribute", "")}]");
            foreach (TraitPropertyConstraintAttribute constraint in prop.Constraints)
            {
                string values = string.Join(',',
                    constraint.Values.Select((v) => v is string ? $"\"{v}\"" : v.ToString()));
                
                // [TraitPropertyConstraint(TraitPropertyConstraint.Constraint.OneOf, "use", "attack")]
                sw.WriteLine($"    [{TraitPropertyConstraintAttribute.Replace("Attribute", "")}({TraitPropertyConstraintAttribute}.Constraint.{System.Enum.GetName(constraint.Operation)}, {values})]");
            }
            foreach (TraitPropertyWarningAttribute warning in prop.Warnings)
            {
                string values = string.Join(',',
                    warning.Values.Select((v) => v is string ? $"\"{v}\"" : v.ToString()));
                
                // [TraitPropertyWarning("warning", TraitPropertyConstraint.Constraint.Equal, 1)]
                sw.WriteLine($"    [{TraitPropertyWarningAttribute.Replace("Attribute", "")}(\"{warning.Warning}\", {TraitPropertyConstraintAttribute}.Constraint.{System.Enum.GetName(warning.Operation)}, {values})]");
            }
            
            // public abstract float Duration { get; }
            if (prop.Required) sw.WriteLine($"    public abstract {prop.Type} {prop.Name} {{ get; }}");
            // public virtual dynamic? Type => null;
            else sw.WriteLine($"    public virtual {prop.Type} {prop.Name} => {FormatValue(prop.DefaultValue, prop.Type)};");
            
        }
        
        sw.WriteLine("}");
        
        string s = sw.ToString();
        sw.Dispose();
        return s;
    }
}