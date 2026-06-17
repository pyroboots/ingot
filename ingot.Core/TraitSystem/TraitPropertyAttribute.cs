namespace ingot.Core.TraitSystem;

/// <summary>
/// Marks a property in a trait interface as a property to be serialized in reflection
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TraitPropertyAttribute : Attribute
{
    /// <summary>
    /// Molang or JSON path prefix for the serialized property value.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Marks a trait property with an explicit serialization path.
    /// </summary>
    /// <param name="path">Molang or JSON path prefix for the property value.</param>
    public TraitPropertyAttribute(string path) => Path = path;

    /// <summary>
    /// Marks a trait property using the default serialization path.
    /// </summary>
    public TraitPropertyAttribute() => Path = "@=*";
}