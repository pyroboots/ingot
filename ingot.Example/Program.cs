using ingot.Core;

namespace ingot.Example;

class Program
{
    static void Main(string[] args)
    {
        BehaviourPack bp = BehaviourPack.Create(Guid.NewGuid().ToString())
            .AddItem<LasagnaItem>()
            .AddBlock<DenseLasagnaBlock>();
        
        ResourcePack rp = ResourcePack.Create(Guid.NewGuid().ToString())
            .AddBlockTexture("block_of_dense_lasagna", "ingot.Example/assets/block_of_dense_lasagna.png")
            .AddItemTexture("lasagna", "ingot.Example/assets/lasagna.png");

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