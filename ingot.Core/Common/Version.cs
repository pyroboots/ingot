using Newtonsoft.Json;

namespace ingot.Core.Common;

/// <summary>
/// Internal use implementation of a semantic version
/// </summary>
public class Version : ICompilableFragment, IComparable<Version>
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
    public void Compile(ref JsonWriter writer)
    {
        writer.WriteStartArray();
        writer.WriteValue(Major);
        writer.WriteValue(Minor);
        writer.WriteValue(Patch);
        writer.WriteEndArray();
    }
    
    /// <summary>Component-wise less-than comparison.</summary>
    public static bool operator <(Version? left, Version? right)
    {
        if (ReferenceEquals(left, right))
            return false;
        if (left is null)
            return true;
        return left.CompareTo(right) < 0;
    }

    /// <summary>Component-wise greater-than comparison.</summary>
    public static bool operator >(Version? left, Version? right) => right < left;

    /// <summary>Component-wise less-than-or-equal comparison.</summary>
    public static bool operator <=(Version? left, Version? right) => !(right < left);

    /// <summary>Component-wise greater-than-or-equal comparison.</summary>
    public static bool operator >=(Version? left, Version? right) => !(left < right);

    /// <summary>Component-wise equality.</summary>
    public static bool operator ==(Version? left, Version? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;
        return left.CompareTo(right) == 0;
    }

    /// <summary>Component-wise inequality.</summary>
    public static bool operator !=(Version? left, Version? right) => !(left == right);

    /// <inheritdoc/>
    public int CompareTo(Version? other)
    {
        if (ReferenceEquals(this, other))
            return 0;

        if (other is null)
            return 1;

        int majorComparison = Major.CompareTo(other.Major);
        if (majorComparison != 0)
            return majorComparison;

        int minorComparison = Minor.CompareTo(other.Minor);
        if (minorComparison != 0)
            return minorComparison;

        return Patch.CompareTo(other.Patch);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Version other && CompareTo(other) == 0;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch);
}