using Version = ingot.Core.Common.Version;

namespace ingot.Core.TraitSystem;

/// <summary>
/// Declares the minimum content <c>format_version</c> required to use a trait component.
/// Applied on the trait interface alongside <see cref="TraitAttribute"/>.
/// </summary>
/// <remarks>
/// Reflection throws if the content instance's <c>FormatVersion</c> is lower than
/// <see cref="Minimum"/>
/// </remarks>
[AttributeUsage(AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
public sealed class TraitFormatVersionAttribute : Attribute
{
    /// <summary>
    /// Minimum format version required by this trait (e.g. <c>"1.26.0"</c>).
    /// </summary>
    public string Minimum { get; }

    /// <summary>
    /// Marks a trait as requiring at least the given content format version.
    /// </summary>
    /// <param name="minimum">Dotted version string, e.g. <c>"1.26.0"</c>.</param>
    public TraitFormatVersionAttribute(string minimum)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(minimum);
        Minimum = minimum;
    }

    /// <summary>
    /// Parses <see cref="Minimum"/> as a <see cref="Version"/>.
    /// </summary>
    public Version GetMinimumVersion() => new(Minimum);
}
