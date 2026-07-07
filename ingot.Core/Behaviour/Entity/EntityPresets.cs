using ingot.Core.TraitSystem.Traits.Entity;

namespace ingot.Core.Behaviour.Entity;

/// <summary>
/// Preset entity interface that implements boilerplate entity properties
/// </summary>
public interface IBasicEntity : ITypeFamily, IHealth, IDespawn, IPushable, IPhysics, ICollisionBox;

/// <summary>
/// Preset entity interface that implements typical behaviours and properties of a passive mob
/// </summary>
public interface IEntityBehaviourPresetPassive : IBasicEntity,
    IMovement,
    //IBehaviourFloat,
    //IBehaviourRandomStroll,
    //IBehaviourRandomLookAround,
    INavigationWalk,
    IMovementBasic,
    IJumpStatic;