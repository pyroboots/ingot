namespace ingot.Core.TraitSystem.Traits.Entity;
using ingot.Core.Common;
using ingot.Core.Behaviour.Entity;

/// <summary>
/// Defines the health pool for an entity, measured in health points (1 point = half a heart). Typical values: cow (10), zombie (20), iron golem (100), wither (600).
/// </summary>
[Trait("minecraft:health", TraitSystem.TraitType.Entity)]
public interface IHealth
{
    /// <summary>
    /// Maximum health this entity can have. Can be higher than the starting value to allow healing beyond initial health.
    /// </summary>
    [TraitProperty]
    public abstract int Max { get; }
    /// <summary>
    /// Starting health for this entity in health points (1 point = half a heart).
    /// </summary>
    [TraitProperty]
    public virtual int Value => Max;
}