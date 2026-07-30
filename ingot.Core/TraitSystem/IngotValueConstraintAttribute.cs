using ingot.Core.Common;

namespace ingot.Core.TraitSystem;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class IngotValueConstraintAttribute : Attribute
{
    public Operator Operation;
    public object[] Values;
    
    public enum Operator
    {
        NotEqual,
        GreaterThan,
        LessThan,
        OneOf,
        MinVer,
    }
    public IngotValueConstraintAttribute(Operator op, object[] values)
    {
        Operation = op;
        Values = values;
    }
}