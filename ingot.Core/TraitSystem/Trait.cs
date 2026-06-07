using System.Text;
using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.JsonHelper;

namespace ingot.Core.TraitSystem;

public struct TraitProperty
{
    public TraitProperty(string path, string name, dynamic value)
    {
        Path = path;
        Name = name;
        Value = value;
    }
    
    public string Path = "@=*";
    public string Name;
    public dynamic Value;
}

public class Trait : Identifiable, ICompileableFragment
{
    public Trait(string identifier, Type root) : base(identifier) => RootTrait = root;
    public Trait(Identifier identifier, Type root) : base(identifier) => RootTrait = root;

    public List<TraitProperty> Properties = new();
    public Type RootTrait;
    
    private static string SnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        StringBuilder sb = new();
        sb.Append(char.ToLowerInvariant(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            char current = input[i];

            if (char.IsUpper(current))
            {
                if (sb.Length > 0 && 
                    (char.IsLower(input[i-1]) || 
                     (i + 1 < input.Length && char.IsLower(input[i+1]))))
                {
                    sb.Append('_');
                }
                sb.Append(char.ToLowerInvariant(current));
            }
            else
            {
                sb.Append(current);
            }
        }

        return sb.ToString();
    }
    
    public void Compile(ref JsonTextWriter writer)
    {
        Object(ref writer, Identifier.ToString(), w =>
        {
            foreach (TraitProperty property in Properties)
            {
                string id = SnakeCase(property.Name);
                Property(ref w, id, property.Value);
            }
        });
    }
}