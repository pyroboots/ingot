namespace ingot.Core.TraitSystem.Traits.Item;

using ingot.Core.Common;

/// <summary>
/// Use_animation specifies which animation is played when the player uses the item.
/// </summary>
[Trait("minecraft:use_animation", TraitSystem.TraitType.Item)]
public interface IUseAnimation : IItemTrait
{
    /// <summary>
    /// Specifies which animation to play when the item is used.
    /// </summary>
    [TraitProperty]
    [TraitPropertyConstraint(TraitPropertyConstraintAttribute.Constraint.OneOf,
        "eat", 
        "drink",
        "bow", // broken
        "block", // broken
        "camera", // broken
        "crossbow", // broken
        "none", // broken
        "brush",
        "spear",
        "spyglass"
    )]
    [TraitPropertyWarning("animation '{x}' is broken and will display an incorrect animation", TraitPropertyConstraintAttribute.Constraint.OneOf, 
        "bow", // broken
        "block", // broken
        "camera", // broken
        "crossbow", // broken
        "none" // broken
    )]
    public abstract string Value { get; }
}