namespace ingot.Core.TraitSystem;

using static IngotValueConstraintAttribute;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class IngotValueWarningAttribute : Attribute
{
    public Operator Operation;
    public object[] Values;
    public string Warning;
    
    public IngotValueWarningAttribute(Operator op, object[] values, string warning)
    {
        Operation = op;
        Values = values;
        Warning = warning;
    }
}