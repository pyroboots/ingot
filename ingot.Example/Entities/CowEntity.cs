using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;
using ingot.Core.Resource;
using ingot.Core.TraitSystem.Traits.Entity;

using Version = ingot.Core.Common.Version;

namespace ingot.Example.Entities;

/// <summary>
/// Vanilla-style cow as <c>test:custom_cow</c> - behaviour, groups, events, client visuals, and sounds.
/// </summary>
public class CowEntity : Entity, IEntityPresetPassiveLand, IEntityPresetSameSpeciesOffspring
{
    public override Identifier Identifier => new("test", "custom_cow");
    public override Version FormatVersion => new("1.26.10");
    public override bool IsSpawnable => true;
    public override bool IsSummonable => true;

    public override Type? ClientEntityType => typeof(Client);

    string IEntityPresetSameSpeciesOffspring.SpeciesId => Identifier.ToString();

    dynamic ITypeFamily.Family => new[] { "cow", "mob" };
    int IHealth.Max => 10;
    float IMovement.Value => 0.25f;
    string[] IBehaviorTempt.Items => ["wheat"];

    float ICollisionBox.Width => 0.9f;
    float ICollisionBox.Height => 1.3f;

    public override EntityComponentGroup[] ComponentGroups => [new Baby(), new Adult()];

    public override Dictionary<Identifier, IEntityEventAction[]> Events => EntityEvents.Map(
        (Identifier.Vanilla("entity_spawned"),
            EntityEvents.SpawnedAdultOrBaby(95f, 5f, "test:spawn_adult", Baby.Id)),
        (Identifier.Vanilla("entity_born"), [EntityEvents.Add(Baby.Id)]),
        (Identifier.Vanilla("entity_transformed"),
            [EntityEvents.Remove(), EntityEvents.Add(Adult.Id)]),
        (Identifier.Vanilla("ageable_grow_up"), EntityEvents.GrowUp(Baby.Id, Adult.Id)),
        (new Identifier("test", "spawn_adult"), [EntityEvents.Add(Adult.Id)])
    );

    /// <summary>Baby component group.</summary>
    public class Baby : EntityComponentGroup<CowEntity>, IIsBaby, IScale, IAgeable, IRideable, IBehaviorFollowParent
    {
        public static Identifier Id { get; } = new("test", "custom_cow_baby");
        public override Identifier Identifier => Id;

        float IScale.Value => 0.5f;
        float IAgeable.Duration => 1200f;
        dynamic IAgeable.FeedItems => "wheat";
        dynamic IAgeable.GrowUp => EntityEventTargets.GrowUpSelf("minecraft:ageable_grow_up");
        string[] IAgeable.PauseGrowthItems => ["golden_dandelion"];
        string[] IAgeable.ResetGrowthItems => ["golden_dandelion"];

        int IRideable.SeatCount => 1;
        string[] IRideable.FamilyTypes => ["baby_undead"];
        dynamic IRideable.Seats => new Dictionary<string, object>
        {
            ["position"] = new[] { 0.0, 1.0, 0.0 },
        };

        int IBehaviorFollowParent.Priority => 6;
        float IBehaviorFollowParent.SpeedMultiplier => 1.1f;
    }

    /// <summary>Adult component group.</summary>
    public class Adult : EntityComponentGroup<CowEntity>,
        ILeashableTo, IExperienceReward, ILoot, IRideable, IBehaviorBreed, IBreedable, IInteract
    {
        public static Identifier Id { get; } = new("test", "custom_cow_adult");
        public override Identifier Identifier => Id;

        dynamic IExperienceReward.OnBred => "Math.Random(1,7)";
        dynamic IExperienceReward.OnDeath => "query.last_hit_by_player ? Math.Random(1,3) : 0";
        string ILoot.Table => "loot_tables/entities/cow.json";

        int IRideable.SeatCount => 1;
        string[] IRideable.FamilyTypes => ["baby_undead"];
        dynamic IRideable.Seats => new Dictionary<string, object>
        {
            ["position"] = new[] { 0.0, 1.15, 0.0 },
        };

        int IBehaviorBreed.Priority => 3;
        bool IBreedable.RequireTame => false;
        string[] IBreedable.BreedItems => ["wheat"];
        dynamic IBreedable.BreedsWith => new Dictionary<string, object>
        {
            ["test:custom_cow"] = new Dictionary<string, object>(),
        };

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

    /// <summary>Resource-pack client entity (auto-discovered by <c>AddEntity&lt;CowEntity&gt;()</c>).</summary>
    public class Client : ClientEntity<CowEntity>
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

        public override string[] RenderControllers => [new RenderControllerReference<CowV3RenderController>()];
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

        public override ClientEntitySounds? EntitySounds =>
            ClientEntitySounds.FromVanilla("cow", includeMilk: true);
    }
}

/// <summary>
/// Vanilla <c>controller.render.cow.v3</c> (register with <c>AddRenderController</c> or nest under entity later).
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
        ["Array.textures"] = ["Texture.default", "Texture.warm", "Texture.cold"],
        ["Array.baby_textures"] = ["Texture.baby_default", "Texture.baby_warm", "Texture.baby_cold"],
    };

    public override Dictionary<string, string[]>? GeometryArrays => new()
    {
        ["Array.geos"] = ["Geometry.default", "Geometry.warm", "Geometry.cold"],
    };
}
