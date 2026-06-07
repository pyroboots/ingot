namespace ingot.Core.TraitSystem;

[AttributeUsage(AttributeTargets.Interface)]
public class TraitAttribute : Attribute
{
    public string Identifier;
    public TraitAttribute(string identifier)
    {
        Identifier = identifier;
    }
}