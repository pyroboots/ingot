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
| `ComponentGroups`  | `EntityComponentGroup[]`                  | `[]`        | Named component sets toggled by events. See [Entity Component Groups](entity-component-groups.md). |
| `Events`           | `Dictionary<Identifier, IEntityEventAction[]>` | `{}`     | Event definitions and their actions. See [Entity Events](entity-events.md). |

These are written into the `description`, `component_groups`, `components`, and `events` sections of the generated entity JSON.

## Compiled Output

A minimal entity compiles to:

```json
{
    "format_version": "1.20.10",
    "minecraft:entity": {
        "description": {
            "identifier": "mynamespace:my_entity",
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
    .AddEntity<MyEntity>();

pack.Compile("./output");
```

Capture identifiers from your entity class when you need them for cross-references elsewhere in your project.

This writes `bp/entities/my_entity.json` (filename is the part after the `:` in the identifier).

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

Calling `ClientEntity.Compile(type)` alone does **not** copy files - that runs only under full pack compile when `CompilerState.CurrentPack` is set.

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
- Override `DefaultMaterial` / `DefaultTexture` / `DefaultGeometry` to change the built-in `default` short-names - do not re-declare a second `"default"` member.

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
```

When reusing a vanilla controller id (e.g. `controller.render.cow`), set `EmitDefaultRenderController` to `false` so ingot does not write a local stub file for it.

### Entity sounds

Custom entity ids do **not** inherit vanilla sound mappings. Without `EntitySounds`, you typically only hear generic damage audio.

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

`SoundEffects` on the client entity is separate: those are short-names for **animation** sound hooks, not the `sounds.json` entity map.

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

See the [`ingot.Example`](../../ingot.Example) project:

- `LasagnaSpiritEntity.cs` - custom flying mob (behaviour + client entity)
- `CowEntity.cs` - full cow-style behaviour, client entity, render controller, and `EntitySounds` as `test:custom_cow`

Next: learn about [entity component groups](entity-component-groups.md) and [entity events](entity-events.md).
