namespace ingot.Tests.Support;

internal static class CompileTestHelper
{
    public static string CreateOutputDirectory()
    {
        string outputDir = Path.Combine(Path.GetTempPath(), "ingot-tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(outputDir);
        return outputDir;
    }

    public static void DeleteOutputDirectory(string outputDir)
    {
        if (Directory.Exists(outputDir))
            Directory.Delete(outputDir, recursive: true);
    }
}