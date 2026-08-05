namespace ingot.Generators;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        string output = args.Length > 0 ? args[0] : "/home/pyro/RiderProjects/ingot/ingot.Core/TraitSystem/Traits/Item";
        await TraitGeneratorV2.GenerateItemTraits(output, Environment.GetEnvironmentVariable("gh_api_token")!);
    }
}