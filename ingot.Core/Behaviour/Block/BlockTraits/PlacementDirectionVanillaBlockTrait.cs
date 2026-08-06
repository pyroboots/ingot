using ingot.Core.Common;
using ingot.Core.Common.SharedConstructs;

using Newtonsoft.Json;

using Version = ingot.Core.Common.Version;

namespace ingot.Core.Behaviour.Block.BlockTraits;

/// <summary>
/// Contains information about the player's rotation when the block was placed.
/// Requires format version <c>1.26.0</c> or later
/// </summary>
public class PlacementDirectionVanillaBlockTrait : IVanillaBlockTrait
{
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
