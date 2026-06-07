namespace ingot.Core.TraitSystem;

public class TraitPropertyAttribute : Attribute
{
    public string Path { get; }
    public TraitPropertyAttribute(string path) => Path = path;
    public TraitPropertyAttribute() => Path = "@=*";
}