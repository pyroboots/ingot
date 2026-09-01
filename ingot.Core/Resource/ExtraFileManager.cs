namespace ingot.Core.Resource;

/// <summary>
/// Arbitrary files copied into a compiled pack at caller-specified relative paths.
/// Used for resource-pack overlays (JSON UI libraries, nineslice metadata, extra textures)
/// and behaviour-pack extras such as Script API helper modules.
/// </summary>
public class ExtraFileManager
{
    internal readonly Dictionary<string, ExtraFileSource> Files = new(StringComparer.Ordinal);

    /// <summary>
    /// Destination paths (forward slashes, relative to the pack root) registered on this manager.
    /// </summary>
    public IReadOnlyCollection<string> RelativePaths => Files.Keys;

    /// <summary>
    /// Registers a single source file copied to <paramref name="relativePath"/> in the pack.
    /// Overwrites a previous registration for the same destination.
    /// </summary>
    /// <param name="sourcePath">Path to the source file on disk (copied as-is).</param>
    /// <param name="relativePath">Destination path relative to the pack root.
    /// Nested paths are allowed (<c>textures/qwo/button/default.png</c>).
    /// <c>.</c> and <c>..</c> segments are rejected.</param>
    public void Add(string sourcePath, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentException("source path cannot be empty", nameof(sourcePath));
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("relative path cannot be empty", nameof(relativePath));

        string dest = NormalizeRelativePath(relativePath, nameof(relativePath));
        Files[dest] = new ExtraFileSource(Path.GetFullPath(sourcePath), dest);
    }

    /// <summary>
    /// Registers every file under <paramref name="sourceDir"/>, preserving relative paths
    /// under optional <paramref name="relativePrefix"/>. Junk files such as <c>Gallery.cache</c>
    /// and dotfiles are skipped.
    /// </summary>
    /// <param name="sourceDir">Directory whose contents should be copied into the pack.</param>
    /// <param name="relativePrefix">Optional prefix prepended to each relative path
    /// (for example <c>ui</c> to copy a folder of screens under <c>ui/</c>).
    /// Omit or pass empty to copy the tree at the pack root.</param>
    public void AddTree(string sourceDir, string? relativePrefix = null)
    {
        if (string.IsNullOrWhiteSpace(sourceDir))
            throw new ArgumentException("source directory cannot be empty", nameof(sourceDir));

        string fullDir = Path.GetFullPath(sourceDir);
        if (!Directory.Exists(fullDir))
            throw new DirectoryNotFoundException($"source directory not found: {fullDir}");

        string prefix = string.IsNullOrWhiteSpace(relativePrefix)
            ? string.Empty
            : NormalizeRelativePath(relativePrefix, nameof(relativePrefix));

        foreach (string file in Directory.EnumerateFiles(fullDir, "*", SearchOption.AllDirectories))
        {
            if (ShouldSkip(file))
                continue;

            string rel = Path.GetRelativePath(fullDir, file).Replace('\\', '/');
            string dest = string.IsNullOrEmpty(prefix) ? rel : $"{prefix}/{rel}";
            Add(file, dest);
        }
    }

    internal IEnumerable<ResourceCopy> EnumerateCopies()
    {
        foreach ((string relative, ExtraFileSource source) in Files)
        {
            yield return new ResourceCopy(
                source.SourcePath,
                source.RelativePath,
                relative,
                "extra file");
        }
    }

    internal static bool ShouldSkip(string filePath)
    {
        string name = Path.GetFileName(filePath);
        if (string.IsNullOrEmpty(name))
            return true;
        if (name[0] == '.')
            return true;
        if (name.Equals("Thumbs.db", StringComparison.OrdinalIgnoreCase))
            return true;
        if (name.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase))
            return true;
        if (name.EndsWith(".cache", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    internal static string NormalizeRelativePath(string relativePath, string paramName)
    {
        string relative = relativePath.Replace('\\', '/').Trim().Trim('/');
        if (string.IsNullOrWhiteSpace(relative))
            throw new ArgumentException("relative path cannot be empty", paramName);

        string[] parts = relative.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (string part in parts)
        {
            if (part is "." or "..")
            {
                throw new ArgumentException(
                    "relative path cannot contain '.' or '..' segments",
                    paramName);
            }
        }

        return string.Join('/', parts);
    }

    internal readonly record struct ExtraFileSource(string SourcePath, string RelativePath);
}
