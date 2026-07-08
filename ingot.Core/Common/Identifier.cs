using ingot.Core;

using Newtonsoft.Json;

namespace ingot.Core.Common;

using System.Text.RegularExpressions;

/// <summary>
/// Internal use class to represent a Minecraft identifier
/// </summary>
public class Identifier : IEquatable<Identifier>, ICompilableFragment
{
    /// <summary>
    /// The <c>minecraft</c> part in <c>minecraft:dirt</c>
    /// </summary>
    public string Namespace { get; }
    /// <summary>
    /// The <c>dirt</c> part in <c>minecraft:dirt</c>, or the <c>potion_type</c> part in <c>minecraft:potion_type:awkward</c>
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The <c>awkward</c> part in <c>minecraft:potion_type:awkward</c>
    /// </summary>
    public string? Auxiliary { get; }

    /// <summary>
    /// Whether this identifier has an auxiliary value (e.g. potion types in brewing recipes)
    /// </summary>
    public bool HasAuxiliary => Auxiliary is not null;

    private static readonly Regex ValidPartRegex = new(
        @"^[a-z0-9_]+$",
        RegexOptions.Compiled);

    /// <summary>
    /// Creates an identifier from a namespace and name (e.g. <c>test:my_block</c>).
    /// </summary>
    /// <param name="namespace">The namespace part of the identifier.</param>
    /// <param name="name">The name part of the identifier.</param>
    public Identifier(string @namespace, string name)
        : this(@namespace, name, null)
    {
    }

    /// <summary>
    /// Creates an identifier with an optional auxiliary value (e.g. <c>minecraft:potion_type:awkward</c>).
    /// </summary>
    /// <param name="namespace">The namespace part of the identifier.</param>
    /// <param name="name">The name part of the identifier.</param>
    /// <param name="auxiliary">Optional auxiliary value appended after a second colon.</param>
    public Identifier(string @namespace, string name, string? auxiliary)
    {
        Namespace = Normalize(@namespace);
        Name = Normalize(name);
        Auxiliary = auxiliary is null ? null : Normalize(auxiliary);

        ValidatePart(Namespace, "namespace", nameof(@namespace));
        ValidatePart(Name, "name", nameof(name));
        if (Auxiliary is not null)
            ValidatePart(Auxiliary, "auxiliary", nameof(auxiliary));
    }

    /// <summary>
    /// Parses a full identifier string (e.g. <c>minecraft:dirt</c> or <c>test:item:variant</c>).
    /// </summary>
    /// <param name="fullIdentifier">Colon-separated identifier string.</param>
    public Identifier(string fullIdentifier)
    {
        if (string.IsNullOrWhiteSpace(fullIdentifier))
        {
            CompilerState.Warn(ref _dummyWriter, $"identifier cannot be empty or whitespace");
            Namespace = "minecraft";
            Name = "unknown";
            Auxiliary = null;
            return;
        }

        string[] parts = fullIdentifier.Split(':', StringSplitOptions.TrimEntries);

        switch (parts.Length)
        {
            case 1:
                Namespace = "minecraft";
                Name = Normalize(parts[0]);
                Auxiliary = null;
                break;
            case 2:
                Namespace = Normalize(parts[0]);
                Name = Normalize(parts[1]);
                Auxiliary = null;
                break;
            default:
                Namespace = Normalize(parts[0]);
                Name = Normalize(parts[1]);
                Auxiliary = Normalize(parts[2]);
                if (parts.Length > 3)
                {
                    CompilerState.Warn(ref _dummyWriter,
                        "identifier has more than 3 colon-separated parts; extra parts are ignored");
                }
                break;
        }

        ValidatePart(Namespace, "namespace", "fullIdentifier");
        ValidatePart(Name, "name", "fullIdentifier");
        if (Auxiliary is not null)
            ValidatePart(Auxiliary, "auxiliary", "fullIdentifier");
    }

    private static void ValidatePart(string value, string partName, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            CompilerState.Warn(ref _dummyWriter, $"identifier {partName} cannot be empty");
            return;
        }

        if (!ValidPartRegex.IsMatch(value))
        {
            CompilerState.Warn(ref _dummyWriter,
                $"invalid identifier {partName} ({value}) " +
                "only lowercase letters, numbers, underscores are allowed");
        }
    }

    private static string Normalize(string input)
        => string.IsNullOrWhiteSpace(input) ? "" : input.Trim().ToLowerInvariant();

    /// <summary>
    /// Creates a vanilla <c>minecraft:</c> identifier.
    /// </summary>
    /// <param name="name">The vanilla item, block, or entity name.</param>
    public static Identifier Vanilla(string name) => new("minecraft", name);

    /// <summary>
    /// Creates a vanilla <c>minecraft:</c> identifier with an auxiliary value.
    /// </summary>
    /// <param name="type">The vanilla type name (e.g. <c>potion_type</c>).</param>
    /// <param name="value">The auxiliary value (e.g. <c>awkward</c>).</param>
    public static Identifier VanillaAuxiliary(string type, string value) => new("minecraft", type, value);

    /// <summary>
    /// Parses a full identifier string into an <see cref="Identifier"/>.
    /// </summary>
    /// <param name="id">Colon-separated identifier string.</param>
    public static Identifier Parse(string id) => new(id);

    /// <summary>
    /// Returns a new identifier in the same namespace with <paramref name="suffix"/> appended to the name
    /// (e.g. <c>test:custom_cow</c> + <c>_baby</c> → <c>test:custom_cow_baby</c>).
    /// </summary>
    public Identifier WithNameSuffix(string suffix) =>
        new(Namespace, Name + suffix, Auxiliary);

    /// <summary>
    /// Returns a new identifier in the same namespace with a different name.
    /// </summary>
    public Identifier WithName(string name) => new(Namespace, name, Auxiliary);

    /// <inheritdoc/>
    public override string ToString()
        => Auxiliary is null ? $"{Namespace}:{Name}" : $"{Namespace}:{Name}:{Auxiliary}";

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer) => writer.WriteValue(ToString());

    /// <inheritdoc/>
    public bool Equals(Identifier? other)
        => other is not null
           && Namespace == other.Namespace
           && Name == other.Name
           && Auxiliary == other.Auxiliary;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Identifier id && Equals(id);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Namespace, Name, Auxiliary);

    /// <summary>Determines whether two identifiers are equal.</summary>
    public static bool operator ==(Identifier? left, Identifier? right) => left?.Equals(right) ?? right is null;

    /// <summary>Determines whether two identifiers are not equal.</summary>
    public static bool operator !=(Identifier? left, Identifier? right) => !(left == right);

    private static JsonTextWriter? _dummyWriter;
}