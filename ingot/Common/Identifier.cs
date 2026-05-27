using ingot;
using Newtonsoft.Json;

namespace ingot.Common;

using System;
using System.Text.RegularExpressions;

public sealed class Identifier : IEquatable<Identifier>
{
    public string Namespace { get; }
    public string Name { get; }

    private static readonly Regex ValidPartRegex = new(
        @"^[a-z0-9_]+$", 
        RegexOptions.Compiled);

    public Identifier(string @namespace, string name)
    {
        Namespace = Normalize(@namespace);
        Name = Normalize(name);

        ValidatePart(Namespace, "namespace", nameof(@namespace));
        ValidatePart(Name, "name", nameof(name));
    }

    public Identifier(string fullIdentifier)
    {
        if (string.IsNullOrWhiteSpace(fullIdentifier))
        {
            CompileTimeLogging.Warn(ref _dummyWriter, $"identifier cannot be empty or whitespace");
            Namespace = "minecraft";
            Name = "unknown";
            return;
        }

        string[] parts = fullIdentifier.Split(':', 2, StringSplitOptions.TrimEntries);

        if (parts.Length == 1)
        {
            Namespace = "minecraft";
            Name = Normalize(parts[0]);
        }
        else
        {
            Namespace = Normalize(parts[0]);
            Name = Normalize(parts[1]);
        }

        ValidatePart(Namespace, "namespace", "fullIdentifier");
        ValidatePart(Name, "name", "fullIdentifier");
    }

    private static void ValidatePart(string value, string partName, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            CompileTimeLogging.Warn(ref _dummyWriter, $"identifier {partName} cannot be empty");
            return;
        }

        if (!ValidPartRegex.IsMatch(value))
        {
            CompileTimeLogging.Warn(ref _dummyWriter,
                $"invalid identifier {partName} ({value}) " +
                "only lowercase letters, numbers, underscores are allowed");
        }
    }

    private static string Normalize(string input)
        => string.IsNullOrWhiteSpace(input) ? "" : input.Trim().ToLowerInvariant();

    public static Identifier Vanilla(string name) => new("minecraft", name);
    public static Identifier Parse(string id) => new(id);

    public override string ToString() => $"{Namespace}:{Name}";
    
    public bool Equals(Identifier? other)
        => other is not null && Namespace == other.Namespace && Name == other.Name;

    public override bool Equals(object? obj) => obj is Identifier id && Equals(id);
    public override int GetHashCode() => HashCode.Combine(Namespace, Name);

    public static bool operator ==(Identifier? left, Identifier? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(Identifier? left, Identifier? right) => !(left == right);
    
    private static JsonTextWriter? _dummyWriter;
}