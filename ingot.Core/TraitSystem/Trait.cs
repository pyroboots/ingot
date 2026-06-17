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
    /// <summary>
    /// Creates a reflected trait property ready for JSON compilation.
    /// </summary>
    /// <param name="path">Molang or JSON path prefix for the property value.</param>
    /// <param name="name">Property name on the trait interface.</param>
    /// <param name="value">Runtime value of the property.</param>
    public TraitProperty(string path, string name, dynamic value)
    {
        Path = path;
        Name = name;
        Value = value;
    }
    
    /// <summary>
    /// Molang or JSON path prefix for the property value.
    /// </summary>
    public string Path = "@=*";
    /// <summary>
    /// Property name on the trait interface.
    /// </summary>
    public string Name;
    /// <summary>
    /// Runtime value of the property.
    /// </summary>
    public dynamic Value;
}

/// <summary>
/// Contains reflected data of a trait interface
/// </summary>
public class Trait : IIdentifiable, ICompileableFragment
{
    /// <inheritdoc/>
    public Identifier Identifier { get; }

    /// <summary>
    /// Creates a <see cref="Trait"/> from a component identifier and the concrete trait interface type.
    /// </summary>
    /// <param name="identifier">Bedrock component identifier (e.g. <c>minecraft:food</c>).</param>
    /// <param name="root">Concrete trait interface type implemented by the content class.</param>
    public Trait(Identifier identifier, Type root)
    {
        Identifier = identifier;
        RootTrait = root;
    }

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