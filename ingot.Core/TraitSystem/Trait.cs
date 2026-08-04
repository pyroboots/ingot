using System.Text;

using ingot.Core.Common;

using Newtonsoft.Json;

using static ingot.Core.Common.JsonHelper;

using Formatting = ingot.Core.Common.Formatting;

namespace ingot.Core.TraitSystem;

/// <summary>
/// Contains reflected data of a member in a trait interface
/// </summary>
public record TraitProperty : ICompilableFragment
{
    /// <summary>
    /// Creates a reflected trait property ready for JSON compilation.
    /// </summary>
    /// <param name="name">Property name on the trait interface.</param>
    /// <param name="value">Runtime value of the property.</param>
    public TraitProperty(string name, object? value)
    {
        Name = name;
        Value = value;
    }
    
    /// <summary>
    /// Property name on the trait interface.
    /// </summary>
    public string Name;
    /// <summary>
    /// Runtime value of the property.
    /// </summary>
    public object? Value;

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer) => Property(ref writer, Formatting.PascalToSnakeCase(Name), Value);
}

/// <summary>
/// Contains reflected data of a trait interface
/// </summary>
public class Trait : IIdentifiable, ICompilableFragment
{
    /// <inheritdoc/>
    public Identifier Identifier { get; }

    /// <summary>
    /// Creates a <see cref="Trait"/> from a component identifier and the concrete trait interface type.
    /// </summary>
    /// <param name="identifier">Bedrock component identifier (e.g. <c>minecraft:food</c>).</param>
    /// <param name="root">Concrete trait interface type implemented by the content class.</param>
    public Trait(Identifier identifier, Type? root = null)
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
    public Type? RootTrait;

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);

        json.Object(Identifier.ToString(), () =>
        {
            foreach (TraitProperty property in Properties)
            {
                property.Compile(ref json.Writer);
            }
        });
    }
}