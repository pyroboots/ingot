using ingot.Core;

namespace ingot.Example;

class Program
{
    static void Main(string[] args)
    {
        BehaviourPack bp = BehaviourPack.Create(Guid.NewGuid().ToString())
            // items
            .AddItem<LasagnaItem>()
            .AddItem<CheeseItem>()
            .AddItem<PastaItem>()
            .AddItem<SauceItem>()
            // blocks
            .AddBlock<DenseLasagnaBlock>()
            // entities
            .AddEntity<LasagnaSpiritEntity>()
            // recipes
            .AddRecipe<LasagnaRecipe>()
            .AddRecipe<LasagnaBowlRecipe>()
            // loot tables
            // not needed as DenseLasagnaBlock will auto register it for us! :sparkle:
            //.AddLootTable<DenseLasagnaLoot>()
            .AddLootTable<LasagnaSpiritLoot>();

        ResourcePack rp = ResourcePack.Create(Guid.NewGuid().ToString())
            .AddBlockTexture("block_of_dense_lasagna", "./dense_lasagna.png")
            .AddItemTexture("lasagna", "./lasagna.png")
            // ingredient textures optional — compile succeeds without source PNGs
            .AddItemTexture("cheese", "./lasagna.png")
            .AddItemTexture("pasta", "./lasagna.png")
            .AddItemTexture("spooky_special_sauce", "./lasagna.png");

        Pack pack = new()
        {
            Description = "Example pack made with ingot",
            Name = "ingot example",
            BehaviourPack = bp,
            ResourcePack = rp,
            LinkPacks = true,
            ScriptsEnabled = true,
        };

        pack.Compile("./");

        // ingot writes the script module entry in manifest.json but does not copy script sources yet
        string scriptDest = "./bp/scripts/main.js";
        Directory.CreateDirectory(Path.GetDirectoryName(scriptDest)!);
        string scriptSource = Path.Combine(Directory.GetCurrentDirectory(), "scripts", "main.js");
        if (!File.Exists(scriptSource))
            scriptSource = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "scripts", "main.js"));
        if (File.Exists(scriptSource))
            File.Copy(scriptSource, scriptDest, overwrite: true);
        else
            File.WriteAllText(scriptDest, "import { world } from \"@minecraft/server\";\n");
    }
}