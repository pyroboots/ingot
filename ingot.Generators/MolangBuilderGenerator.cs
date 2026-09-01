using System.Text;

using Newtonsoft.Json;

using Formatting = ingot.Core.Common.Formatting;

namespace ingot.Generators;

public class MolangBuilderGenerator
{
    private struct MolangSchema
    {
        public record MolangMathFunction(
            string description,
            string name,
            int? min_args = null,
            int? max_args = null);
        
        public record MolangQuery(
            string description,
            string name,
            string return_type,
            int? min_args = null,
            int? max_args = null);

        
        public MolangMathFunction[] math_functions;
        public string module_type;
        public string name;
        public MolangQuery[] queries;
    }

    private static void Function(ref StringBuilder builder, string returnType, string funcName, string[] args, int? minArgs, int? maxArgs, string funcId)
    {
        const string tab = "    ";
        
        builder.AppendLine($"{tab}public {returnType} {funcName}({string.Join(", ", args)})");
        builder.AppendLine($"{tab}{{");
            
        if (maxArgs is not null)
            builder.AppendLine($"{tab}{tab}if (args.Length > {maxArgs}) throw new ArgumentException(\"max argument count of {maxArgs}\");");
        if (minArgs is not null && minArgs > 0)
            builder.AppendLine($"{tab}{tab}if (args.Length < {minArgs}) throw new ArgumentException(\"min argument count of {minArgs}\");");
        builder.AppendLine($"{tab}{tab}_tokens.Add($\"{funcId}({{FormatParams(args)}})\");");
        builder.AppendLine($"{tab}{tab}return this;");
        builder.AppendLine($"{tab}}}");
    }
    
    public static string GenerateMolangBuilderClass(string json)
    {
        const string tab = "    ";
        const string className = "Molang";
        
        StringBuilder builder = new();
        MolangSchema molang = JsonConvert.DeserializeObject<MolangSchema>(json);

        builder.AppendLine($"// queries: {molang.queries.Length}");
        builder.AppendLine($"// math funcs: {molang.math_functions.Length}");
        builder.AppendLine($"public partial class {className}");
        builder.AppendLine("{");
        
        builder.AppendLine($"{tab}#region queries");
        foreach (MolangSchema.MolangQuery query in molang.queries)
        {
            builder.AppendLine($"{tab}/// <summary>{query.description}</summary>");
            builder.AppendLine($"{tab}/// <returns><c>{query.return_type}</c></returns>");
            
            string funcName = Formatting.SnakeToPascalCase(query.name.Split('.')[1]);
            Function(ref builder, className, funcName, ["params object[] args"], query.min_args, query.max_args, query.name);
        }
        builder.AppendLine($"{tab}#endregion");
        
        builder.AppendLine($"{tab}#region math");
        foreach (MolangSchema.MolangMathFunction math in molang.math_functions)
        {
            builder.AppendLine($"{tab}/// <summary>{math.description}</summary>");
            
            string funcName = Formatting.SnakeToPascalCase(math.name.Split('.')[1]);
            Function(ref builder, className, funcName, ["params object[] args"], math.min_args, math.max_args, math.name);
        }
        builder.AppendLine($"{tab}#endregion");
        
        builder.AppendLine("}");
        
        return builder.ToString();
    }
}