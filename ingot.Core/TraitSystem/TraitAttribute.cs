namespace ingot.Core.TraitSystem;

[AttributeUsage(AttributeTargets.Interface)]
public class TraitAttribute : Attribute
{
    public TraitSystem.TraitType Constraint;
    public string Identifier;
    public TraitAttribute(string identifier, TraitSystem.TraitType constraint)
    {
        Identifier = identifier;
        Constraint = constraint;
    }
}