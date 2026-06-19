using ingot.Core.Common;

namespace ingot.Core.TraitSystem;

/// <summary>
/// Marks an interface as a valid trait
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public class TraitAttribute : Attribute
{
    /// <summary>
    /// Content type on which this trait is valid on
    /// </summary>
    public TraitSystem.TraitType Constraint;

    /// <summary>
    /// Identifier of the encapsulated component
    /// </summary>
    public Identifier Identifier;

    /// <summary>
    /// Marks an interface as a valid trait
    /// </summary>
    /// <param name="identifier">Identifier of the encapsulated component</param>
    /// <param name="constraint">Content type on which this trait is valid on</param>
    public TraitAttribute(string identifier, TraitSystem.TraitType constraint)
    {
        Identifier = new(identifier);
        Constraint = constraint;
    }
}