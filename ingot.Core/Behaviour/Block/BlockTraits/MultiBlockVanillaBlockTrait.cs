using ingot.Core.Common;

using Newtonsoft.Json;

using Version = ingot.Core.Common.Version;

namespace ingot.Core.Behaviour.Block.BlockTraits;

/// <summary>
/// Causes the block to be treated as a multi-block made up of multiple parts.
/// Requires format version <c>1.26.0</c> or later
/// </summary>
public class MultiBlockVanillaBlockTrait : IVanillaBlockTrait
{
    /// <inheritdoc/>
    public Identifier Identifier => "minecraft:multi_block";

    /// <inheritdoc/>
    public Version MinimumFormatVersion => new("1.26.0");

    /// <inheritdoc/>
    public Identifier[] EnabledStates => [new("minecraft:multi_block_part")];

    /// <inheritdoc/>
    public ProvidedState[] ProvidedStates =>
    [
        new("minecraft:multi_block_part", [..Enumerable.Range(0, Parts)])
    ];

    /// <summary>
    /// Determines the direction in which parts of the multi-block are placed.
    /// May only be <c>up</c> or <c>down</c>
    /// </summary>
    public required string Direction { get; init; }

    /// <summary>
    /// Determines the number of blocks that make up the multi-block (2–4).
    /// Controls placement count and the range of <c>minecraft:multi_block_part</c> (0..Parts-1)
    /// </summary>
    public required int Parts { get; init; }

    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        if (Parts is < 2 or > 4)
            throw new ArgumentOutOfRangeException(nameof(Parts), Parts, "Parts must be between 2 and 4 inclusive");
        if (Direction is not ("up" or "down"))
            throw new ArgumentException("Direction can only be \"up\" or \"down\"", nameof(Direction));

        JsonHelper json = new(ref writer);
        json.Object(Identifier, () =>
        {
            json.Property("enabled_states", EnabledStates);
            json.Property("parts", Parts);
            json.Property("direction", Direction);
        });
    }
}
