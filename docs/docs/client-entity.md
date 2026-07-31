# Client Entities & Render Controllers

Behaviour entities only define gameplay. **Visuals** live in the resource pack via a separate `ClientEntity` (or `ClientEntity<TParent>`) class, which compiles to `minecraft:client_entity`. Render controllers decide how those short-names are drawn.

For the behaviour side (traits, groups, events), see [Making an Entity](entity.md). For texture and pack layout details, see [Resource Packs & Textures](resource-packs.md#entity-textures-and-client-entities).

## Minimal Client Entity

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

### Auto-discovery from the behaviour entity

`Pack.AddEntity<T>(discoverClient: true)` (the default) looks for a matching client entity without a separate `AddClientEntity` call:

1. Nested type `MyEntity.Client : ClientEntity<MyEntity>`
2. Or a type named `MyClientEntity` in the same assembly
3. Or `Entity.ClientEntityType` when set explicitly

Use `AddEntity<T>(discoverClient: false)` to skip RP discovery. Nested `RenderController` types on the entity are registered when found; top-level controllers still use `AddRenderController<T>()`.

> [!TIP]
> Prefer a nested `Client` type on the entity (`MyEntity.Client : ClientEntity<MyEntity>`) so `AddEntity` discovers visuals without a separate `AddClientEntity` call.

## Entity Textures (PNG Files)

Client entity JSON uses **paths** (for example `textures/entity/my_entity`), not atlas keys like items/blocks.

To copy a source PNG into the resource pack:

1. **Auto (default texture only):** set `DefaultTexturePath` to a file on disk. During `Pack.Compile`, ingot strips a leading `textures/entity/` from `DefaultTexture` and registers `rp/textures/entity/<relative>.png`.
2. **Manual:** `pack.AddEntityTexture("my_entity", "Data/my_entity.png")` (or `"subdir/my_entity"` for nested paths).

Extra texture short-names (angry, alt, ...) are written into JSON only. Register their PNGs with `AddEntityTexture` if you need the files in the pack.

> [!NOTE]
> Calling `ClientEntity.Compile(type)` alone does **not** copy texture files. PNG registration runs only under full pack compile when `CompilerState.CurrentPack` is set.

## Custom Short-Names (Attributes)

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
- If the id is omitted, the short-name is derived from the member name (with a `Default...` to `default` special case).
- Override `DefaultMaterial` / `DefaultTexture` / `DefaultGeometry` to change the built-in `default` short-names.

> [!CAUTION]
> Do not re-declare a second `"default"` material, texture, or geometry short-name. Override the built-in `Default*` members instead.

## Optional Description Fields

| Property | JSON key | Notes |
|----------|----------|--------|
| `RenderControllers` | `render_controllers` | Defaults to `["controller.render.{name}"]` |
| `EmitDefaultRenderController` | - | When `true` (default), auto-writes a simple RC for any unregistered `controller.render.*` id listed above |
| `Animations` | `animations` | Short-name maps to animation / animation controller id |
| `Scripts` | `scripts` | `ClientEntityScripts`: `Initialize`, `PreAnimation`, `Animate`, `Scale` / `ScaleX` / `ScaleY` / `ScaleZ` |
| `SoundEffects` | `sound_effects` | Short-name maps to sound definition (for animations) |
| `EntitySounds` | *(rp/sounds.json)* | Gameplay sounds (`entity_sounds.entities`); see [Entity sounds](#entity-sounds) |
| `ParticleEffects` | `particle_effects` | Short-name maps to particle identifier |
| `SpawnEgg` | `spawn_egg` | `BaseColor` + `OverlayColor`, or `Texture` (+ optional `TextureIndex`) |
| `EnableAttachables` | `enable_attachables` | Optional bool |
| `HideArmor` | `hide_armor` | Optional bool |
| `MinEngineVersion` | `min_engine_version` | Optional string (e.g. player persona constraint) |
| `ExtraTextures` | `textures` (merged) | Optional `Dictionary<string, string>` of short-name to path, merged into the textures map (in addition to attributed members / `DefaultTexture`). Useful for many render-controller array entries. |
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

## Custom Render Controllers

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
| `Materials` | `[{ "*": "Material.default" }]` | Bone pattern maps to material reference |
| `Textures` | `["Texture.default"]` | Texture layers (bottom to top) |
| `TextureArrays` / `GeometryArrays` / `MaterialArrays` | `null` | `arrays` block for dynamic selection |
| `PartVisibility` | `null` | Bone pattern maps to Molang |
| `Color` | `null` | Optional RGBA Molang components |
| `IsHurtColor` | `null` | Optional hurt-color overlay flag |
| `IgnoreLighting` | `null` | Optional lighting bypass |
| `RebuildAnimationMatrices` | `null` | Optional animation matrix rebuild |

You can also build a simple controller without a subclass:

```csharp
pack.AddRenderController(RenderController.CreateSimple("controller.render.my_entity"));
// or compile a built instance yourself:
// string json = RenderController.CompileFromInstance(controller);
```

> [!TIP]
> When reusing a vanilla controller id (e.g. `controller.render.cow`), set `EmitDefaultRenderController` to `false` so ingot does not write a local stub file for it.

## Entity Sounds

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

Event keys are the gameplay names Bedrock fires (`ambient`, `hurt`, `death`, `step`, `milk`, ...). Values are sound definition names from the vanilla (or your) `sound_definitions` (e.g. `mob.cow.hurt`). You can reuse vanilla definitions without re-shipping audio files.

For common livestock-style mappings, use the helper:

```csharp
public override ClientEntitySounds? EntitySounds =>
    ClientEntitySounds.FromVanilla("cow", includeMilk: true);
```

`FromVanilla` maps `ambient`/`hurt`/`death`/`step` (and optionally `milk`) to `mob.{name}.*` with a default pitch range of `0.8`-`1.2`.

> [!NOTE]
> `SoundEffects` on the client entity is separate: those are short-names for **animation** sound hooks, not the `sounds.json` entity map.

## Compiled Client Entity Shape

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

See [`CowEntity.cs`](../../ingot.Example/Entities/CowEntity.cs) in `ingot.Example`: nested `Client`, `EntitySounds.FromVanilla("cow")`, and `CowV3RenderController`.

## See Also

- [Making an Entity](entity.md) - behaviour entities, traits, properties, registration
- [Entity Component Groups](entity-component-groups.md) and [Entity Events](entity-events.md)
- [Resource Packs & Textures](resource-packs.md) - entity textures, geometry, animations, sounds
