using ingot.Core.Common;

namespace ingot.Example.CombinationCooking;

/// <summary>
/// Per-combo food data. Closed generic item types read one of these via a type token.
/// </summary>
public sealed class FoodSpec
{
    public required Identifier Identifier { get; init; }
    public required int Nutrition { get; init; }
    public required float SaturationModifier { get; init; }
    public required string[] Tags { get; init; }
    public required string Texture { get; init; }
    public string? TexturePath { get; init; }
    public required string DisplayName { get; init; }

    public string FoodType => Tags.Length > 0 ? Tags[0] : "soup";
}
