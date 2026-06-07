using ingot.Core.TraitSystem;
using ingot.Generators;

namespace ingot.Test;

class Program
{
    static void Main(string[] args)
    {
        //TraitGenerator.GenerateAllTraits("/home/pyro/RiderProjects/ingot/ingot.Core/TraitSystem/Traits/Item/");

        Console.WriteLine(Item.Compile<Lasagna>());
    }
}