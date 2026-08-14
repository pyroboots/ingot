namespace ingot.Core.Common;

/// <summary>
/// Common static ingot values
/// </summary>
public static class IngotCommon
{
    // TODO: DONT FORGET TO BUMP THE VERSION!!!
    
    /// <summary>
    /// Current ingot version
    /// </summary>
    public static readonly string IngotVersion = "1.0.1";

    /// <summary>
    /// Writes the ingot version and name to the console at the start of compilation
    /// </summary>
    public static void WriteHeader() => Console.WriteLine($"ingot compiler - {IngotVersion} ({GetVersionName()})");

    /// <summary>
    /// Generates a version name from the current version
    /// </summary>
    public static string GetVersionName()
    {
        // was inspired by linux distro build names and it could be fun
        // for ingot to have one. so we have a list of 32 adjs and 64
        // blocks / items = 2048 possible combos. if i need more, i can just
        // add 16 colours
        
        int seed = BasicHash(IngotVersion);
        Random rng = new(seed);

        string adjective = _adjectives[rng.Next(_adjectives.Length)];
        string item = _blocksAndItems[rng.Next(_blocksAndItems.Length)];

        // this gives a nice version name like "WeatheredStrider"
        return $"{adjective}{item}";
    }

    private static int BasicHash(string input)
    {
        // turns out object.GetHashCode() is different everytime, so a simple
        // hash func like this is fine
        unchecked
        {
            int hash = 17;
            foreach (char c in input)
                hash = hash * 31 + c;
            return hash;
        }
    }
    
    private static readonly string[] _adjectives =
    [
        // ores / materials - 11
        "Resin",
        "Amethyst",
        "Iron",
        "Diamond",
        "Gold",
        "Netherite",
        "Quartz",
        "Lapis",
        "Coal",
        "Copper",
        "Redstone",
        
        // biome / dimension - 10
        "Lush",
        "Mushroom",
        "Nether",
        "Ender",
        "Overworld",
        "Swampy",
        "Crimson",
        "Warped",
        "Soul",
        "Sculk",
        
        // block types - 9
        "Glazed",
        "Chiseled",
        "Waxed",
        "Weathered",
        "Cut",
        "Enchanted",
        "Cursed",
        "Infested",
        "Petrified",
        
        // colours (i chose my favourite) - 3
        "Red",
        "Orange",
        "Yellow"
    ];

    private static readonly string[] _blocksAndItems =
    [
        // blocks - 13
        "CherryLeaves",
        "MangrovePropagule",
        "Crafter",
        "PaleOakLog",
        "Eyeblossom",
        "Shelf",
        "ShulkerBox",
        "Beacon",
        "DragonEgg",
        "Conduit",
        "Lodestone",
        "RespawnAnchor",
        "Piston",
        
        // items - 11
        "Spyglass",
        "EchoShard",
        "Brush",
        "RecoveryCompass",
        "GoatHorn",
        "TrialKey",
        "OminousBottle",
        "WolfArmor",
        "TotemOfUndying",
        "Elytra",
        "Trident",
        
        // entities - 40
        "EndCrystal",
        "DriedGhast",
        "Creeper",
        "Warden",
        "PiglinBrute",
        "Breeze",
        "Bogged",
        "Creaking",
        "SulfurCube",
        "Phantom",
        "Hoglin",
        "Zoglin",
        "Piglin",
        "Axolotl",
        "Sniffer",
        "Allay",
        "Armadillo",
        "HappyGhast",
        "Nautilus",
        "Tadpole",
        "GlowSquid",
        "Strider",
        "Villager",
        "SnowGolem",
        "ArmorStand",
        "ItemFrame",
        "Painting",
        "BoatWithChest",
        "MinecartWithHopper"
    ];
}
