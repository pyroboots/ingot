using ingot.Core.Common;

namespace ingot.Example.BricksGalore;

/// <summary>
/// Per-combo block data. Closed generic block types read one of these via a type token.
/// </summary>
public sealed class BlockSpec
{
    public required Identifier Identifier { get; init; }
    /// <summary>Body / brick material.</summary>
    public required string Material { get; init; }
    public required string Pattern { get; init; }
    /// <summary>Mortar / chiseled inlay material, when present.</summary>
    public string? OverlayMaterial { get; init; }
    public required string DisplayName { get; init; }
    public required string Texture { get; init; }
    public required string TexturePath { get; init; }
    public required string Sound { get; init; }
    public required float SecondsToDestroy { get; init; }
    public required float ExplosionResistance { get; init; }
    public required string[] Tags { get; init; }
}
