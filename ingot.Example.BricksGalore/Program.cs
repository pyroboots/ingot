using System.Reflection;

using ingot.Core;
using ingot.Core.Common;

namespace ingot.Example.BricksGalore;

/// <summary>
/// Bricks Galore content entry point.
/// <para>
/// <b>Extending the pack</b> - edit the registration block in <see cref="BuildContent"/>:
/// </para>
/// <list type="bullet">
/// <item>New material: add <c>Palettes/{id}.gpl</c>, then <c>reg.AddMaterial(...)</c></item>
/// <item>New pattern: add base PNG under <c>Textures/{Bricks|Chiseled|Tiles}/</c>,
/// optional overlay under <c>Textures/Overlays/...</c>, then <c>reg.AddPattern(...)</c></item>
/// </list>
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        CompilerState.ShowInfoLogs = true;
        CompilerState.Push("precompile");

        IngotCommon.WriteHeader();

        // 1. content registry (materials + patterns) - edit buildcontent()
        CompilerState.Push("content registration");
        BrickRegistry reg = BuildContent();
        BrickRegistry.Activate(reg);
        CompilerState.Info(
            $"registered {reg.MaterialIds.Count} materials, {reg.PatternIds.Count} patterns " +
            $"({reg.Patterns.Values.Count(p => p.HasOverlay)} with overlays)");
        CompilerState.Pop();

        // 2. textures
        CompilerState.Push("texture generation");
        foreach ((string material, string[] colors) in reg.PaletteMap)
            CompilerState.Info($"palette {material}: {colors.Length} colours");

        TextureGenerator.GenerateAll(reg.PaletteMap);
        CompilerState.Pop();

        // 3. blocks + recipes + functions + lore service
        CompilerState.Push("block generation");
        List<Type> blockTypes = BlockGenerator.GenerateBlockTypes().ToList();
        CompilerState.Info($"prepared {blockTypes.Count} block types");
        CompilerState.Pop();

        CompilerState.Push("recipe generation");
        List<Type> recipeTypes = RecipeGenerator.GenerateRecipeTypes().ToList();
        CompilerState.Info($"prepared {recipeTypes.Count} recipe types");
        CompilerState.Pop();

        CompilerState.Push("function generation");
        string functionsDir = Path.Combine(AppContext.BaseDirectory, "Functions");
        (string placeAllPath, string clearAllPath) = PlaceAllFunction.Generate(functionsDir);
        CompilerState.Pop();

        CompilerState.Push("script generation");
        string descriptionService = Path.Combine(AppContext.BaseDirectory, "Scripts", "DescriptionService.js");
        reg.GenerateDescriptionService(descriptionService);
        CompilerState.Pop();

        CompilerState.Pop(); // precompile

        // 4. pack
        const string bpUuid = "e3f4a5b6-c7d8-9012-efab-345678901234";
        const string rpUuid = "f4a5b6c7-d8e9-0123-fabc-456789012345";

        Pack pack = Pack.Create(
            bpUuid,
            "Bricks Galore",
            "Example pack: material x pattern x mortar/inlay brick blocks generated with ingot",
            rpUuid);

        pack.ScriptsEnabled = true;
        pack.LinkPacks = false;

        MethodInfo addBlock = typeof(Pack).GetMethods()
            .Single(m => m.Name == nameof(Pack.AddBlock)
                         && m.IsGenericMethodDefinition
                         && m.GetParameters().Length == 0);

        MethodInfo addRecipe = typeof(Pack).GetMethods()
            .Single(m => m.Name == nameof(Pack.AddRecipe)
                         && m.IsGenericMethodDefinition
                         && m.GetParameters().Length == 0);

        CompilerState.Push("pack registration");
        int i = 0;
        foreach (Type blockType in blockTypes)
        {
            i++;
            addBlock.MakeGenericMethod(blockType).Invoke(pack, null);
            if (i % 50 == 0 || i == blockTypes.Count)
                CompilerState.Info($"registered {i}/{blockTypes.Count} blocks");
        }

        i = 0;
        foreach (Type recipeType in recipeTypes)
        {
            i++;
            addRecipe.MakeGenericMethod(recipeType).Invoke(pack, null);
            if (i % 50 == 0 || i == recipeTypes.Count)
                CompilerState.Info($"registered {i}/{recipeTypes.Count} recipes");
        }

        pack.AddFunction(PlaceAllFunction.PlaceFunctionName, placeAllPath);
        pack.AddFunction(PlaceAllFunction.ClearFunctionName, clearAllPath);
        pack.AddService(descriptionService, intervalTicks: 10);
        CompilerState.Info(
            $"registered functions {PlaceAllFunction.PlaceFunctionName}, {PlaceAllFunction.ClearFunctionName}; " +
            "DescriptionService lore");

        CompilerState.Pop();

        pack.CompileComMojang(
            "/home/pyro/.var/app/io.mrarm.mcpelauncher/data/mcpelauncher/games/com.mojang/",
            cache: true);
    }

    /// <summary>
    /// All pack content. To extend:
    /// <list type="number">
    /// <item>Drop <c>Palettes/my_mat.gpl</c> -> <c>reg.AddMaterial("my_mat", ...)</c></item>
    /// <item>Drop base (+ optional overlay) textures -> <c>reg.AddPattern("my_style", ...)</c></item>
    /// </list>
    /// Overlay PNGs named <c>{pattern}_mortar.png</c> or <c>{pattern}_overlay.png</c> under
    /// <c>Textures/Overlays/</c> are picked up automatically.
    /// </summary>
    private static BrickRegistry BuildContent()
    {
        BrickRegistry reg = new();

        // materials (palette file: palettes/{id}.gpl)
        reg
            .AddMaterial("amethyst",
                ingredient: "minecraft:amethyst_shard",
                loreColor: "§d",
                sound: "amethyst_block",
                secondsToDestroy: 1.5f,
                explosionResistance: 3f)
            .AddMaterial("copper",
                ingredient: "minecraft:copper_ingot",
                loreColor: "§6",
                sound: "copper",
                secondsToDestroy: 2f)
            .AddMaterial("diamond",
                ingredient: "minecraft:diamond",
                loreColor: "§b",
                sound: "metal",
                secondsToDestroy: 3.5f,
                tags:
                [
                    "minecraft:is_pickaxe_item_destructible",
                    "minecraft:diamond_pick_diggable",
                ])
            .AddMaterial("emerald",
                ingredient: "minecraft:emerald",
                loreColor: "§a",
                sound: "metal",
                secondsToDestroy: 3f)
            .AddMaterial("gold",
                ingredient: "minecraft:gold_ingot",
                loreColor: "§e",
                sound: "metal",
                secondsToDestroy: 2f)
            .AddMaterial("lapis",
                ingredient: "minecraft:lapis_lazuli",
                loreColor: "§9",
                sound: "stone",
                secondsToDestroy: 2f)
            .AddMaterial("netherite",
                ingredient: "minecraft:netherite_ingot",
                loreColor: "§8",
                sound: "netherite",
                secondsToDestroy: 4f,
                explosionResistance: 12f,
                tags:
                [
                    "minecraft:is_pickaxe_item_destructible",
                    "minecraft:diamond_pick_diggable",
                ])
            .AddMaterial("resin",
                ingredient: "minecraft:resin_brick",
                loreColor: "§6",
                sound: "resin",
                secondsToDestroy: 1.5f,
                explosionResistance: 3f,
                tags:
                [
                    "minecraft:is_pickaxe_item_destructible",
                    "stone",
                ]);

        // patterns (base under textures/..., overlay auto under overlays/)
        // bricks
        reg.AddPattern("inset_bricks", "Bricks/inset_bricks", "minecraft:tuff_bricks")
           .AddPattern("offset_bricks", "Bricks/offset_bricks", "minecraft:clay")
           .AddPattern("polished_offset_bricks", "Bricks/polished_offset_bricks", "minecraft:wax")
           .AddPattern("polished_small_bricks", "Bricks/polished_small_bricks", "minecraft:quartz")
           .AddPattern("sharp_bricks", "Bricks/sharp_bricks", "minecraft:stone_bricks")
           .AddPattern("small_bricks", "Bricks/small_bricks", "minecraft:brick");

        // chiseled
        reg.AddPattern("bee_chiseled", "Chiseled/bee_chiseled", "minecraft:honeycomb")
           .AddPattern("beveled_bricks", "Chiseled/beveled_bricks", "minecraft:sand")
           .AddPattern("breeze_chiseled", "Chiseled/breeze_chiseled", "minecraft:breeze_rod")
           .AddPattern("creaking_chiseled", "Chiseled/creaking_chiseled", "minecraft:resin_clump")
           .AddPattern("creeper_chiseled", "Chiseled/creeper_chiseled", "minecraft:gunpowder")
           .AddPattern("slime_chiseled", "Chiseled/slime_chiseled", "minecraft:slime_ball")
           .AddPattern("snout_chiseled", "Chiseled/snout_chiseled", "minecraft:blackstone")
           .AddPattern("square_chiseled", "Chiseled/square_chiseled", "minecraft:tuff")
           .AddPattern("tectonic_chiseled", "Chiseled/tectonic_chiseled", "minecraft:deepslate")
           .AddPattern("termite_chiseled", "Chiseled/termite_chiseled", "minecraft:gravel")
           .AddPattern("wither_chiseled", "Chiseled/wither_chiseled", "minecraft:wither_rose");

        // tiles
        reg.AddPattern("medium_tiles", "Tiles/medium_tiles", "minecraft:smooth_stone")
           .AddPattern("shaped_tiles", "Tiles/shaped_tiles", "minecraft:tuff")
           .AddPattern("small_tiles", "Tiles/small_tiles", "minecraft:deepslate_tiles");

        // optional: auto-register unlisted textures (default catalyst minecraft:brick).
        //reg.DiscoverUnregisteredPatterns();

        return reg;
    }
}
