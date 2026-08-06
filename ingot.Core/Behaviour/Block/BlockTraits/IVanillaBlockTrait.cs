using ingot.Core.Common;

using Version = ingot.Core.Common.Version;

namespace ingot.Core.Behaviour.Block.BlockTraits;

/// <summary>
/// Represents a vanilla block trait that applies vanilla block states under
/// <c>minecraft:block/description/traits</c>.
/// Not to be confused with component traits (<see cref="TraitSystem.Traits.IBlockTrait"/>)
/// </summary>
public interface IVanillaBlockTrait : ICompilableFragment
{
    /// <summary>
    /// Identifier of this vanilla block trait (e.g. <c>minecraft:placement_direction</c>)
    /// </summary>
    Identifier Identifier { get; }

    /// <summary>
    /// Minimum block <c>format_version</c> required to use this trait
    /// </summary>
    Version MinimumFormatVersion { get; }

    /// <summary>
    /// Array of states this vanilla block trait enables
    /// </summary>
    Identifier[] EnabledStates { get; }

    /// <summary>
    /// Metadata for the block states this vanilla block trait can provide.
    /// Values are restricted to <see cref="int"/>,
    /// <see cref="bool"/>, or <see cref="string"/> arrays (same rules as <see cref="Block.States"/>)
    /// </summary>
    ProvidedState[] ProvidedStates { get; }
}

/// <summary>
/// Represents a block state and its possible values.
/// </summary>
public readonly struct ProvidedState
{
    /// <summary>
    /// State identifier
    /// </summary>
    public Identifier State { get; }

    /// <summary>
    /// Possible values. Must consist of <see cref="int"/>,
    /// <see cref="bool"/>, or <see cref="string"/> entries only
    /// </summary>
    public object[] Values { get; }

    /// <summary>
    /// Creates a provided-state descriptor and validates value types
    /// </summary>
    /// <param name="state">State identifier</param>
    /// <param name="values">Possible values (non-empty int/bool/string only)</param>
    public ProvidedState(Identifier state, object[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length == 0)
            throw new ArgumentException("ProvidedState values cannot be empty", nameof(values));

        Type? elementType = null;
        for (int i = 0; i < values.Length; i++)
        {
            object? value = values[i];
            if (value is null)
                throw new ArgumentException("ProvidedState values cannot contain null", nameof(values));

            Type t = value.GetType();
            if (t != typeof(int) && t != typeof(float) && t != typeof(bool) && t != typeof(string))
            {
                throw new ArgumentException(
                    $"ProvidedState values must be int, bool, or string (got {t.Name} at index {i})",
                    nameof(values));
            }

            elementType ??= t;
            if (t != elementType)
            {
                throw new ArgumentException(
                    $"ProvidedState values must be type consistent (mixed {elementType.Name} and {t.Name})",
                    nameof(values));
            }
        }

        State = state;
        Values = values;
    }

    /// <summary>
    /// Shortcut for a boolean state's possible values
    /// </summary>
    public static object[] BooleanValues => [true, false];

    /// <summary>
    /// Shortcut for a cardinal direction state's possible values
    /// </summary>
    public static object[] CardinalDirectionValues => ["south", "west", "north", "east"];
}
