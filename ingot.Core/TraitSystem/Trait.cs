using ingot.Core.Common;
using Newtonsoft.Json;

namespace ingot.Core.TraitSystem;

public struct TraitProperty
{
    public TraitProperty(string path, string name, object value)
    {
        Path = path;
        Name = name;
        Value = value;
    }
    
    public string Path = "@=*";
    public required string Name;
    public required object Value;
}

public class Trait : Identifiable, ICompileableFragment
{
    public Trait(string identifier) : base(identifier) {}
    public Trait(Identifier identifier) : base(identifier) {}

    public List<TraitProperty> Properties = new();
    
    public void Compile(ref JsonTextWriter writer)
    {
        throw new NotImplementedException();
    }
}