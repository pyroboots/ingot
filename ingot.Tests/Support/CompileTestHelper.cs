using ingot.Core;
using ingot.Core.Behaviour.Item;

namespace ingot.Tests.Support;

internal static class CompileTestHelper
{
    public static TempOutputDirectory CreateTempDirectory()
    {
        string outputDir = Path.Combine(Path.GetTempPath(), "ingot-tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(outputDir);
        return new TempOutputDirectory(outputDir);
    }

    public static string CompileItemJson<TItem>() where TItem : Item, new()
    {
        using TempOutputDirectory output = CreateTempDirectory();
        Pack pack = PackTestBuilder.Create().AddItem<TItem>();
        pack.Compile(output.Path, verbose: false);

        string fileName = pack.BehaviourPack.Items[0].Identifier.Name + ".json";
        return File.ReadAllText(Path.Combine(output.Path, "bp", "items", fileName));
    }

    public static void DeleteOutputDirectory(string outputDir)
    {
        if (Directory.Exists(outputDir))
            Directory.Delete(outputDir, recursive: true);
    }
}

internal sealed class TempOutputDirectory(string path) : IDisposable
{
    public string Path { get; } = path;

    public void Dispose() => CompileTestHelper.DeleteOutputDirectory(Path);
}