# Making an Entity

Entities are defined by deriving from the abstract `Entity` class in `ingot.Core.Behaviour.Entity`. Like blocks and items, entities use the [trait system](trait-system.md) for component definitions. ingot also provides C# types for [component groups](entity-component-groups.md) and [events](entity-events.md).

## Minimal Entity

```csharp
using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

public class MyEntity : Entity
{
    public override Identifier Identifier => new("mynamespace:my_entity");
}
```

Every entity **must** implement:

- `Identifier` - the full `namespace:name` string used in Minecraft.

## Key Properties

| Property           | Type                                      | Default     | Description |
|--------------------|-------------------------------------------|-------------|-------------|
| `FormatVersion`    | `Version`                                 | `"1.20.10"` | Target format version. |
| `IsSpawnable`      | `bool`                                    | `false`     | Whether the entity can spawn naturally in the world. |
| `IsSummonable`     | `bool`                                    | `true`      | Whether the entity can be summoned with commands. |
| `IsExperimental`   | `bool`                                    | `false`     | Whether the entity requires experimental gameplay. |
| `RuntimeIdentifier`| `Identifier?`                             | `null`      | Optional vanilla id to imitate hard-coded engine behavior. |
| `Properties`       | `Dictionary<Identifier, IEntityProperty>` | `{}`        | Typed entity properties (server-side state, optional client sync). See [Entity Properties](#entity-properties). |
| `ComponentGroups`  | `EntityComponentGroup[]`                  | `[]`        | Named component sets toggled by events. See [Entity Component Groups](entity-component-groups.md). |
| `Events`           | `Dictionary<Identifier, IEntityEventAction[]>` | `{}`     | Event definitions and their actions. See [Entity Events](entity-events.md). |
| `DynamicTraits`    | `Trait[]`                                 | `[]`        | Hand-built `Trait` components for identifiers without a generated trait interface. See [Dynamic Traits](trait-system.md#dynamic-traits). |

These are written into the `description` (including `properties`), `component_groups`, `components`, and `events` sections of the generated entity JSON.

## Compiled Output

A minimal entity compiles to:

```json
{
    "format_version": "1.20.10",
    "minecraft:entity": {
        "description": {
            "identifier": "mynamespace:my_entity",
            "properties": {},
            "is_spawnable": false,
            "is_summonable": true,
            "is_experimental": false
        },
        "component_groups": {},
        "components": {},
        "events": {}
    }
}
```

Override `IsSpawnable`, `IsSummonable`, or `IsExperimental` on your entity class when you need different values.

## Entity Properties

Entity properties store typed state on an entity without a full component, similar to block states. They compile into `minecraft:entity` → `description` → `properties`.

Override `Properties` with a dictionary of `Identifier` → `IEntityProperty`. Four implementations are available:

| Class | JSON `type` | Fields | Notes |
|-------|-------------|--------|--------|
| `BooleanEntityProperty` | `bool` | `Default` | `true` / `false` |
| `EnumEntityProperty` | `enum` | `Values`, `Default` | `Default` must be one of `Values` or compile throws |
| `FloatEntityProperty` | `float` | `Min`, `Max`, `Default` | Emits `"range": [min, max]`; default must be in range |
| `IntEntityProperty` | `int` | `Min`, `Max`, `Default` | Same range rules as float |

Every property also has `ClientSync` (default `true`), which controls whether clients can read the value.

```csharp
using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

public class MyEntity : Entity
{
    public override Identifier Identifier => new("mynamespace:my_entity");

    public override Dictionary<Identifier, IEntityProperty> Properties => new()
    {
        [new("mynamespace:is_charged")] = new BooleanEntityProperty
        {
            Default = false,
        },
        [new("mynamespace:mood")] = new EnumEntityProperty
        {
            Values = ["calm", "alert", "angry"],
            Default = "calm",
            ClientSync = true,
        },
        [new("mynamespace:power")] = new FloatEntityProperty
        {
            Min = 0f,
            Max = 1f,
            Default = 0.5f,
        },
        [new("mynamespace:level")] = new IntEntityProperty
        {
            Min = 0,
            Max = 10,
            Default = 1,
            ClientSync = false,
        },
    };
}
```

Compiles to (property block only):

```json
"properties": {
    "mynamespace:is_charged": {
        "type": "bool",
        "default": false,
        "client_sync": true
    },
    "mynamespace:mood": {
        "type": "enum",
        "values": ["calm", "alert", "angry"],
        "default": "calm",
        "client_sync": true
    },
    "mynamespace:power": {
        "type": "float",
        "range": [0.0, 1.0],
        "default": 0.5,
        "client_sync": true
    },
    "mynamespace:level": {
        "type": "int",
        "range": [0, 10],
        "default": 1,
        "client_sync": false
    }
}
```

> [!CAUTION]
> Validation runs at compile time and **throws** on invalid property definitions:
>
> - `EnumEntityProperty`: `Default` not listed in `Values` → `InvalidEnumArgumentException`
> - `FloatEntityProperty` / `IntEntityProperty`: `Default` outside `[Min, Max]` → `ArgumentOutOfRangeException`

## Component Groups

Component groups let you attach different sets of traits to an entity at runtime. See the dedicated [Entity Component Groups](entity-component-groups.md) guide.

## Entity Events

Events add or remove component groups, run commands, emit particles, and more. See the dedicated [Entity Events](entity-events.md) guide.

## Adding Behavior with Traits

Entity traits use the same pattern as blocks and items. Implement `IEntityTrait` interfaces on your entity class or on `EntityComponentGroup` subclasses:

```csharp
using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Entity;

public class MyEntity : Entity, IHealth, ITypeFamily
{
    public override Identifier Identifier => new("mynamespace:my_entity");

    int IHealth.Max => 20;
    dynamic ITypeFamily.Family => "mob";
}
```

### Behaviour Presets

For common mob archetypes, ingot provides preset interfaces that bundle the typical components for a mob type. See the [Trait System - Entity Traits and Behaviour Presets](trait-system.md#entity-traits-and-behaviour-presets) section for the full list.

```csharp
public class LasagnaSpiritEntity : Entity, IEntityPresetFlying
{
    public override Identifier Identifier => new("test:lasagna_spirit");

    dynamic ITypeFamily.Family => "lasagna";
    int IHealth.Max => 20;
    dynamic IDespawn.DespawnFromDistance => null;
    EntityFilter IDespawn.Filters => null;

    float IMovement.Max => 6;
    float IMovement.Value => 3;
    string[] INavigationFly.BlocksToAvoid => [];
    float IBehaviorFloatWander.FloatDuration => 6f;
}
```

Presets compose many individual traits. You can still add extra traits beyond what a preset provides, or implement traits one at a time without using a preset.

See the [Entity Traits API reference](https://pyroboots.github.io/ingot/api/ingot.Core.TraitSystem.Traits.Entity.html) for the complete list.

## Raw JSON Entities

If you already have hand-authored entity JSON, derive from `JsonEntity` instead of `Entity`:

```csharp
using ingot.Core.Behaviour;
using ingot.Core.Common;

public class HandAuthoredEntity : JsonEntity
{
    public override Identifier Identifier => new("mynamespace:custom_entity");
    protected override string Json => LoadJson("path/to/entity.json");
}
```

`JsonEntity` bypasses trait and event compilation and writes your JSON verbatim.

## Compilation & Registration

Register entities with `Pack.Create`:

```csharp
using ingot.Core;

const string packUuid = "77f1fef2-bb39-411a-b25c-ae475c21169f";

Pack pack = Pack.Create(packUuid, "My Addon", "Entities made with ingot")
    .AddEntity<MyEntity>(); // also discovers ClientEntity&lt;MyEntity&gt; / nested Client by default

pack.Compile("./output");
```

To register a pre-configured instance (including `JsonEntity`), use `pack.BehaviourPack.AddEntityFromInstance(inst)`. Direct compile helpers are `Entity.Compile(Type)`, `Entity.Compile<T>()`, and `Entity.CompileFromInstance(inst)` (JsonEntity instances are written as raw JSON).

`AddEntity<T>(discoverClient: true)` (default) looks for a matching client entity:

1. Nested type `MyEntity.Client : ClientEntity<MyEntity>`
2. Or a type named `MyClientEntity` in the same assembly
3. Or `Entity.ClientEntityType` when set explicitly

Use `AddEntity<T>(discoverClient: false)` to skip RP discovery. Nested `RenderController` types on the entity are registered when found; top-level controllers still use `AddRenderController<T>()`.

> [!TIP]
> Prefer a nested `Client` type on the entity (`MyEntity.Client : ClientEntity<MyEntity>`) so `AddEntity` discovers visuals without a separate `AddClientEntity` call.

This writes `bp/entities/my_entity.json` (filename is the part after the `:` in the identifier).

## Write less: presets, events, groups

### Behaviour presets

Prefer presets over listing dozens of traits. `IEntityPresetPassiveLand` bundles walk navigation, panic/tempt/breed goals, leash/balloon/nameable, lava hurt, and sensible AI priorities. Override only what differs:

```csharp
public class CowEntity : Entity, IEntityPresetPassiveLand, IEntityPresetSameSpeciesOffspring
{
    public override Identifier Identifier => new("test", "custom_cow");
    string IEntityPresetSameSpeciesOffspring.SpeciesId => Identifier.ToString();
    dynamic ITypeFamily.Family => new[] { "cow", "mob" };
    int IHealth.Max => 10;
    float IMovement.Value => 0.25f;
    string[] IBehaviorTempt.Items => ["wheat"];
}
```

> [!NOTE]
> Optional trait properties default to `null` and are omitted from JSON - you no longer need `=> null!` stubs for unused fields.

### Event helpers

Use `EntityEvents` instead of hand-building nested actions:

```csharp
public override Dictionary<Identifier, IEntityEventAction[]> Events => EntityEvents.Map(
    (Identifier.Vanilla("entity_spawned"),
        EntityEvents.SpawnedAdultOrBaby(95f, 5f, "test:spawn_adult", Baby.Id)),
    (Identifier.Vanilla("ageable_grow_up"), EntityEvents.GrowUp(Baby.Id, Adult.Id)),
    (new Identifier("test", "spawn_adult"), [EntityEvents.Add(Adult.Id)])
);
```

### Component groups

Prefer `EntityComponentGroup<TParent>` so `Parent` is inferred. Nested types under the entity keep groups co-located:

```csharp
public class Baby : EntityComponentGroup<CowEntity>, IIsBaby, IScale, IAgeable
{
    public static Identifier Id { get; } = new("test", "custom_cow_baby");
    public override Identifier Identifier => Id;
    float IScale.Value => 0.5f;
    // ...
}
```

## Client Entities & Render Controllers

Behaviour entities only define gameplay. **Visuals** live in the resource pack via a separate `ClientEntity` (or `ClientEntity<TParent>`) class, which compiles to `minecraft:client_entity`. Render controllers decide how those short-names are drawn.

For texture and pack layout details, see [Resource Packs & Textures](resource-packs.md#entity-textures-and-client-entities).

### Minimal client entity

`ClientEntity<TParent>` takes its `Identifier` from the behaviour entity. Base members already map the three required short-names:

| Property | Attribute | Short-name | Default |
|----------|-----------|------------|---------|
| `DefaultMaterial` | `[ClientEntityMaterial("default")]` | `default` | `"entity"` |
| `DefaultTexture` | `[ClientEntityTexture("default")]` | `default` | *(required override)* |
| `DefaultGeometry` | `[ClientEntityGeometry("default")]` | `default` | `"geometry.{entity_name}"` |

```csharp
using ingot.Core.Behaviour.Entity;

public class MyClientEntity : ClientEntity<MyEntity>
{
    public override string DefaultMaterial => "entity_alphatest";
    public override string DefaultTexture => "textures/entity/my_entity";
    // DefaultGeometry defaults to geometry.my_entity

    // Optional: copy this PNG into rp/textures/entity/my_entity.png during pack compile
    public override string? DefaultTexturePath => "Data/my_entity.png";

    public override ClientEntitySpawnEgg? SpawnEgg => new()
    {
        BaseColor = "#db7500",
        OverlayColor = "#242222",
    };
}
```

Register both sides with the pack:

```csharp
pack.AddEntity<MyEntity>()
    .AddClientEntity<MyClientEntity>();
```

This writes:

- `rp/entity/my_entity.json` - client entity short-names and description
- `rp/render_controllers/my_entity.json` - auto-emitted simple controller (`controller.render.my_entity`) that always uses `Geometry.default`, `Material.default`, and `Texture.default`
- `rp/textures/entity/my_entity.png` - only when `DefaultTexturePath` is set (or you call `AddEntityTexture`)

### Entity textures (PNG files)

Client entity JSON uses **paths** (for example `textures/entity/my_entity`), not atlas keys like items/blocks.

To copy a source PNG into the resource pack:

1. **Auto (default texture only):** set `DefaultTexturePath` to a file on disk. During `Pack.Compile`, ingot strips a leading `textures/entity/` from `DefaultTexture` and registers `rp/textures/entity/<relative>.png`.
2. **Manual:** `pack.AddEntityTexture("my_entity", "Data/my_entity.png")` (or `"subdir/my_entity"` for nested paths).

Extra texture short-names (angry, alt, …) are written into JSON only. Register their PNGs with `AddEntityTexture` if you need the files in the pack.

> [!NOTE]
> Calling `ClientEntity.Compile(type)` alone does **not** copy texture files. PNG registration runs only under full pack compile when `CompilerState.CurrentPack` is set.

### Custom short-names (attributes)

Tag additional properties or fields so they appear under `materials`, `textures`, or `geometry`:

```csharp
[ClientEntityMaterial("invisible")]
public string InvisibleMaterial => "spider_invisible";

[ClientEntityTexture("angry")]
public string AngryTexture => "textures/entity/my_entity_angry";

[ClientEntityGeometry("charged")]
public string ChargedGeometry => "geometry.my_entity.charged";
```

- Pass an id in the attribute for the short-name (`"invisible"`).
- If the id is omitted, the short-name is derived from the member name (with a `Default…` → `default` special case).
- Override `DefaultMaterial` / `DefaultTexture` / `DefaultGeometry` to change the built-in `default` short-names.

> [!CAUTION]
> Do not re-declare a second `"default"` material, texture, or geometry short-name. Override the built-in `Default*` members instead.

### Optional description fields

| Property | JSON key | Notes |
|----------|----------|--------|
| `RenderControllers` | `render_controllers` | Defaults to `["controller.render.{name}"]` |
| `EmitDefaultRenderController` | - | When `true` (default), auto-writes a simple RC for any unregistered `controller.render.*` id listed above |
| `Animations` | `animations` | Short-name → animation / animation controller id |
| `Scripts` | `scripts` | `ClientEntityScripts`: `Initialize`, `PreAnimation`, `Animate`, `Scale` / `ScaleX` / `ScaleY` / `ScaleZ` |
| `SoundEffects` | `sound_effects` | Short-name → sound definition (for animations) |
| `EntitySounds` | *(rp/sounds.json)* | Gameplay sounds (`entity_sounds.entities`); see [Entity sounds](#entity-sounds) |
| `ParticleEffects` | `particle_effects` | Short-name → particle identifier |
| `SpawnEgg` | `spawn_egg` | `BaseColor` + `OverlayColor`, or `Texture` (+ optional `TextureIndex`) |
| `EnableAttachables` | `enable_attachables` | Optional bool |
| `HideArmor` | `hide_armor` | Optional bool |
| `MinEngineVersion` | `min_engine_version` | Optional string (e.g. player persona constraint) |
| `FormatVersion` | `format_version` | Defaults to `1.10.0` |

Example with scripts and animations:

```csharp
public override Dictionary<string, string>? Animations => new()
{
    ["walk"] = "animation.my_entity.walk",
    ["controller"] = "controller.animation.my_entity",
};

public override ClientEntityScripts? Scripts => new()
{
    Initialize = ["v.scale = 1;"],
    Animate =
    [
        "controller",
        new Dictionary<string, string> { ["walk"] = "q.modified_move_speed" },
    ],
    Scale = "v.scale",
};
```

### Custom render controllers

A render controller maps client-entity short-names into what is drawn (`Geometry.*`, `Material.*`, `Texture.*`). Simple entities can rely on auto-emit; define a `RenderController` when you need layering, arrays, or bone-specific materials.

```csharp
public class MyRenderController : RenderController
{
    public override string ControllerId => "controller.render.my_entity";
    public override string[] Textures => ["Texture.default", "Texture.overlay"];
}

// On the client entity:
public override string[] RenderControllers => ["controller.render.my_entity"];
public override bool EmitDefaultRenderController => false; // you registered your own

// On the pack:
pack.AddRenderController<MyRenderController>();
```

Useful `RenderController` members:

| Property | Default | Purpose |
|----------|---------|---------|
| `ControllerId` | *(required)* | e.g. `controller.render.my_entity` |
| `FileName` | derived from id | Output under `rp/render_controllers/` |
| `Geometry` | `Geometry.default` | Geometry Molang reference |
| `Materials` | `[{ "*": "Material.default" }]` | Bone pattern → material reference |
| `Textures` | `["Texture.default"]` | Texture layers (bottom → top) |
| `TextureArrays` / `GeometryArrays` / `MaterialArrays` | `null` | `arrays` block for dynamic selection |
| `PartVisibility` | `null` | Bone pattern → Molang |
| `Color` | `null` | Optional RGBA Molang components |

You can also build a simple controller without a subclass:

```csharp
pack.AddRenderController(RenderController.CreateSimple("controller.render.my_entity"));
// or compile a built instance yourself:
// string json = RenderController.CompileFromInstance(controller);
```

> [!TIP]
> When reusing a vanilla controller id (e.g. `controller.render.cow`), set `EmitDefaultRenderController` to `false` so ingot does not write a local stub file for it.

### Entity sounds

> [!WARNING]
> Custom entity ids do **not** inherit vanilla sound mappings. Without `EntitySounds`, you typically only hear generic damage audio.

Override `EntitySounds` on the client entity. ingot writes `rp/sounds.json` under `entity_sounds.entities`:

```csharp
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
```

Event keys are the gameplay names Bedrock fires (`ambient`, `hurt`, `death`, `step`, `milk`, …). Values are sound definition names from the vanilla (or your) `sound_definitions` (e.g. `mob.cow.hurt`). You can reuse vanilla definitions without re-shipping audio files.

For common livestock-style mappings, use the helper:

```csharp
public override ClientEntitySounds? EntitySounds =>
    ClientEntitySounds.FromVanilla("cow", includeMilk: true);
```

`FromVanilla` maps `ambient`/`hurt`/`death`/`step` (and optionally `milk`) to `mob.{name}.*` with a default pitch range of `0.8`–`1.2`.

> [!NOTE]
> `SoundEffects` on the client entity is separate: those are short-names for **animation** sound hooks, not the `sounds.json` entity map.

### Compiled client entity shape

```json
{
    "format_version": "1.10.0",
    "minecraft:client_entity": {
        "description": {
            "identifier": "mynamespace:my_entity",
            "materials": {
                "default": "entity_alphatest"
            },
            "textures": {
                "default": "textures/entity/my_entity"
            },
            "geometry": {
                "default": "geometry.my_entity"
            },
            "render_controllers": [
                "controller.render.my_entity"
            ],
            "spawn_egg": {
                "base_color": "#db7500",
                "overlay_color": "#242222"
            }
        }
    }
}
```

## Full Example

See [`CowEntity.cs`](../../ingot.Example/Entities/CowEntity.cs) in `ingot.Example`: presets + event DSL + nested `Baby`/`Adult`/`Client`, `EntitySounds.FromVanilla("cow")`, and `CowV3RenderController`.

Next: learn about [entity component groups](entity-component-groups.md) and [entity events](entity-events.md).
