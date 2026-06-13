using ingot.Core.Content.Block;
using ingot.Core.TraitSystem;
using ingot.Generators;

namespace ingot.Example;

class Program
{
    static void Main(string[] args)
    {
        //TraitGenerator generator = new();
        //generator.GenerateAllBlockTraits("/home/pyro/RiderProjects/ingot/ingot.Core/TraitSystem/Traits/Block/");

        string block = Block.Compile<DenseLasagnaBlock>();
        Console.WriteLine(block);
    }
}