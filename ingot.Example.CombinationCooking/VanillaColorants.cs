using ingot.Core.Common;

namespace ingot.Example.CombinationCooking;

/// <summary>
/// Curated Bedrock vanilla items used as colourants, with approximate average RGB values
/// (sampled from typical item colours — not Java jar assets).
/// </summary>
public static class VanillaColorants
{
    public readonly record struct Colorant(string ItemId, byte R, byte G, byte B)
    {
        public Identifier Identifier => new(ItemId);
    }

    /// <summary>
    /// Bedrock item identifiers with representative average colours.
    /// Enough variety to hit every palette swatch used by overlay tints.
    /// </summary>
    public static readonly Colorant[] All =
    [
        // warm / reds / oranges
        new("minecraft:carrot", 230, 125, 30),
        new("minecraft:beetroot", 140, 30, 40),
        new("minecraft:sweet_berries", 160, 30, 40),
        new("minecraft:red_dye", 176, 46, 38),
        new("minecraft:orange_dye", 225, 111, 15),
        new("minecraft:melon_slice", 180, 40, 40),
        new("minecraft:apple", 180, 40, 35),
        new("minecraft:glow_berries", 230, 150, 40),
        new("minecraft:magma_cream", 200, 80, 30),
        new("minecraft:blaze_powder", 230, 170, 40),

        // yellows / limes / greens
        new("minecraft:yellow_dye", 223, 158, 19),
        new("minecraft:gold_ingot", 220, 180, 50),
        new("minecraft:glowstone_dust", 230, 200, 80),
        new("minecraft:lime_dye", 119, 191, 25),
        new("minecraft:green_dye", 80, 120, 30),
        new("minecraft:kelp", 60, 120, 50),
        new("minecraft:dried_kelp", 50, 70, 40),
        new("minecraft:cactus", 70, 130, 50),
        new("minecraft:slime_ball", 90, 180, 70),
        new("minecraft:emerald", 40, 180, 90),

        // cyans / blues
        new("minecraft:cyan_dye", 17, 144, 144),
        new("minecraft:prismarine_shard", 80, 160, 150),
        new("minecraft:prismarine_crystals", 100, 200, 190),
        new("minecraft:blue_dye", 50, 70, 180),
        new("minecraft:lapis_lazuli", 40, 70, 170),
        new("minecraft:heart_of_the_sea", 30, 90, 160),

        // purples / magentas / pinks
        new("minecraft:purple_dye", 122, 30, 194),
        new("minecraft:chorus_fruit", 120, 70, 140),
        new("minecraft:amethyst_shard", 150, 100, 200),
        new("minecraft:magenta_dye", 178, 57, 168),
        new("minecraft:pink_dye", 245, 125, 162),
        new("minecraft:pink_petals", 230, 140, 170),

        // neutrals
        new("minecraft:ink_sac", 30, 30, 40),
        new("minecraft:black_dye", 30, 30, 35),
        new("minecraft:coal", 35, 35, 35),
        new("minecraft:gray_dye", 89, 106, 108),
        new("minecraft:iron_ingot", 180, 180, 185),
        new("minecraft:bone_meal", 220, 220, 210),
        new("minecraft:white_dye", 230, 230, 230),
        new("minecraft:sugar", 240, 240, 240),
        new("minecraft:quartz", 230, 225, 220),
        new("minecraft:ghast_tear", 200, 220, 220),
    ];

    /// <summary>
    /// Maps an RGB triple to the nearest palette colour name using the given
    /// mid-vibrancy swatches (same index used for overlay tints in <c>Program</c>).
    /// </summary>
    public static string NearestPaletteColor(
        byte r,
        byte g,
        byte b,
        IReadOnlyDictionary<string, (byte R, byte G, byte B)> paletteSwatches)
    {
        string best = "gray";
        double bestDist = double.MaxValue;

        foreach ((string name, (byte pr, byte pg, byte pb)) in paletteSwatches)
        {
            double dr = r - pr;
            double dg = g - pg;
            double db = b - pb;
            double dist = dr * dr + dg * dg + db * db;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = name;
            }
        }

        return best;
    }

    /// <summary>
    /// Builds swatches from loaded palette hex arrays at the given vibrancy index.
    /// </summary>
    public static Dictionary<string, (byte R, byte G, byte B)> BuildSwatches(
        IReadOnlyDictionary<string, string[]> palettes,
        int vibrancyIndex)
    {
        Dictionary<string, (byte, byte, byte)> result = new(StringComparer.Ordinal);
        foreach ((string name, string[] hexColors) in palettes)
        {
            int index = Math.Clamp(vibrancyIndex, 0, hexColors.Length - 1);
            string hex = hexColors[index].TrimStart('#');
            byte r = Convert.ToByte(hex[..2], 16);
            byte g = Convert.ToByte(hex[2..4], 16);
            byte b = Convert.ToByte(hex[4..6], 16);
            result[name] = (r, g, b);
        }

        return result;
    }
}
