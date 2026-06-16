using ingot.Core;
using Newtonsoft.Json;

namespace ingot.Core.Common;

using System.Text.RegularExpressions;

/// <summary>
/// Internal use class to represent a Minecraft identifier
/// </summary>
public class Identifier : IEquatable<Identifier>, ICompileableFragment
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

    public Identifier(string @namespace, string name)
        : this(@namespace, name, null)
    {
    }

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

    public static Identifier Vanilla(string name) => new("minecraft", name);
    public static Identifier VanillaAuxiliary(string type, string value) => new("minecraft", type, value);
    public static Identifier Parse(string id) => new(id);

    public override string ToString()
        => Auxiliary is null ? $"{Namespace}:{Name}" : $"{Namespace}:{Name}:{Auxiliary}";

    public void Compile(ref JsonTextWriter writer) => writer.WriteValue(ToString());
    
    public bool Equals(Identifier? other)
        => other is not null
           && Namespace == other.Namespace
           && Name == other.Name
           && Auxiliary == other.Auxiliary;

    public override bool Equals(object? obj) => obj is Identifier id && Equals(id);
    public override int GetHashCode() => HashCode.Combine(Namespace, Name, Auxiliary);

    public static bool operator ==(Identifier? left, Identifier? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(Identifier? left, Identifier? right) => !(left == right);
    
    private static JsonTextWriter? _dummyWriter;
}