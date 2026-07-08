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

/// <summary>
/// Passive land mob with common “livestock” components and navigation defaults
/// (climb, nameable, leash, balloon, breathable, lava hurt, look-at, tempt, mount pathing, breed goal).
/// Implementers still set family, health, movement, and temptation items.
/// </summary>
public interface IEntityPresetPassiveLand : IEntityPresetPassive,
    ICanClimb,
    INameable,
    ILeashable,
    IBalloonable,
    IBreathable,
    IHurtOnCondition,
    IConditionalBandwidthOptimization,
    IBehaviorPanic,
    IBehaviorLookAtPlayer,
    IBehaviorTempt,
    IBehaviorMountPathing,
    IBehaviorBreed,
    IBehaviorFollowParent,
    IPushableByEntity,
    IPushableByBlock,
    IIsHiddenWhenInvisible,
    ISpawnEggInteraction
{
    bool INavigationWalk.CanPathOverWater => true;
    bool INavigationWalk.AvoidWater => true;
    bool INavigationWalk.AvoidDamageBlocks => true;
    bool INavigationWalk.CanFloat => true;

    int IBreathable.TotalSupply => 15;
    int IBreathable.SuffocateTime => 0;

    int IBehaviorFloat.Priority => 0;

    int IBehaviorPanic.Priority => 1;
    float IBehaviorPanic.SpeedMultiplier => 1.25f;

    int IBehaviorMountPathing.Priority => 2;
    float IBehaviorMountPathing.SpeedMultiplier => 1.5f;
    float IBehaviorMountPathing.TargetDist => 0f;
    bool IBehaviorMountPathing.TrackTarget => true;

    int IBehaviorBreed.Priority => 3;
    float IBehaviorBreed.SpeedMultiplier => 1f;

    int IBehaviorTempt.Priority => 4;
    float IBehaviorTempt.SpeedMultiplier => 1.25f;
    int IBehaviorTempt.SoundInterval => 0;

    int IBehaviorFollowParent.Priority => 5;
    float IBehaviorFollowParent.SpeedMultiplier => 1.1f;

    int IBehaviorRandomStroll.Priority => 6;
    float IBehaviorRandomStroll.SpeedMultiplier => 0.8f;

    int IBehaviorLookAtPlayer.Priority => 7;
    float IBehaviorLookAtPlayer.LookDistance => 6f;
    float IBehaviorLookAtPlayer.Probability => 0.02f;

    int IBehaviorRandomLookAround.Priority => 9;

    dynamic IHurtOnCondition.DamageConditions => new object[]
    {
        new Dictionary<string, object?>
        {
            ["filters"] = new Dictionary<string, object?>
            {
                ["test"] = "in_lava",
                ["subject"] = "self",
                ["operator"] = "==",
                ["value"] = true,
            },
            ["cause"] = "lava",
            ["damage_per_tick"] = 4,
        },
    };

    dynamic IDespawn.DespawnFromDistance => new Dictionary<string, object>();
}

/// <summary>
/// Adds offspring pairing defaults for a breedable species that produces the same entity type.
/// Override <see cref="IOffspring.OffspringPairs"/> when offspring differ.
/// </summary>
public interface IEntityPresetSameSpeciesOffspring : IOffspring
{
    /// <summary>
    /// Species id used as both parent and offspring in <c>offspring_pairs</c>
    /// (typically the same as the entity identifier string).
    /// </summary>
    string SpeciesId { get; }

    dynamic IOffspring.OffspringPairs => new Dictionary<string, string>
    {
        [SpeciesId] = SpeciesId,
    };
}