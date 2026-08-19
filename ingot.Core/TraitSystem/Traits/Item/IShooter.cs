namespace ingot.Core.TraitSystem.Traits.Item;
using ingot.Core.Common;
using ingot.Core.Common.SharedConstructs;
using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits;

using Newtonsoft.Json;

/// <summary>
/// Shooter Item Component.
/// </summary>
[Trait("minecraft:shooter", TraitSystem.TraitType.Item)]
[TraitFormatVersion("1.20.50")]
public interface IShooter : IItemTrait
{
    /// <summary>
    /// Ammunition entry in <see cref="IShooter"/>.
    /// </summary>
    public struct AmmunitionItem(Either<Identifier, ItemTagsDescriptor> item)
    {
        /// <summary>
        /// Denotes the item description identifier. Item must have the minecraft:projectile component.
        /// </summary>
        [JsonProperty("item")] public Either<Identifier, ItemTagsDescriptor> Item = item;
        /// <summary>
        /// Determines whether inventory slots can be searched for this ammunition.
        /// </summary>
        [JsonProperty("search_inventory")] public bool SearchInventory;
        /// <summary>
        /// Determines whether this ammunition should be used by default when the holder is in creative mode.
        /// </summary>
        [JsonProperty("use_in_creative")] public bool UseInCreative;
        /// <summary>
        /// Determines whether this ammunition can be used when in the off-hand slot.
        /// </summary>
        [JsonProperty("use_offhand")] public bool UseOffhand;
    }

    /// <summary>
    /// Ammunition. Item descriptor: identifier string or a tags object.
    /// </summary>
    [TraitProperty]
    public virtual AmmunitionItem[] Ammunition => [];
    /// <summary>
    /// Charge on draw? Default is set to false.
    /// </summary>
    [TraitProperty]
    public virtual bool ChargeOnDraw => false;
    /// <summary>
    /// Draw Duration. Default is set to 0.
    /// </summary>
    [TraitProperty]
    public virtual float MaxDrawDuration => 0;
    /// <summary>
    /// Scale power by draw duration? Default is set to false.
    /// </summary>
    [TraitProperty]
    public virtual bool ScalePowerByDrawDuration => false;
}
