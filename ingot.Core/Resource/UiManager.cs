using Newtonsoft.Json;

namespace ingot.Core.Resource;

/// <summary>
/// Registered JSON UI files copied under <c>ui/</c> in the resource pack,
/// plus generated <c>ui/_ui_defs.json</c> entries for custom namespaces.
/// </summary>
public class UiManager
{
    internal readonly record struct UiSource(string SourcePath, string RpName, string Extension, bool IncludeInUiDefs);

    internal readonly Dictionary<string, UiSource> Entries = new(StringComparer.Ordinal);

    /// <summary>
    /// UI file names (without extension, relative to <c>ui/</c>) registered on this manager.
    /// </summary>
    public IReadOnlyCollection<string> Names => Entries.Keys;

    /// <summary>
    /// Registers a JSON UI file copied to <c>ui/{rpName}{extension}</c>.
    /// <c>.jsonc</c> sources keep that extension so libraries such as StarLib can be listed
    /// from <c>_ui_defs.json</c> with their original paths. Other sources copy as <c>.json</c>.
    /// Custom namespace files are listed in generated <c>ui/_ui_defs.json</c> unless
    /// <paramref name="includeInUiDefs"/> is <see langword="false"/>.
    /// </summary>
    /// <param name="sourceJsonPath">Path to the source UI JSON on disk (copied as-is).</param>
    /// <param name="rpName">Filename without extension under <c>ui/</c>. Nested paths are allowed
    /// (<c>custom/menu</c> → <c>ui/custom/menu.json</c>). Defaults to the source file name.
    /// A trailing <c>.json</c> or <c>.jsonc</c> on this value is stripped and also selects
    /// the output extension.</param>
    /// <param name="includeInUiDefs">
    /// When <see langword="true"/>, the file is listed in <c>_ui_defs.json</c>.
    /// Defaults to <see langword="true"/> except for system files whose names start with <c>_</c>
    /// (for example <c>_ui_defs</c>, <c>_global_variables</c>). Pass <see langword="false"/> when
    /// overlaying a vanilla screen such as <c>hud_screen</c>.
    /// </param>
    public void Add(string sourceJsonPath, string? rpName = null, bool? includeInUiDefs = null)
    {
        if (string.IsNullOrWhiteSpace(sourceJsonPath))
            throw new ArgumentException("source ui json path cannot be empty", nameof(sourceJsonPath));

        string outputExt = ResolveExtension(Path.GetExtension(sourceJsonPath));

        string resolvedRpName = (rpName ?? Path.GetFileNameWithoutExtension(sourceJsonPath))
            .Replace('\\', '/')
            .Trim()
            .Trim('/');
        if (string.IsNullOrWhiteSpace(resolvedRpName))
            throw new ArgumentException("ui rp name cannot be empty", nameof(rpName));

        if (resolvedRpName.EndsWith(".jsonc", StringComparison.OrdinalIgnoreCase))
        {
            resolvedRpName = resolvedRpName[..^6];
            outputExt = ".jsonc";
        }
        else if (resolvedRpName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            resolvedRpName = resolvedRpName[..^5];
            outputExt = ".json";
        }

        if (string.IsNullOrWhiteSpace(resolvedRpName))
            throw new ArgumentException("ui rp name cannot be empty", nameof(rpName));

        bool include = includeInUiDefs ?? !IsSystemUiFile(resolvedRpName);
        Entries[resolvedRpName] = new UiSource(
            Path.GetFullPath(sourceJsonPath),
            resolvedRpName,
            outputExt,
            include);
    }

    internal IEnumerable<ResourceCopy> EnumerateCopies()
    {
        foreach ((string name, UiSource source) in Entries)
        {
            yield return new ResourceCopy(
                source.SourcePath,
                $"ui/{source.RpName}{source.Extension}",
                name,
                "ui");
        }
    }

    internal bool HasProvidedUiDefs =>
        Entries.Keys.Any(name => name.Equals("_ui_defs", StringComparison.OrdinalIgnoreCase));

    internal void WriteUiDefs(string resourcePackDir)
    {
        if (HasProvidedUiDefs)
            return;

        string[] defs = Entries.Values
            .Where(source => source.IncludeInUiDefs)
            .Select(source => $"ui/{source.RpName}{source.Extension}")
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        if (defs.Length == 0)
            return;

        ResourcePackIo.WriteJson(
            Path.Combine(resourcePackDir, "ui", "_ui_defs.json"),
            new UiDefsDocument(defs),
            "_ui_defs.json",
            $"wrote _ui_defs.json with {defs.Length} namespace file(s)");
    }

    private static bool IsSystemUiFile(string rpName) =>
        Path.GetFileName(rpName).StartsWith('_');

    private static string ResolveExtension(string sourceExtension) =>
        sourceExtension.Equals(".jsonc", StringComparison.OrdinalIgnoreCase)
            ? ".jsonc"
            : ".json";

    private sealed class UiDefsDocument(string[] uiDefs)
    {
        [JsonProperty("ui_defs")]
        public string[] UiDefs { get; } = uiDefs;
    }
}
