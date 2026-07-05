using ingot.Core;
using ingot.Example.Blocks;
using ingot.Example.Entities;
using ingot.Example.Items;
using ingot.Example.LootTables;
using ingot.Example.Recipes;

using Version = ingot.Core.Common.Version;

namespace ingot.Example;

class Program
{
    static void Main(string[] args)
    {
        const string bpUuid = "a8f3c2e1-4b5d-6e7f-8091-a2b3c4d5e6f7";
        const string rpUuid = "b9e4d3c2-5a6b-7c8d-9e0f-b1c2d3e4f5a6";

        string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");

        Pack pack = Pack.Create(
                bpUuid,
                "ingot example",
                "Example pack made with ingot",
                rpUuid)
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
            // loot tables
            // not needed as DenseLasagnaBlock will auto register it for us! :sparkle:
            //.AddLootTable<DenseLasagnaLoot>()
            .AddLootTable<LasagnaSpiritLoot>();

        pack.MinEngineVersion = new Version(1, 21, 0);
        pack.PackIcon = Path.Combine(dataDir, "pack_icon.png");
        pack.ScriptsEnabled = false;
        pack.LinkPacks = false;

        pack.AddBlockTexture("block_of_dense_lasagna", Path.Combine(dataDir, "dense_lasagna.png"))
            .AddItemTexture("lasagna", Path.Combine(dataDir, "lasagna.png"))
            .AddItemTexture("cheese", Path.Combine(dataDir, "cheese.png"))
            .AddItemTexture("pasta", Path.Combine(dataDir, "pasta.png"))
            .AddItemTexture("spooky_special_sauce", Path.Combine(dataDir, "spooky_special_sauce.png"));

        pack.CompileComMojang("/home/pyro/.var/app/io.mrarm.mcpelauncher/data/mcpelauncher/games/com.mojang/", cache: true);
    }
}