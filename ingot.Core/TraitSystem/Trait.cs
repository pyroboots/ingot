using System.Text;
using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;
using Formatting = ingot.Core.Common.Formatting;

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
    
    public void Compile(ref JsonTextWriter writer)
    {
        Object(ref writer, Identifier.ToString(), w =>
        {
            foreach (TraitProperty property in Properties)
            {
                string id = Formatting.PascalToSnakeCase(property.Name);
                Property(ref w, id, property.Value);
            }
        });
    }
}