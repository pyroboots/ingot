using System.Text;
using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;
using Formatting = ingot.Core.Common.Formatting;

namespace ingot.Core.TraitSystem;

/// <summary>
/// Contains reflected data of a member in a trait interface
/// </summary>
public record TraitProperty
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

/// <summary>
/// Contains reflected data of a trait interface
/// </summary>
public class Trait : Identifiable, ICompileableFragment
{
    public Trait(Identifier identifier, Type root) : base(identifier) => RootTrait = root;

    /// <summary>
    /// List of properties in a trait at runtime
    /// </summary>
    public List<TraitProperty> Properties = new();
    /// <summary>
    /// The concrete type this trait is derived from
    /// </summary>
    public Type RootTrait;
    
    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        
        json.Object(Identifier.ToString(), () =>
        {
            foreach (TraitProperty property in Properties)
            {
                string id = Formatting.PascalToSnakeCase(property.Name);
                json.Property(id, property.Value);
            }
        });
    }
}