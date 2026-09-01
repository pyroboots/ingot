using Newtonsoft.Json;

using Formatting = Newtonsoft.Json.Formatting;

namespace ingot.Core.Resource;

/// <summary>
/// A source file that should be copied into the compiled resource pack.
/// </summary>
/// <param name="SourcePath">Absolute or relative path to the file on disk.</param>
/// <param name="RelativePath">Destination path relative to the resource pack root (forward slashes).</param>
/// <param name="DisplayName">Name used in compile errors (identifier, texture key, ...).</param>
/// <param name="Kind">Short kind label used in missing-file errors (e.g. <c>geometry</c>, <c>sound file</c>).</param>
public readonly record struct ResourceCopy(
    string SourcePath,
    string RelativePath,
    string DisplayName,
    string Kind = "file");

internal static class ResourcePackIo
{
    public static void WriteJson(string path, object value, string trace, string? successMessage = null)
    {
        CompilerState.Push(trace);

        string? dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        using StringWriter sw = new();
        using JsonTextWriter writer = new(sw)
        {
            Formatting = Formatting.Indented,
            Indentation = 4,
        };
        JsonSerializer.CreateDefault().Serialize(writer, value);
        File.WriteAllText(path, sw.ToString());

        if (successMessage is not null)
            CompilerState.Info(successMessage);

        CompilerState.Pop();
    }

    public static void CopyFiles(string outputDir, IEnumerable<ResourceCopy> files, string trace)
    {
        Dictionary<string, ResourceCopy> copies = new(StringComparer.OrdinalIgnoreCase);
        foreach (ResourceCopy file in files)
        {
            string relative = file.RelativePath.Replace('\\', '/').Trim().TrimStart('/');
            if (string.IsNullOrWhiteSpace(relative))
                continue;

            if (string.IsNullOrWhiteSpace(file.SourcePath))
                throw new ArgumentException($"{file.Kind} '{file.DisplayName}' has no source registered");

            string sourceFull = Path.GetFullPath(file.SourcePath);
            if (copies.TryGetValue(relative, out ResourceCopy existing))
            {
                string existingSource = Path.GetFullPath(existing.SourcePath);
                if (!string.Equals(existingSource, sourceFull, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"{file.Kind} path '{relative}' is registered from multiple source files: " +
                        $"'{existingSource}' and '{sourceFull}'");
                }

                continue;
            }

            copies[relative] = file with { SourcePath = sourceFull, RelativePath = relative };
        }

        if (copies.Count == 0)
            return;

        CompilerState.Push(trace);
        int c = 0;
        foreach (ResourceCopy file in copies.Values)
        {
            c++;
            if (!File.Exists(file.SourcePath))
            {
                throw new FileNotFoundException(
                    $"source {file.Kind} not found for '{file.DisplayName}': {file.SourcePath}",
                    file.SourcePath);
            }

            string targetFull = Path.Combine(outputDir, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            string? targetDir = Path.GetDirectoryName(targetFull);
            if (!string.IsNullOrEmpty(targetDir))
                Directory.CreateDirectory(targetDir);

            File.Copy(file.SourcePath, targetFull, overwrite: true);
            CompilerState.Info($"({c}/{copies.Count}) registered {file.Kind} '{file.DisplayName}' -> {file.RelativePath}");
        }

        CompilerState.Info($"wrote {copies.Count} {trace}");
        CompilerState.Pop();
    }
}
