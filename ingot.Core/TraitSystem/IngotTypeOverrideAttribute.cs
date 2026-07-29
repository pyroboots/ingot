namespace ingot.Core.TraitSystem;

/// <summary>
/// Overrides a trait property value at reflection time
/// Useful for when certain trait properties accept multiple types
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class IngotTypeOverrideAttribute : Attribute
{
    /// <summary>
    /// The value to set the override the default with
    /// </summary>
    public readonly object? OverrideValue;
    /// <summary>
    /// Overrides a trait property value at reflection time
    /// Useful for when certain trait properties accept multiple types
    /// <param name="overrideValue">The value to set the override the default with</param>
    /// </summary>
    public IngotTypeOverrideAttribute(object? overrideValue) => OverrideValue = overrideValue;
}