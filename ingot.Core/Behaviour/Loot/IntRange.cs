using ingot.Core.Common;
using Newtonsoft.Json;

namespace ingot.Core.Behaviour.Loot;

/// <summary>
/// An integer value or min/max range used by loot table rolls and functions
/// </summary>
public record IntRange : ICompileableFragment
{
    /// <summary>
    /// Minimum value
    /// </summary>
    public int Min { get; init; }
    /// <summary>
    /// Maximum value
    /// </summary>
    public int Max { get; init; }

    /// <summary>
    /// Creates an exact integer value
    /// </summary>
    public IntRange(int value)
    {
        Min = value;
        Max = value;
    }

    /// <summary>
    /// Creates a min/max range
    /// </summary>
    public IntRange(int min, int max)
    {
        Min = min;
        Max = max;
    }

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        if (Min == Max)
            writer.WriteValue(Min);
        else
        {
            writer.WriteStartObject();
            writer.WritePropertyName("min");
            writer.WriteValue(Min);
            writer.WritePropertyName("max");
            writer.WriteValue(Max);
            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// Implicit conversion from a single integer
    /// </summary>
    public static implicit operator IntRange(int value) => new(value);
}