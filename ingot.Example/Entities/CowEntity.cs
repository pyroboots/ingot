using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Entity;

using Version = ingot.Core.Common.Version;

namespace ingot.Example.Entities;

/// <summary>
/// C# port of vanilla cow behaviour as <c>test:custom_cow</c>
/// </summary>
public class CowEntity :
    Entity,
    IOffspring,
    ISpawnEggInteraction,
    IIsHiddenWhenInvisible,
    ITypeFamily,
    IBreathable,
    INavigationWalk,
    IMovementBasic,
    IJumpStatic,
    ICanClimb,
    ICollisionBox,
    INameable,
    IHealth,
    IHurtOnCondition,
    IMovement,
    IDespawn,
    IBehaviorFloat,
    IBehaviorPanic,
    IBehaviorMountPathing,
    IBehaviorBreed,
    IBehaviorTempt,
    IBehaviorFollowParent,
    IBehaviorRandomStroll,
    IBehaviorLookAtPlayer,
    IBehaviorRandomLookAround,
    ILeashable,
    IBalloonable,
    IPhysics,
    IPushableByEntity,
    IPushableByBlock,
    IConditionalBandwidthOptimization
{
    public override Identifier Identifier => new("test", "custom_cow");
    public override Version FormatVersion => new("1.26.10");
    public override bool IsSpawnable => true;
    public override bool IsSummonable => true;

    public override EntityComponentGroup[] ComponentGroups =>
    [
        new CowBabyComponentGroup(),
        new CowAdultComponentGroup(),
    ];

    // --- components ---

    dynamic IOffspring.DenyParentsVariant => null!;
    dynamic IOffspring.MutationFactor => null!;
    dynamic IOffspring.OffspringPairs => new Dictionary<string, string>
    {
        ["test:custom_cow"] = "test:custom_cow",
    };
    string[] IOffspring.ParentCentricAttributeBlending => null!;
    dynamic IOffspring.PropertyInheritance => null!;

    dynamic ITypeFamily.Family => new[] { "cow", "mob" };

    string[] IBreathable.BreatheBlocks => null!;
    string[] IBreathable.NonBreatheBlocks => null!;
    int IBreathable.SuffocateTime => 0;
    int IBreathable.TotalSupply => 15;

    bool INavigationWalk.CanPathOverWater => true;
    bool INavigationWalk.AvoidWater => true;
    bool INavigationWalk.AvoidDamageBlocks => true;
    string[] INavigationWalk.BlocksToAvoid => null!;
    bool INavigationWalk.CanFloat => true;
    string INavigationWalk.UsingDoorAnnotation => null!;

    float ICollisionBox.Width => 0.9f;
    float ICollisionBox.Height => 1.3f;

    string INameable.DefaultTrigger => null!;
    dynamic INameable.NameActions => null!;

    int IHealth.Max => 10;
    int IHealth.Value => 10;

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

    float IMovement.Value => 0.25f;
    float IMovement.Max => 0.25f;

    dynamic IDespawn.DespawnFromDistance => new Dictionary<string, object>();
    EntityFilter IDespawn.Filters => null!;

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
    string[] IBehaviorTempt.Items => ["wheat"];
    int IBehaviorTempt.SoundInterval => 0;
    string IBehaviorTempt.TemptSound => null!;

    int IBehaviorFollowParent.Priority => 5;
    float IBehaviorFollowParent.SpeedMultiplier => 1.1f;

    int IBehaviorRandomStroll.Priority => 6;
    float IBehaviorRandomStroll.SpeedMultiplier => 0.8f;

    int IBehaviorLookAtPlayer.Priority => 7;
    float IBehaviorLookAtPlayer.LookDistance => 6f;
    float IBehaviorLookAtPlayer.Probability => 0.02f;

    int IBehaviorRandomLookAround.Priority => 9;

    string ILeashable.OnLeash => null!;
    string ILeashable.OnUnleash => null!;
    dynamic ILeashable.Presets => null!;

    EntityEventTrigger IBalloonable.OnBalloon => null!;
    EntityEventTrigger IBalloonable.OnUnballoon => null!;

    dynamic IConditionalBandwidthOptimization.ConditionalValues => null!;
    dynamic IConditionalBandwidthOptimization.DefaultValues => null!;

    // --- events (entity properties / set_property omitted) ---

    public override Dictionary<Identifier, IEntityEventAction[]> Events => new()
    {
        [Identifier.Vanilla("entity_spawned")] =
        [
            new SequenceEntityEventAction
            {
                EventActions =
                [
                    new RandomizeEntityEventAction
                    {
                        EventActions =
                        [
                            new RandomizeEntityEventAction.EventActionPool(95f,
                            [
                                new TriggerEntityEventAction { Event = "test:spawn_adult" },
                            ]),
                            new RandomizeEntityEventAction.EventActionPool(5f,
                            [
                                new ComponentGroupAddEntityEventAction
                                {
                                    ComponentGroups = [new Identifier("test", "custom_cow_baby")],
                                },
                            ]),
                        ],
                    },
                ],
            },
        ],

        [Identifier.Vanilla("entity_born")] =
        [
            new ComponentGroupAddEntityEventAction
            {
                ComponentGroups = [new Identifier("test", "custom_cow_baby")],
            },
        ],

        [Identifier.Vanilla("entity_transformed")] =
        [
            new ComponentGroupRemoveEntityEventAction(),
            new ComponentGroupAddEntityEventAction
            {
                ComponentGroups = [new Identifier("test", "custom_cow_adult")],
            },
        ],

        [Identifier.Vanilla("ageable_grow_up")] =
        [
            new ComponentGroupRemoveEntityEventAction
            {
                ComponentGroups = [new Identifier("test", "custom_cow_baby")],
            },
            new ComponentGroupAddEntityEventAction
            {
                ComponentGroups = [new Identifier("test", "custom_cow_adult")],
            },
        ],

        [new Identifier("test", "spawn_adult")] =
        [
            new ComponentGroupAddEntityEventAction
            {
                ComponentGroups = [new Identifier("test", "custom_cow_adult")],
            },
        ],
    };
}

/// <summary>
/// <c>test:custom_cow_baby</c> component group.
/// </summary>
public class CowBabyComponentGroup :
    EntityComponentGroup,
    IIsBaby,
    IScale,
    IAgeable,
    IRideable,
    IBehaviorFollowParent
{
    public override Identifier Identifier => new("test", "custom_cow_baby");
    public override Entity Parent => new CowEntity();

    float IScale.Value => 0.5f;

    float IAgeable.Duration => 1200f;
    dynamic IAgeable.FeedItems => "wheat";
    string[] IAgeable.DropItems => null!;
    dynamic IAgeable.GrowUp => new Dictionary<string, string>
    {
        ["event"] = "minecraft:ageable_grow_up",
        ["target"] = "self",
    };
    EntityFilter IAgeable.InteractFilters => null!;
    string IAgeable.PauseGrowth => null!;
    string[] IAgeable.PauseGrowthItems => ["golden_dandelion"];
    string IAgeable.ResetGrowth => null!;
    string[] IAgeable.ResetGrowthItems => ["golden_dandelion"];

    int IRideable.SeatCount => 1;
    string[] IRideable.FamilyTypes => ["baby_undead"];
    string IRideable.InteractText => null!;
    Identifier IRideable.OnRiderEnterEvent => null!;
    Identifier IRideable.OnRiderExitEvent => null!;
    dynamic IRideable.Seats => new Dictionary<string, object>
    {
        ["position"] = new[] { 0.0, 1.0, 0.0 },
    };

    int IBehaviorFollowParent.Priority => 6;
    float IBehaviorFollowParent.SpeedMultiplier => 1.1f;
}

/// <summary>
/// <c>test:custom_cow_adult</c> component group.
/// </summary>
public class CowAdultComponentGroup :
    EntityComponentGroup,
    ILeashableTo,
    IExperienceReward,
    ILoot,
    IRideable,
    IBehaviorBreed,
    IBreedable,
    IInteract
{
    public override Identifier Identifier => new("test", "custom_cow_adult");
    public override Entity Parent => new CowEntity();

    dynamic IExperienceReward.OnBred => "Math.Random(1,7)";
    dynamic IExperienceReward.OnDeath => "query.last_hit_by_player ? Math.Random(1,3) : 0";

    string ILoot.Table => "loot_tables/entities/cow.json";

    int IRideable.SeatCount => 1;
    string[] IRideable.FamilyTypes => ["baby_undead"];
    string IRideable.InteractText => null!;
    Identifier IRideable.OnRiderEnterEvent => null!;
    Identifier IRideable.OnRiderExitEvent => null!;
    dynamic IRideable.Seats => new Dictionary<string, object>
    {
        ["position"] = new[] { 0.0, 1.15, 0.0 },
    };

    int IBehaviorBreed.Priority => 3;
    float IBehaviorBreed.SpeedMultiplier => 1f;

    bool IBreedable.RequireTame => false;
    string[] IBreedable.BreedItems => ["wheat"];
    dynamic IBreedable.BreedsWith => new Dictionary<string, object>
    {
        ["test:custom_cow"] = new Dictionary<string, object>(),
    };
    dynamic IBreedable.DenyParentsVariant => null!;
    dynamic IBreedable.EnvironmentRequirements => null!;
    EntityFilter IBreedable.LoveFilters => null!;
    dynamic IBreedable.MutationFactor => null!;

    string IInteract.DropItemSlot => null!;
    string IInteract.EquipItemSlot => null!;
    string IInteract.InteractText => null!;
    dynamic IInteract.Interactions => new object[]
    {
        new Dictionary<string, object?>
        {
            ["on_interact"] = new Dictionary<string, object?>
            {
                ["filters"] = new Dictionary<string, object?>
                {
                    ["all_of"] = new object[]
                    {
                        new Dictionary<string, object?>
                        {
                            ["test"] = "is_family",
                            ["subject"] = "other",
                            ["value"] = "player",
                        },
                        new Dictionary<string, object?>
                        {
                            ["test"] = "has_equipment",
                            ["domain"] = "hand",
                            ["subject"] = "other",
                            ["value"] = "bucket:0",
                        },
                    },
                },
            },
            ["use_item"] = true,
            ["swing"] = true,
            ["transform_to_item"] = "bucket:1",
            ["play_sounds"] = "milk",
            ["interact_text"] = "action.interact.milk",
        },
    };
}

/// <summary>
/// Resource-pack client entity for <c>test:custom_cow</c>.
/// </summary>
public class CowClientEntity : ClientEntity<CowEntity>
{
    public override Version FormatVersion => new("1.10.0");
    public override string? MinEngineVersion => "1.8.0";

    public override string DefaultMaterial => "cow";
    public override string DefaultTexture => "textures/entity/cow/cow_v2";
    public override string DefaultGeometry => "geometry.cow.v2";

    [ClientEntityMaterial("cold")]
    public string ColdMaterial => "cow_cold";

    [ClientEntityTexture("warm")]
    public string WarmTexture => "textures/entity/cow/cow_warm";

    [ClientEntityTexture("cold")]
    public string ColdTexture => "textures/entity/cow/cow_cold";

    [ClientEntityTexture("baby_default")]
    public string BabyDefaultTexture => "textures/entity/cow/cow_temperate_baby";

    [ClientEntityTexture("baby_cold")]
    public string BabyColdTexture => "textures/entity/cow/cow_cold_baby";

    [ClientEntityTexture("baby_warm")]
    public string BabyWarmTexture => "textures/entity/cow/cow_warm_baby";

    [ClientEntityGeometry("warm")]
    public string WarmGeometry => "geometry.cow.warm";

    [ClientEntityGeometry("cold")]
    public string ColdGeometry => "geometry.cow.cold";

    // Intentionally omit geometry.cow.baby: combining baby + adult cow geos triggers
    // armor_offset.default_neck locator conflicts when both are loaded on one client entity.
    // Babies use the adult geo scaled via the behaviour scale component + baby textures.

    public override string[] RenderControllers => ["controller.render.cow.v3"];
    public override bool EmitDefaultRenderController => false;

    public override Dictionary<string, string>? Animations => new()
    {
        ["setup"] = "animation.cow.setup",
        ["walk"] = "animation.quadruped.walk",
        ["look_at_target"] = "animation.common.look_at_target",
        ["baby_transform"] = "animation.cow.baby_transform",
    };

    public override ClientEntityScripts? Scripts => new()
    {
        PreAnimation =
        [
            "t.variant = query.property('minecraft:climate_variant');",
            "v.index = (t.variant == 'temperate') ? 0 : ((t.variant == 'warm') ? 1 : 2);",
            "v.is_cold = t.variant == 'cold';",
        ],
        Animate =
        [
            "setup",
            new Dictionary<string, string> { ["walk"] = "query.modified_move_speed" },
            "look_at_target",
            new Dictionary<string, string> { ["baby_transform"] = "query.is_baby" },
        ],
    };

    public override ClientEntitySpawnEgg? SpawnEgg => new()
    {
        Texture = "spawn_egg_cow",
    };

    /// <summary>
    /// Reuses vanilla cow sound definitions so <c>test:custom_cow</c> is not silent.
    /// </summary>
    public override ClientEntitySounds? EntitySounds => new()
    {
        Volume = 1f,
        Pitch = [0.8f, 1.2f],
        Events = new Dictionary<string, object>
        {
            ["ambient"] = "mob.cow.say",
            ["hurt"] = "mob.cow.hurt",
            ["death"] = "mob.cow.death",
            ["step"] = "mob.cow.step",
            ["milk"] = "mob.cow.milk",
        },
    };
}

/// <summary>
/// Vanilla <c>controller.render.cow.v3</c> used by the cow client entity.
/// </summary>
public class CowV3RenderController : RenderController
{
    public override string ControllerId => "controller.render.cow.v3";
    public override string FileName => "cow.v3";
    public override Version FormatVersion => new("1.8.0");

    public override string Geometry => "Array.geos[v.index]";

    public override IReadOnlyList<IReadOnlyDictionary<string, string>> Materials { get; } =
    [
        new Dictionary<string, string>
        {
            ["*"] = "v.is_cold ? Material.cold : Material.default",
        },
    ];

    public override string[] Textures { get; } =
    [
        "query.is_baby ? Array.baby_textures[v.index] : Array.textures[v.index]",
    ];

    public override Dictionary<string, string[]>? TextureArrays => new()
    {
        ["Array.textures"] =
        [
            "Texture.default",
            "Texture.warm",
            "Texture.cold",
        ],
        ["Array.baby_textures"] =
        [
            "Texture.baby_default",
            "Texture.baby_warm",
            "Texture.baby_cold",
        ],
    };

    public override Dictionary<string, string[]>? GeometryArrays => new()
    {
        ["Array.geos"] =
        [
            "Geometry.default",
            "Geometry.warm",
            "Geometry.cold",
        ],
    };
}
