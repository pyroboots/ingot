using ingot.Core;
using ingot.Core.TraitSystem;
using ingot.Generators;

namespace ingot.Example;

class Program
{
    static void Main(string[] args)
    {
        BehaviourPack bp = BehaviourPack.Create(Guid.NewGuid().ToString())
            .AddItem<LasagnaItem>()
            // not needed as DenseLasagnaBlock will auto register it for us! :sparkle:
            //.AddLootTable<DenseLasagnaLoot>()
            .AddBlock<DenseLasagnaBlock>()
            .AddRecipe<LasagnaRecipe>();
        
        ResourcePack rp = ResourcePack.Create(Guid.NewGuid().ToString())
            .AddBlockTexture("block_of_dense_lasagna", "./dense_lasagna.png")
            .AddItemTexture("lasagna", "./lasagna.png");

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
    }
}