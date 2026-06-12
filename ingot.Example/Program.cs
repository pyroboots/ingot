using ingot.Core.TraitSystem;
using ingot.Generators;

namespace ingot.Example;

class Program
{
    static void Main(string[] args)
    {
        //TraitGenerator.GenerateAllItemTraits("/home/pyro/RiderProjects/ingot/ingot.Core/TraitSystem/Traits/Item/");

        string block = Block.Compile<DenseLasagnaBlock>();
    }
}