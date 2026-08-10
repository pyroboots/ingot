using ingot.Core.Common;
using ingot.Core.Common.SharedConstructs;
using ingot.Core.TraitSystem.Traits.Block;

using Newtonsoft.Json;

using Version = ingot.Core.Common.Version;

namespace ingot.Core.Behaviour.Block.BlockTraits;

/// <summary>
/// Contains information about the player's rotation when the block was placed.
/// Requires format version <c>1.26.0</c> or later
/// </summary>
public class PlacementDirectionVanillaBlockTrait : IVanillaBlockTrait
{
    /// <summary>
    /// Helper method that returns pre-defined block permutations that handle the <c>minecraft:cardinal_direction</c> state
    /// </summary>
    /// <typeparam name="TBlock">Parent <see cref="Block"/> of the permutation</typeparam>
    public static IEnumerable<NDirectionBlockPermutation<TBlock>> CardinalDirectionStateHelper<TBlock>() where TBlock : Block, new()
    {
        string[] dirs = ["north", "east", "south", "west"];
        for (int i = 0; i < dirs.Length; i++)
        {
            string dir = dirs[i];
            int rot = 90 * i;
            yield return new NDirectionBlockPermutation<TBlock>(dir, rot);
        }
    }
    
    /// <summary>
    /// Helper block permutation to simplify cardinal direction rotation
    /// </summary>
    public class NDirectionBlockPermutation<TBlock> : BlockPermutation, ITransformation where TBlock : Block, new()
    {
        /// <summary>
        /// Helper block permutation to simplify cardinal direction rotation
        /// </summary>
        /// <param name="direction">Cardinal direction used in the molang</param>
        /// <param name="rotation">Axis aligned (n % 90 == 0) angle to transform</param>
        /// <typeparam name="TBlock">Parent <see cref="Block"/> of the permutation</typeparam>
        public NDirectionBlockPermutation(string direction, int rotation)
        {
            string[] dirs = ["north", "east", "south", "west"];
            if (dirs.Contains(direction) == false)
                throw new ArgumentException("direction must be a valid cardinal direction");
            if (rotation % 90 != 0)
                throw new ArgumentException("rotation angle must be axis aligned");
            
            Condition = direction;
            _rot = rotation;
        }

        private readonly int _rot;
        
        /// <inheritdoc/>
        public override string Condition => $"q.block_state('minecraft:cardinal_direction') == '{field}'";
        /// <inheritdoc/>
        public override Block Parent => new TBlock();

        dynamic ITransformation.Rotation => new[] {0, _rot, 0};
    }
    
    private static readonly HashSet<string> ValidEnabledStates =
    [
        "minecraft:cardinal_direction",
        "minecraft:corner_and_cardinal_direction",
        "minecraft:facing_direction",
        "minecraft:sixteen_way_rotation",
    ];

    /// <inheritdoc/>
    public Identifier Identifier => "minecraft:placement_direction";

    /// <inheritdoc/>
    public Version MinimumFormatVersion => new("1.26.0");

    /// <summary>
    /// Which placement-direction states to enable.
    /// Valid values: <c>minecraft:cardinal_direction</c>, <c>minecraft:corner_and_cardinal_direction</c>,
    /// <c>minecraft:facing_direction</c>, <c>minecraft:sixteen_way_rotation</c>.
    /// Defaults to <c>minecraft:cardinal_direction</c>
    /// </summary>
    public Identifier[] EnabledStates { get; init; } = [new("minecraft:cardinal_direction")];

    /// <inheritdoc/>
    public ProvidedState[] ProvidedStates =>
    [
        new("minecraft:cardinal_direction", ProvidedState.CardinalDirectionValues),
        new("minecraft:corner", ["none", "inner_left", "inner_right", "outer_left", "outer_right"]),
        new("minecraft:facing_direction", [..ProvidedState.CardinalDirectionValues, "up", "down"]),
        new("minecraft:sixteen_way_rotation", [..Enumerable.Range(0, 16)]),
    ];

    /// <summary>
    /// Offset Y rotation added to the default rotation on place.
    /// Only axis-aligned angles (multiples of 90) are allowed
    /// </summary>
    public int YRotationOffset { get; init; }

    /// <summary>
    /// Blocks this block can create corners with.
    /// Only valid when <c>minecraft:corner_and_cardinal_direction</c> is in <see cref="EnabledStates"/>
    /// </summary>
    public BlockTypeDescriptor[] BlocksToCornerWith { get; init; } = [];

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        if (EnabledStates.Length == 0)
            throw new ArgumentException("EnabledStates must contain at least one state", nameof(EnabledStates));

        foreach (Identifier state in EnabledStates)
        {
            if (!ValidEnabledStates.Contains(state.ToString()))
            {
                throw new ArgumentException(
                    $"invalid enabled state \"{state}\" for {Identifier}. " +
                    $"valid: {string.Join(", ", ValidEnabledStates)}",
                    nameof(EnabledStates));
            }
        }

        if (YRotationOffset % 90 != 0)
            throw new ArgumentException(
                "YRotationOffset must be an axis-aligned rotation (multiple of 90)",
                nameof(YRotationOffset));

        bool cornersEnabled = EnabledStates.Any(s => s.ToString() == "minecraft:corner_and_cardinal_direction");
        if (BlocksToCornerWith.Length > 0 && !cornersEnabled)
        {
            throw new ArgumentException(
                "BlocksToCornerWith may only be set when EnabledStates includes minecraft:corner_and_cardinal_direction",
                nameof(BlocksToCornerWith));
        }

        JsonHelper json = new(ref writer);
        json.Object(Identifier, () =>
        {
            json.Property("enabled_states", EnabledStates);
            if (YRotationOffset != 0)
                json.Property("y_rotation_offset", YRotationOffset);
            if (BlocksToCornerWith.Length > 0)
                json.Property("blocks_to_corner_with", BlocksToCornerWith);
        });
    }
}
