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
    [IngotValueConstraint(IngotValueConstraintAttribute.Operator.OneOf, [
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
    ])]
    [IngotValueWarning(IngotValueConstraintAttribute.Operator.OneOf, [
        "bow", // broken
        "block", // broken
        "camera", // broken
        "crossbow", // broken
        "none", // broken
    ], "animation '{x}' is broken and will display a broken animation")]
    public abstract string Value { get; }
}