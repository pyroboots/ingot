namespace ingot.Core.Resource;

/// <summary>
/// Registered geometry files copied under <c>models/</c> in the resource pack.
/// </summary>
public class GeometryManager
{
    internal readonly record struct GeometrySource(string SourcePath, string RpName, string ModelsSubdir);

    internal readonly Dictionary<string, GeometrySource> Entries = new();

    /// <summary>
    /// Geometry identifiers registered on this manager.
    /// </summary>
    public IReadOnlyCollection<string> Identifiers => Entries.Keys;

    /// <summary>
    /// Whether a geometry identifier is already registered.
    /// </summary>
    public bool Contains(string identifier) => Entries.ContainsKey(identifier);

    /// <summary>
    /// Registers a geometry file (<c>.geo.json</c>) that will be copied under
    /// <c>models/{modelsSubdir}/</c> (default <c>blocks</c>).
    /// </summary>
    public void Add(
        string identifier,
        string sourceGeoJsonPath,
        string? rpName = null,
        string modelsSubdir = "blocks")
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("geometry identifier cannot be empty", nameof(identifier));
        if (string.IsNullOrWhiteSpace(sourceGeoJsonPath))
            throw new ArgumentException("source geo json path cannot be empty", nameof(sourceGeoJsonPath));
        if (string.IsNullOrWhiteSpace(modelsSubdir))
            throw new ArgumentException("models subdir cannot be empty", nameof(modelsSubdir));

        Entries[identifier] = new GeometrySource(
            Path.GetFullPath(sourceGeoJsonPath),
            rpName ?? ResolveRpName(identifier),
            modelsSubdir.Trim().Trim('/', '\\'));
    }

    /// <summary>
    /// Registers an entity geometry file under <c>models/entity/</c>.
    /// </summary>
    public void AddEntity(string identifier, string sourceGeoJsonPath, string? rpName = null) =>
        Add(identifier, sourceGeoJsonPath, rpName, modelsSubdir: "entity");

    internal IEnumerable<ResourceCopy> EnumerateCopies()
    {
        foreach ((string identifier, GeometrySource source) in Entries)
        {
            yield return new ResourceCopy(
                source.SourcePath,
                $"models/{source.ModelsSubdir}/{source.RpName}.geo.json",
                identifier,
                "geometry");
        }
    }

    private static string ResolveRpName(string identifier)
    {
        string normalized = identifier.Trim();
        const string minecraftPrefix = "minecraft:geometry.";
        const string geometryPrefix = "geometry.";

        if (normalized.StartsWith(minecraftPrefix, StringComparison.Ordinal))
            normalized = normalized[minecraftPrefix.Length..];
        else if (normalized.StartsWith(geometryPrefix, StringComparison.Ordinal))
            normalized = normalized[geometryPrefix.Length..];

        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException($"geometry identifier '{identifier}' does not contain a usable file name", nameof(identifier));

        return normalized;
    }
}
