namespace ingot.Core.TraitSystem;

/// <summary>
/// Hard requirement on a trait property value. Reflection throws if the value does not satisfy
/// <see cref="Operation"/> against <see cref="Values"/>.
/// </summary>
/// <remarks>
/// Operator meaning (condition that must hold):
/// <list type="bullet">
/// <item><see cref="Constraint.NotEqual"/> — value must not equal any entry in <see cref="Values"/></item>
/// <item><see cref="Constraint.GreaterThan"/> — value must be strictly greater than every numeric entry</item>
/// <item><see cref="Constraint.LessThan"/> — value must be strictly less than every numeric entry</item>
/// <item><see cref="Constraint.OneOf"/> — value must equal one of the entries</item>
/// </list>
/// For component-level minimum format versions, use <see cref="TraitFormatVersionAttribute"/> on the trait interface.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class TraitPropertyConstraint : Attribute
{
    /// <summary>Comparison / membership operator applied to the reflected property value.</summary>
    public Constraint Operation { get; }

    /// <summary>Operands for <see cref="Operation"/> (thresholds or allowed set).</summary>
    public object[] Values { get; }

    /// <summary>Supported value checks.</summary>
    public enum Constraint
    {
        /// <summary>Value must not equal any of <see cref="Values"/>.</summary>
        NotEqual,

        /// <summary>Numeric value must be strictly greater than every entry in <see cref="Values"/>.</summary>
        GreaterThan,

        /// <summary>Numeric value must be strictly less than every entry in <see cref="Values"/>.</summary>
        LessThan,

        /// <summary>Value must be one of <see cref="Values"/>.</summary>
        OneOf,
        
        /// <summary>Value must be between <see cref="Values"/>[0] and <see cref="Values"/>[1].</summary>
        Range,
    }

    /// <summary>
    /// Declares a hard constraint on a trait property.
    /// </summary>
    /// <param name="op">Operator that must hold for the property value.</param>
    /// <param name="values">Operands for the operator.</param>
    public TraitPropertyConstraint(Constraint op, params object[] values)
    {
        Operation = op;
        Values = values ?? [];
    }
}
