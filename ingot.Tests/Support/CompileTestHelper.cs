namespace ingot.Tests.Support;

internal static class CompileTestHelper
{
    public static TempOutputDirectory CreateTempDirectory()
    {
        string outputDir = Path.Combine(Path.GetTempPath(), "ingot-tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(outputDir);
        return new TempOutputDirectory(outputDir);
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