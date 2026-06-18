using Newtonsoft.Json;

namespace ingot.Core.Common;

/// <summary>
/// Internal use implementation of a semantic version
/// </summary>
public class Version : ICompilableFragment
{
    /// <summary>
    /// Major version component.
    /// </summary>
    public int Major;
    /// <summary>
    /// Minor version component.
    /// </summary>
    public int Minor;
    /// <summary>
    /// Patch version component.
    /// </summary>
    public int Patch;

    /// <summary>
    /// Parses a dotted version string (e.g. <c>1.20.10</c>).
    /// </summary>
    /// <param name="version">Dotted version string.</param>
    public Version(string version)
    {
        string[] parts = version.Split('.');
        Major = parts.Length > 0 ? int.Parse(parts[0]) : 0;
        Minor = parts.Length > 1 ? int.Parse(parts[1]) : 0;
        Patch = parts.Length > 2 ? int.Parse(parts[2]) : 0;
    }

    /// <summary>
    /// Creates a version from explicit major, minor, and patch components.
    /// </summary>
    /// <param name="major">Major version component.</param>
    /// <param name="minor">Minor version component.</param>
    /// <param name="patch">Patch version component.</param>
    public Version(int major, int minor, int patch)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Major}.{Minor}.{Patch}";
    
    /// <summary>
    /// Returns the version as a <c>[major, minor, patch]</c> array for manifest JSON.
    /// </summary>
    public int[] AsArray() => new[] { Major, Minor, Patch };
    
    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        writer.WriteStartArray();
        writer.WriteValue(Major);
        writer.WriteValue(Minor);
        writer.WriteValue(Patch);
        writer.WriteEndArray();
    }
}