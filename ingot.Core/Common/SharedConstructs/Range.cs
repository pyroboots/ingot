using Newtonsoft.Json;

namespace ingot.Core.Common.SharedConstructs;

/// <summary>
/// Defines a spread between two numbers
/// </summary>
public class Range : ICompilableFragment
{
    /// <summary>
    /// Minimum value
    /// </summary>
    public required float Min;
    /// <summary>
    /// Maximum value
    /// </summary>
    public required float Max;
    
    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        if (Max < Min) throw new ArgumentOutOfRangeException(nameof(Max));
        
        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Property("min", Min);
            json.Property("max", Max);
        });
    }
}