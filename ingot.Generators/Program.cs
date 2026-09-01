using ingot.Core.TraitSystem;

namespace ingot.Generators;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        string output = args.Length > 0 ? args[0] : "/home/pyro/Documents/RiderProjects/ingot/ingot.Core/TraitSystem/Traits/Item/";
        
        if (Environment.GetEnvironmentVariable("GH_PAT") is null) throw new ArgumentException("requires github pat token for api usage");
        await TraitGeneratorV2.GenerateItemTraits(output, Environment.GetEnvironmentVariable("GH_PAT")!);
    }
}