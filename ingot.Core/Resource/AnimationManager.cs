namespace ingot.Core.Resource;

/// <summary>
/// Registered animation JSON files copied under <c>animations/</c> in the resource pack.
/// </summary>
public class AnimationManager
{
    internal readonly Dictionary<string, string> Entries = new(StringComparer.Ordinal);

    /// <summary>
    /// Animation file names (without extension) registered on this manager.
    /// </summary>
    public IReadOnlyCollection<string> Names => Entries.Keys;

    /// <summary>
    /// Registers an animation JSON file copied to <c>animations/{rpName}.json</c>.
    /// </summary>
    public void Add(string sourceJsonPath, string rpName)
    {
        if (string.IsNullOrWhiteSpace(sourceJsonPath))
            throw new ArgumentException("source animation path cannot be empty", nameof(sourceJsonPath));
        if (string.IsNullOrWhiteSpace(rpName))
            throw new ArgumentException("animation rp name cannot be empty", nameof(rpName));

        Entries[rpName.Trim()] = Path.GetFullPath(sourceJsonPath);
    }

    internal IEnumerable<ResourceCopy> EnumerateCopies()
    {
        foreach ((string rpName, string sourcePath) in Entries)
        {
            yield return new ResourceCopy(
                sourcePath,
                $"animations/{rpName}.json",
                rpName,
                "animation");
        }
    }
}
