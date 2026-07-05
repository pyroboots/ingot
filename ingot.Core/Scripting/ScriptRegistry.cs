namespace ingot.Core.Scripting;

/// <summary>
/// Deduplicated registry of behaviour-pack scripts to write during compilation.
/// </summary>
internal sealed class ScriptRegistry
{
    private readonly Dictionary<string, ScriptEntry> _entries = new(StringComparer.Ordinal);

    /// <summary>Whether any scripts are registered.</summary>
    public bool HasEntries => _entries.Count > 0;

    /// <summary>Registered scripts in deterministic path order.</summary>
    public IReadOnlyList<ScriptEntry> Entries =>
        _entries.Values.OrderBy(e => e.RelativePath, StringComparer.Ordinal).ToArray();

    /// <summary>Registers generated script content.</summary>
    public void RegisterGenerated(string relativePath, string content)
    {
        string normalized = NormalizeRelativePath(relativePath);
        _entries[normalized] = new ScriptEntry(normalized, ScriptEntryKind.Generated, content, null);
    }

    /// <summary>Registers a service script copied from a source file.</summary>
    public void RegisterService(string sourceFile, string relativePath)
    {
        string normalized = NormalizeRelativePath(relativePath);
        string resolvedSource = Path.GetFullPath(sourceFile);
        _entries[normalized] = new ScriptEntry(normalized, ScriptEntryKind.Service, null, resolvedSource);
    }

    /// <summary>Clears all registered scripts.</summary>
    public void Clear() => _entries.Clear();

    private static string NormalizeRelativePath(string relativePath) =>
        relativePath.Replace('\\', '/');
}

/// <summary>
/// A script entry stored in <see cref="ScriptRegistry"/>.
/// </summary>
internal readonly record struct ScriptEntry(
    string RelativePath,
    ScriptEntryKind Kind,
    string? GeneratedContent,
    string? SourceFilePath);

/// <summary>
/// Kind of script entry in the registry.
/// </summary>
internal enum ScriptEntryKind
{
    /// <summary>Generated Script API component registration script.</summary>
    Generated,
    /// <summary>Copied service script that runs every tick.</summary>
    Service,
}