namespace ingot.Core.TraitSystem.Traits.Item;
using ingot.Core.Common;
using ingot.Core.Common.SharedConstructs;
using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits;

using Newtonsoft.Json;

/// <summary>
/// Digger item component specifies how quickly this item can dig specific blocks.
/// </summary>
[Trait("minecraft:digger", TraitSystem.TraitType.Item)]
[TraitFormatVersion("1.20.50")]
public interface IDigger : IItemTrait
{
    /// <summary>
    /// Block + speed pair in <see cref="IDigger"/>.
    /// </summary>
    public struct DestroySpeed(Either<Identifier, BlockPermutationDescriptor, BlockTypeDescriptor> block, int speed)
    {
        /// <summary>
        /// The block that the related destroy speed will apply to.
        /// </summary>
        [JsonProperty("block")] public Either<Identifier, BlockPermutationDescriptor, BlockTypeDescriptor> Block = block;
        /// <summary>
        /// The speed at which the block will be mined.
        /// </summary>
        [JsonProperty("speed")] public int Speed = speed;
    }

    /// <summary>
    /// A list of blocks to dig with correlating speeds of digging.
    /// Block descriptor: identifier string or a permutation/name object.
    /// </summary>
    [TraitProperty]
    public virtual DestroySpeed[] DestroySpeeds => [];
    /// <summary>
    /// Determines whether this item should be impacted if the efficiency enchantment is applied to it.
    /// </summary>
    [TraitProperty]
    public virtual bool UseEfficiency => false;
}
