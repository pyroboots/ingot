using ingot.Core.Common;

using Newtonsoft.Json;

using Version = ingot.Core.Common.Version;

namespace ingot.Core.Behaviour.Block.BlockTraits;

/// <summary>
/// Allows for fence-like connection permutations by updating cardinal connection states based on adjacent blocks.
/// Requires format version <c>1.26.0</c> or later
/// </summary>
public class ConnectionVanillaBlockTrait : IVanillaBlockTrait
{
    /// <inheritdoc/>
    public Identifier Identifier => "minecraft:connection";

    /// <inheritdoc/>
    public Version MinimumFormatVersion => new("1.26.0");

    /// <inheritdoc/>
    public Identifier[] EnabledStates => [new("minecraft:cardinal_connections")];

    /// <inheritdoc/>
    public ProvidedState[] ProvidedStates =>
    [
        new("minecraft:connection_north", ProvidedState.BooleanValues),
        new("minecraft:connection_south", ProvidedState.BooleanValues),
        new("minecraft:connection_west", ProvidedState.BooleanValues),
        new("minecraft:connection_east", ProvidedState.BooleanValues),
    ];

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Object(Identifier, () =>
        {
            json.Property("enabled_states", EnabledStates);
        });
    }
}
