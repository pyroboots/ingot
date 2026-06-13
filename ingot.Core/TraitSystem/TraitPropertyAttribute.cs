namespace ingot.Core.TraitSystem;

/// <summary>
/// Marks a property in a trait interface as a property to be serialized in reflection
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TraitPropertyAttribute : Attribute
{
    public string Path { get; }
    public TraitPropertyAttribute(string path) => Path = path;
    public TraitPropertyAttribute() => Path = "@=*";
}