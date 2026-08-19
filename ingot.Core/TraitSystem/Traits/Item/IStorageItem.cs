namespace ingot.Core.TraitSystem.Traits.Item;
using ingot.Core.Common;
using ingot.Core.Common.SharedConstructs;
using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits;

/// <summary>
/// [EXPERIMENTAL] Storage Items can be used by other components to store other items within this item.
/// </summary>
[Trait("minecraft:storage_item", TraitSystem.TraitType.Item)]
[TraitFormatVersion("1.21.30")]
public interface IStorageItem : IItemTrait
{
    /// <summary>
    /// Determines whether another Storage Item is allowed inside of this item. Default is true.
    /// </summary>
    [TraitProperty]
    public virtual bool AllowNestedStorageItems => true;
    /// <summary>
    /// List of items that are exclusively allowed in this Storage Item. If empty all items are allowed.
    /// </summary>
    [TraitProperty]
    public virtual Either<Identifier, ItemTagsDescriptor>[] AllowedItems => [];
    /// <summary>
    /// List of items that are not allowed in this Storage Item.
    /// </summary>
    [TraitProperty]
    public virtual Either<Identifier, ItemTagsDescriptor>[] BannedItems => [];
    /// <summary>
    /// The maximum number of different item stacks. Maximum is 64. Default is 64.
    /// </summary>
    [TraitProperty]
    [TraitPropertyConstraint(TraitPropertyConstraintAttribute.Constraint.LessThanEq, 64)]
    public virtual int MaxSlots => 64;
    /// <summary>
    /// The maximum allowed weight of the sum of all contained items. Maximum is 64. Default is 64.
    /// </summary>
    [TraitProperty]
    [TraitPropertyConstraint(TraitPropertyConstraintAttribute.Constraint.LessThanEq, 64)]
    public virtual int MaxWeightLimit => 64;
    /// <summary>
    /// The weight of this item when inside another Storage Item. Default is 4. 0 means item is not allowed in another Storage Item.
    /// </summary>
    [TraitProperty]
    [TraitPropertyConstraint(TraitPropertyConstraintAttribute.Constraint.GreaterThanEq, 0)]
    public virtual int WeightInStorageItem => 4;
}
