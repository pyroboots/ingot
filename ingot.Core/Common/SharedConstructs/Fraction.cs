using Newtonsoft.Json;

namespace ingot.Core.Common.SharedConstructs;

/// <summary>
/// Defines a fraction using a numerator and denominator
/// </summary>
public class Fraction : ICompilableFragment
{
    /// <summary>
    /// Numerator value of fraction
    /// </summary>
    public required int Numerator;
    /// <summary>
    /// Denominator value of fraction
    /// </summary>
    public required int Denominator;
    
    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        if (Numerator == 1 || Denominator == 1)
            throw new ArgumentException("cannot be equal to 1");
        
        // minecraft wont allow according to the wiki:
        // https://wiki.bedrock.dev/documentation/shared-constructs#fraction-objects
        // "the denominator cannot be equal to the numerator."
        if (Denominator == Numerator)
            throw new ArgumentException("denominator cannot be equal to numerator");
        
        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Property("numerator", Numerator);
            json.Property("denominator", Denominator);
        });
    }
}