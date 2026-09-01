using ingot.Core.Common;

using Newtonsoft.Json;

using Formatting = ingot.Core.Common.Formatting;

namespace ingot.Core.Behaviour.Entity;

/// <summary>
/// C# representation of an entity filter
/// </summary>
public class EntityFilter : ICompilableFragment
{
    /// <summary>
    /// Array of <see cref="EntityFilter"/> where all have to be <c>true</c> to pass
    /// </summary>
    public EntityFilter[] AllOf = [];
    /// <summary>
    /// Array of <see cref="EntityFilter"/> where any can be <c>true</c> to pass
    /// </summary>
    public EntityFilter[] AnyOf = [];
    /// <summary>
    /// Array of <see cref="EntityFilter"/> where none have to be <c>true</c> to pass
    /// </summary>
    public EntityFilter[] NoneOf = [];

    /// <summary>
    /// Instigator to run the test on
    /// </summary>
    public required Enums.Target Subject;
    /// <summary>
    /// Which mathematical operator to use
    /// </summary>
    public required string Operator;
    /// <summary>
    /// Test to run
    /// </summary>
    public required string Test;
    /// <summary>
    /// Value to test against. <c>Test (operator) Value</c>
    /// </summary>
    public required dynamic Value;

    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Property(Formatting.PascalToSnakeCase(nameof(AllOf)), AllOf);
            json.Property(Formatting.PascalToSnakeCase(nameof(AnyOf)), AnyOf);
            json.Property(Formatting.PascalToSnakeCase(nameof(NoneOf)), NoneOf);
            
            json.Property(Formatting.PascalToSnakeCase(nameof(Subject)), Enums.Target_AsString(Subject));
            json.Property(Formatting.PascalToSnakeCase(nameof(Operator)), Operator);
            json.Property(Formatting.PascalToSnakeCase(nameof(Test)), Test);
            json.Property(Formatting.PascalToSnakeCase(nameof(Value)), Value);
            
        });
    }
}