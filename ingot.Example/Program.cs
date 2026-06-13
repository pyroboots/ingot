using ingot.Core;

namespace ingot.Example;

class Program
{
    static void Main(string[] args)
    {
        BehaviourPack bp = BehaviourPack.Create(Guid.NewGuid().ToString())
            .AddItem<LasagnaItem>()
            .AddBlock<DenseLasagnaBlock>();
        
        Pack pack = new()
        {
            Description = "Example pack made with ingot",
            Name = "ingot example",
            BehaviourPack = bp,
            ResourcePack = ResourcePack.Create(Guid.NewGuid().ToString()),
            LinkPacks = true,
            ScriptsEnabled = true,
        };
        
        pack.Compile("./");
    }
}