namespace ingot.Generators;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        string blockOutputDir = args.ElementAtOrDefault(0)
            ?? Path.Combine("..", "ingot.Core", "TraitSystem", "Traits", "Block");
        string itemOutputDir = args.ElementAtOrDefault(1)
            ?? Path.Combine("..", "ingot.Core", "TraitSystem", "Traits", "Item");

        Directory.CreateDirectory(blockOutputDir);
        Directory.CreateDirectory(itemOutputDir);

        await using TraitGenerator generator = new();
        await generator.GenerateAllBlockTraitsAsync(blockOutputDir);
        await generator.GenerateAllItemTraitsAsync(itemOutputDir);
    }
}