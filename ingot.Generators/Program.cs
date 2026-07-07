namespace ingot.Generators;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        string blockOutputDir = args.ElementAtOrDefault(0) ?? Path.Combine("..", "ingot.Core", "TraitSystem", "Traits", "Block");
        string itemOutputDir = args.ElementAtOrDefault(1) ?? Path.Combine("..", "ingot.Core", "TraitSystem", "Traits", "Item");
        string entityOutputDir = args.ElementAtOrDefault(1) ?? Path.Combine("..", "ingot.Core", "TraitSystem", "Traits", "Entity");

        Directory.CreateDirectory(blockOutputDir);
        Directory.CreateDirectory(itemOutputDir);

        await using TraitGenerator generator = new();
        await generator.GenerateAllBlockTraitsAsync(blockOutputDir);
        await generator.GenerateAllItemTraitsAsync(itemOutputDir);
        await generator.GenerateAllEntityTraitsAsync("/home/pyro/RiderProjects/ingot/ingot.Core/TraitSystem/Traits/Entity");
        await generator.GenerateAllEntityBehaviourTraitsAsync("/home/pyro/RiderProjects/ingot/ingot.Core/TraitSystem/Traits/Entity/Behaviour");
    }
}