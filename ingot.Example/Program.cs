using ingot.Core;
using ingot.Example.Blocks;
using ingot.Example.Entities;
using ingot.Example.Items;
using ingot.Example.Recipes;

using Version = ingot.Core.Common.Version;

namespace ingot.Example;

class Program
{
    static async Task Main(string[] args)
    {
        const string bpUuid = "a8f3c2e1-4b5d-6e7f-8091-a2b3c4d5e6f7";
        const string rpUuid = "b9e4d3c2-5a6b-7c8d-9e0f-b1c2d3e4f5a6";

        string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");

        Pack pack = Pack.Create(
                bpUuid,
                "ingot example",
                "Example pack made with ingot",
                rpUuid)
            .AddItem<LasagnaItem>()
            .AddItem<CheeseItem>()
            .AddItem<PastaItem>()
            .AddItem<SauceItem>()
            .AddBlock<DenseLasagnaBlock>()
            .AddEntity<CowEntity>();

        pack.MinEngineVersion = new Version(1, 21, 0);
        pack.PackIcon = Path.Combine(dataDir, "pack_icon.png");
        pack.ScriptsEnabled = true;
        pack.LinkPacks = false;

        string scriptsDir = Path.Combine(AppContext.BaseDirectory, "scripts");
        pack.AddService(Path.Combine(scriptsDir, "services", "tick_service.js"), intervalTicks: 20);
        
        pack.CompileComMojang("/home/pyro/.var/app/io.mrarm.mcpelauncher/data/mcpelauncher/games/com.mojang/", cache: true);
    }
}