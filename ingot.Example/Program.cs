using ingot.Core;

namespace ingot.Example;

class Program
{
    static void Main(string[] args)
    {
        Pack pack = Pack.Create(Guid.NewGuid().ToString(), "ingot example", "Example pack made with ingot")
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

        pack.ScriptsEnabled = true;
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