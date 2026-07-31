namespace ingot.Core.TraitSystem;

using static TraitPropertyConstraintAttribute;

/// <summary>
/// Soft check on a trait property value. Reflection emits a compile warning when the value
/// <em>matches</em> <see cref="Operation"/> against <see cref="Values"/> (does not throw).
/// </summary>
/// <remarks>
/// Unlike <see cref="TraitPropertyConstraintAttribute"/> (which requires the condition to hold),
/// a warning fires when the condition holds. Example: <see cref="Constraint.OneOf"/> with a list of
/// broken animation names warns if the property is one of those names.
/// Use <c>{x}</c> in <see cref="Warning"/> as a placeholder for the property value.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class TraitPropertyWarningAttribute : Attribute
{
    /// <summary>Comparison / membership operator that triggers the warning when it matches.</summary>
    public Constraint Operation { get; }

    /// <summary>Operands for <see cref="Operation"/>.</summary>
    public object[] Values { get; }

    /// <summary>Warning message. <c>{x}</c> is replaced with the property value.</summary>
    public string Warning { get; }

    /// <summary>
    /// Declares a soft warning when a trait property value matches the operator condition.
    /// </summary>
    /// <param name="op">Operator that triggers the warning when it matches the value.</param>
    /// <param name="values">Operands for the operator.</param>
    /// <param name="warning">Message to emit; <c>{x}</c> is replaced with the value.</param>
    public TraitPropertyWarningAttribute(string warning, Constraint op, params object[] values)
    {
        Operation = op;
        Values = values ?? [];
        Warning = warning;
    }
}
