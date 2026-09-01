namespace ingot.Core.Resource;

/// <summary>
/// Registered particle effect JSON files copied under <c>particles/</c> in the resource pack.
/// </summary>
public class ParticleManager
{
    internal readonly record struct ParticleSource(string SourcePath, string RpName);

    internal readonly Dictionary<string, ParticleSource> Entries = new(StringComparer.Ordinal);

    /// <summary>
    /// Particle effect identifiers registered on this manager.
    /// </summary>
    public IReadOnlyCollection<string> Identifiers => Entries.Keys;

    /// <summary>
    /// Registers a particle effect JSON copied to <c>particles/{rpName}.json</c>.
    /// </summary>
    public void Add(string identifier, string sourceJsonPath, string? rpName = null)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("particle identifier cannot be empty", nameof(identifier));
        if (string.IsNullOrWhiteSpace(sourceJsonPath))
            throw new ArgumentException("source particle json path cannot be empty", nameof(sourceJsonPath));

        Entries[identifier.Trim()] = new ParticleSource(
            Path.GetFullPath(sourceJsonPath),
            rpName ?? ResolveRpName(identifier));
    }

    internal IEnumerable<ResourceCopy> EnumerateCopies()
    {
        foreach ((string identifier, ParticleSource source) in Entries)
        {
            string rpRelative = source.RpName.Replace('\\', '/').Trim('/');
            yield return new ResourceCopy(
                source.SourcePath,
                $"particles/{rpRelative}.json",
                identifier,
                "particle");
        }
    }

    private static string ResolveRpName(string identifier)
    {
        string normalized = identifier.Trim().Replace('\\', '/');
        int colon = normalized.IndexOf(':');
        if (colon >= 0 && colon < normalized.Length - 1)
            normalized = normalized[(colon + 1)..];

        normalized = normalized.Trim().Trim('/');
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException(
                $"particle identifier '{identifier}' does not contain a usable file name",
                nameof(identifier));

        return normalized;
    }
}
