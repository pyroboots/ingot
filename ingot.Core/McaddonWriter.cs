using System.IO.Compression;

namespace ingot.Core;

internal static class McaddonWriter
{
    internal static void Write(string outputPath, string compileDir, string packName)
    {
        string? outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir))
            Directory.CreateDirectory(outputDir);

        if (File.Exists(outputPath))
            File.Delete(outputPath);

        using ZipArchive archive = ZipFile.Open(outputPath, ZipArchiveMode.Create);

        AddDirectory(archive, Path.Combine(compileDir, "bp"), $"{packName} BP");
        AddDirectory(archive, Path.Combine(compileDir, "rp"), $"{packName} RP");
    }

    private static void AddDirectory(ZipArchive archive, string sourceDir, string entryRoot)
    {
        foreach (string file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(sourceDir, file);
            string entryName = $"{entryRoot}/{relative.Replace('\\', '/')}";
            archive.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
        }
    }
}