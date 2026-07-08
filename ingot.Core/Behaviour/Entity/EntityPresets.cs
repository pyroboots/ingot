using ingot.Core.TraitSystem.Traits.Entity;

namespace ingot.Core.Behaviour.Entity;

/// <summary>
/// Preset entity interface that implements boilerplate entity properties
/// </summary>
public interface IBasicEntity : 
    ITypeFamily, 
    IHealth, 
    IDespawn, 
    IPushable, 
    IPhysics, 
    ICollisionBox;

/// <summary>
/// Preset entity interface that implements typical behaviours and properties of a passive land mob
/// </summary>
public interface IEntityPresetPassive : IBasicEntity,
    IMovement,
    IBehaviorFloat,
    IBehaviorRandomStroll,
    IBehaviorRandomLookAround,
    INavigationWalk,
    IMovementBasic,
    IJumpStatic;

/// <summary>
/// Preset entity interface that implements typical behaviours of a passive land mob that flees from threats
/// </summary>
public interface IEntityPresetTimid : IEntityPresetPassive,
    IBehaviorPanic,
    IBehaviorAvoidMobType;

/// <summary>
/// Preset entity interface that implements typical behaviours of a neutral land mob that retaliates when attacked
/// </summary>
public interface IEntityPresetNeutral : IEntityPresetPassive,
    IAttack,
    IFollowRange,
    IBehaviorHurtByTarget,
    IBehaviorMeleeAttack,
    IBehaviorMoveTowardsTarget,
    IBehaviorLookAtTarget;

/// <summary>
/// Preset entity interface that implements typical behaviours of a hostile land mob that actively seeks and attacks targets
/// </summary>
public interface IEntityPresetHostile : IEntityPresetNeutral,
    IBehaviorNearestAttackableTarget;

/// <summary>
/// Preset entity interface that implements typical behaviours of a tameable passive land mob
/// </summary>
public interface IEntityPresetTameable : IEntityPresetPassive,
    ITameable,
    IBehaviorFollowOwner,
    IBehaviorOwnerHurtByTarget,
    IBehaviorOwnerHurtTarget;

/// <summary>
/// Preset entity interface that implements typical behaviours and properties of an aquatic mob
/// </summary>
public interface IEntityPresetAquatic : IBasicEntity,
    IMovement,
    INavigationSwim,
    IMovementGeneric,
    IWaterMovement,
    IBehaviorRandomLookAround;

/// <summary>
/// Preset entity interface that implements typical behaviours and properties of an amphibious mob that moves between land and water
/// </summary>
public interface IEntityPresetAmphibious : IEntityPresetAquatic,
    IMovementAmphibious,
    IBehaviorFloat,
    IBehaviorRandomStroll,
    IBehaviorMoveToWater,
    IBehaviorMoveToLand;

/// <summary>
/// Preset entity interface that implements typical behaviours and properties of a flying mob
/// </summary>
public interface IEntityPresetFlying : IBasicEntity,
    IMovement,
    INavigationFly,
    IMovementFly,
    IBehaviorFloat,
    IBehaviorFloatWander,
    IBehaviorRandomLookAround;

/// <summary>
/// Preset entity interface that implements typical behaviours of a hostile flying mob
/// </summary>
public interface IEntityPresetFlyingHostile : IEntityPresetFlying,
    IAttack,
    IFollowRange,
    IBehaviorHurtByTarget,
    IBehaviorNearestAttackableTarget,
    IBehaviorMeleeAttack,
    IBehaviorMoveTowardsTarget,
    IBehaviorLookAtTarget;

/// <summary>
/// Preset entity interface that implements typical behaviours of a hostile aquatic mob
/// </summary>
public interface IEntityPresetAquaticHostile : IEntityPresetAquatic,
    IAttack,
    IFollowRange,
    IBehaviorHurtByTarget,
    IBehaviorNearestAttackableTarget,
    IBehaviorMeleeAttack,
    IBehaviorMoveTowardsTarget,
    IBehaviorLookAtTarget;

/// <summary>
/// Preset entity interface that implements behaviour that emulates a block's collision
/// </summary>
public interface IEntityPresetSolid :
    IIsCollidable,
    ICollisionBox,
    IBodyRotationBlocked,
    IRotationAxisAligned,
    IRendersWhenInvisible,
    ISpellEffects,
    IIsStackable,
    IPushThrough
{
    EntitySpellEffect[] ISpellEffects.AddEffects =>
    [
        new() { InfiniteDuration = true, Effect = "invisibility", Duration = 0, }
    ];

    float IPushThrough.Value => 1;
}