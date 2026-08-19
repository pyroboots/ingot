using ingot.Core.Common;

using Newtonsoft.Json;

using Version = ingot.Core.Common.Version;

namespace ingot.Core.Behaviour.Block.BlockTraits;

/// <summary>
/// Contains information about where the player placed the block.
/// Requires format version <c>1.20.20</c> or later
/// </summary>
public class PlacementPositionVanillaBlockTrait : IVanillaBlockTrait
{
    private static readonly HashSet<string> ValidEnabledStates =
    [
        "minecraft:block_face",
        "minecraft:vertical_half",
    ];

    /// <inheritdoc/>
    public Identifier Identifier => "minecraft:placement_position";

    /// <inheritdoc/>
    public Version MinimumFormatVersion => new("1.20.20");

    /// <summary>
    /// Which placement-position states to enable.
    /// Valid values: <c>minecraft:block_face</c>, <c>minecraft:vertical_half</c>
    /// </summary>
    public Identifier[] EnabledStates { get; init; } =
    [
        new("minecraft:block_face"),
        new("minecraft:vertical_half"),
    ];

    /// <inheritdoc/>
    public ProvidedState[] ProvidedStates =>
    [
        new("minecraft:block_face", [..ProvidedState.CardinalDirectionValues, "up", "down"]),
        new("minecraft:vertical_half", ["bottom", "top"]),
    ];

    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        if (EnabledStates.Length == 0)
            throw new ArgumentException("EnabledStates must contain at least one state", nameof(EnabledStates));

        foreach (Identifier state in EnabledStates)
        {
            if (!ValidEnabledStates.Contains(state.ToString()))
            {
                throw new ArgumentException(
                    $"Invalid enabled state \"{state}\" for {Identifier}. " +
                    $"Valid: {string.Join(", ", ValidEnabledStates)}",
                    nameof(EnabledStates));
            }
        }

        JsonHelper json = new(ref writer);
        json.Object(Identifier, () =>
        {
            json.Property("enabled_states", EnabledStates);
        });
    }
}
