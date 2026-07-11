namespace ingot.Example.CombinationCooking;

/// <summary>
/// Nutrition / saturation defaults per food type.
/// </summary>
public static class FoodStats
{
    public static readonly string[] FoodTypes = ["magic", "pasta", "soup"];

    public static readonly string[] Colors =
    [
        "black", "blue", "cyan", "gray", "green", "lime", "magenta",
        "orange", "pink", "purple", "red", "white", "yellow",
    ];

    public static (int Nutrition, float Saturation) ForType(string foodType) => foodType switch
    {
        "soup" => (4, 0.4f),
        "pasta" => (6, 0.6f),
        "magic" => (8, 0.8f),
        _ => (4, 0.4f),
    };

    public static string CatalystForType(string foodType) => foodType switch
    {
        "magic" => "minecraft:nether_wart",
        "soup" => "minecraft:red_mushroom",
        "pasta" => "minecraft:wheat",
        _ => throw new ArgumentOutOfRangeException(nameof(foodType), foodType, "unknown food type"),
    };

    public static string TitleCase(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToUpperInvariant(value[0]) + value[1..];
}
