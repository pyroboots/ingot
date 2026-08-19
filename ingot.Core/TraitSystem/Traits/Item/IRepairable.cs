using ingot.Core.Common.SharedConstructs;

using Newtonsoft.Json;

namespace ingot.Core.TraitSystem.Traits.Item;
using ingot.Core.Common;
using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits;

/// <summary>
/// The repairable item component specifies which items can be used to repair this item, along with how much durability is gained.
/// </summary>
[Trait("minecraft:repairable", TraitSystem.TraitType.Item)]
[TraitFormatVersion("1.20.50")]
public interface IRepairable : IItemTrait
{
    /// <summary>
    /// Repair entry in <see cref="IRepairable"/>
    /// </summary>
    public struct RepairItem(Either<int, Molang> amount, params Either<Identifier, ItemTagsDescriptor>[] items)
    {
        /// <summary>
        /// The items used to repair the item
        /// </summary>
        [JsonProperty("items")] public required Either<Identifier, ItemTagsDescriptor>[] Items = items;
        /// <summary>
        /// How much durability is repaired
        /// </summary>
        [JsonProperty("repair_amount")] public Either<int, Molang> RepairAmount = amount;
    }

    /// <summary>
    /// List of repair item entries. Each entry needs to define a list of strings for `items` that can be used for the repair and an optional `repair_amount` for how much durability is gained.
    /// </summary>
    [TraitProperty]
    public virtual RepairItem[] RepairItems => [];
}
