# Resource Packs & Textures

ingot builds **both** a behaviour pack (`bp/`) and a resource pack (`rp/`) from a single `Pack` object.

- Behaviour definitions (`Block`, `Item`, `Entity`, traits, etc.) describe *what* your content is and how it behaves.
- Resource assets describe how it *looks* (textures, client entities, render controllers, geometry, …).

Supported on the resource side today:

- **Blocks / items** - texture atlases, optional block geometry
- **Entities** - client entities, render controllers, entity texture PNGs, entity sound mappings (`sounds.json`)
- **Particles** - effect JSON via `AddParticle`, particle textures via `AddParticleTexture`
- **Sounds** - custom definitions via `RegisterSoundDefinition` (optional source audio copy)
- **Animations** - animation JSON via `AddAnimation`

## The Pack Class (recommended)

Use `Pack.Create` as the single entry point. Textures declared on your `Block` and `Item` classes (and optional `ClientEntity.DefaultTexturePath`) are auto-registered during compile:

```csharp
using ingot.Core;

const string packUuid = "77f1fef2-bb39-411a-b25c-ae475c21169f";

Pack pack = Pack.Create(packUuid, "My Addon", "Example with resources")
    .AddBlock<MyBlock>()              // textures declared on the block class
    .AddItem<MyItem>()                // icon path declared on the item class
    .AddEntity<MyEntity>()
    .AddClientEntity<MyClientEntity>(); // materials / textures / geometry short-names

pack.Compile("./output");
```

> [!IMPORTANT]
> Use a fixed pack UUID in real projects. Regenerating UUIDs every build makes Minecraft treat each compile as a different pack.

On the behaviour / client side, provide optional source PNG paths:

```csharp
// MyBlock.cs
public override MaterialInstances MaterialInstances => new()
{
    All = new MaterialInstance("my_block", MaterialInstance.RenderMethods.Opaque, "assets/my_block.png")
};

// MyItem.cs
public override string Texture => "my_item";
public override string? TexturePath => "assets/my_item.png";

// MyClientEntity.cs
public override string DefaultTexture => "textures/entity/my_entity";
public override string? DefaultTexturePath => "assets/my_entity.png";
```

Manual texture registration is still available when you need overrides or assets not tied to a specific definition:

```csharp
pack.AddBlockTexture("custom_key", "assets/custom.png")
    .AddItemTexture("another_key", "assets/another.png")
    .AddEntityTexture("my_entity", "assets/my_entity.png")
    .AddGeometry("geometry.my_block", "assets/my_block.geo.json");
```

> [!NOTE]
> Manual registrations take precedence over auto-discovered paths for the same key.

Key `Pack` members:

- `Create(string behaviourUuid, string name, string description, ...)` - creates linked behaviour and resource packs.
- `AddBlock<T>()`, `AddItem<T>()`, `AddEntity<T>()`, `AddRecipe<T>()`, `AddLootTable<T>()` - register content (fluent, returns `Pack`).
- `AddBlockTexture(string key, string sourcePngPath)` - manual block texture override.
- `AddItemTexture(string key, string sourcePngPath)` - manual item texture override.
- `AddEntityTexture(string key, string sourcePngPath)` - entity texture under `textures/entity/`.
- `AddGeometry(string identifier, string sourceGeoJsonPath)` - register a block geometry file (`.geo.json`).
- `AddParticle(string identifier, string sourceJsonPath, string? rpName = null)` - register a particle effect JSON under `particles/`.
- `AddParticleTexture(string key, string sourcePngPath, string? rpName = null)` - particle texture PNG under `textures/particles/`.
- `AddClientEntity<T>()` - resource-pack client entity (materials, textures, geometry, spawn egg, …).
- `AddRenderController<T>()` - custom render controller; simple per-entity controllers are auto-emitted when a client entity lists them.
- `ScriptsEnabled` - enables Script API script generation during compile.
- `AddService(sourceFile, name?, intervalTicks?)` - registers a [service](script-services.md) whose tick body is wrapped in `system.runInterval` (default every tick) and written to `bp/scripts/services/`.
- `ScriptEntry` - script module entry path (defaults to `scripts/main.js`).
- `ScriptApiModules` - Script API module dependencies (defaults to `@minecraft/server` 2.8.0).

The behaviour pack manifest includes a script module only when block/item events or services produce at least one script file.

- `PackIcon` - optional path to a PNG copied into both `bp/` and `rp/` using the source filename (e.g. `pack_icon.png`).
- `Compile(string outputDir)` - deletes any existing `bp/` and `rp/` subfolders, then compiles fresh ones under the output directory.
- `CompileMcaddon(string outputPath)` - deletes any existing `.mcaddon` file, compiles to a temporary directory, zips a `.mcaddon` with `{Name} BP/` and `{Name} RP/` at the archive root, then deletes the temp files.
- `CompileComMojang(string comMojangPath)` - deletes any existing development pack folders, then compiles directly into `development_behavior_packs/{Name} BP/` and `development_resource_packs/{Name} RP/` under a `com.mojang` folder.

> [!WARNING]
> All three compile methods **delete prior pack output** before writing so stale files are not left behind. Hand-edited files inside those output folders are wiped. `.ingot` cache files and `ingot.log` in the output directory are preserved.

> [!TIP]
> On an interactive terminal, verbose compiles show Spectre.Console progress bars for the major pack stages. Use `verbose: false` in tests or automation when you want silent compiles.

Set a pack icon before compiling:

```csharp
string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
pack.PackIcon = Path.Combine(dataDir, "pack_icon.png");
pack.Compile("./output");
```

This copies the file to `{outputDir}/bp/pack_icon.png` and `{outputDir}/rp/pack_icon.png`.

When `ScriptsEnabled` is `true`, **ingot** writes:

- per-content event scripts under `bp/scripts/blocks/` and `bp/scripts/items/` ([Block Events](block-events.md), [Item Events](item-events.md))
- [service](script-services.md) scripts under `bp/scripts/services/` (via `AddService`, wrapped to run every tick)
- a generated `bp/scripts/main.js` entry that imports all of the above

## Deploying to Minecraft

| Method | When to use |
|--------|-------------|
| `Compile("./output")` | Inspect generated JSON, commit `bp/`/`rp/` to version control, or copy folders manually |
| `CompileMcaddon("./output/my-pack.mcaddon")` | Share or import a single addon file with the correct zip structure |
| `CompileComMojang("/path/to/games/com.mojang")` | Fast iteration - writes straight into development pack folders |

```csharp
pack.CompileMcaddon("./artifacts/example/ingot example.mcaddon");

pack.CompileComMojang(
    "/home/user/.var/app/io.mrarm.mcpelauncher/data/mcpelauncher/games/com.mojang");
```

> [!NOTE]
> The lower-level `BehaviourPack` and `ResourcePack` types are still available if you need direct access. `Pack` is the recommended developer surface.

## Texture Keys - the Bridge Between Behaviour and Resources

### Blocks and items (atlas keys)

The strings you write for blocks and items are **atlas keys**, not file paths:

- `new MaterialInstance("block_of_dense_lasagna", ...)` in a `Block` or `BlockPermutation`
- `public override string Texture => "lasagna";` on an `Item`
- `string IDestructionParticles.Texture => "my_particles";` (via trait)

These keys become real images when you either:

1. Provide a `SourcePath` on the `MaterialInstance` or `TexturePath` on the `Item` (auto-registered during compile), or
2. Register them manually with `Pack.AddBlockTexture` / `Pack.AddItemTexture`.

> [!WARNING]
> If a key has no source PNG (auto or manual), ingot still emits an atlas entry but warns at compile time. Missing assets show as purple/black textures in Minecraft.

### Entity textures and client entities

Entity textures work differently. Client entity short-names point at **paths** under the resource pack (for example `textures/entity/my_mob`), not entries in `terrain_texture.json` / `item_texture.json`.

| Mechanism | What it does |
|-----------|----------------|
| `DefaultTexture` on `ClientEntity` | Path written into `rp/entity/*.json` under the `default` texture short-name |
| `DefaultTexturePath` | Source PNG auto-copied to `rp/textures/entity/<relative>.png` during pack compile |
| `Pack.AddEntityTexture(key, pngPath)` | Manual copy to `rp/textures/entity/{key}.png` (nested keys like `spider/cave` are allowed) |
| `AddClientEntity<T>()` | Writes `rp/entity/{name}.json` and can auto-emit a simple render controller |
| `AddRenderController<T>()` | Writes `rp/render_controllers/{file}.json` |
| `ClientEntity.EntitySounds` | Writes `rp/sounds.json` → `entity_sounds.entities` (ambient/hurt/death/step/…) |

> [!WARNING]
> Without `EntitySounds`, custom entity ids only get generic damage audio. Full API: [Making an Entity](entity.md#client-entities--render-controllers) (including [entity sounds](entity.md#entity-sounds)).

## Particles

Particle effects are resource-pack JSON files under `rp/particles/`. ingot does not author the effect graph for you: export or write a particle `.json` (for example with [Snowstorm](https://snowstorm.app/)), then register it on the pack so it is copied at compile time.

### Methods

| Method | Output | Description |
|--------|--------|-------------|
| `Pack.AddParticle(identifier, sourceJsonPath, rpName?)` | `rp/particles/{rpName}.json` | Registers a particle effect file. |
| `Pack.AddParticleTexture(key, sourcePngPath, rpName?)` | `rp/textures/particles/{rpName}.png` | Registers a PNG used by particle `basic_render_parameters.texture`. |

Both methods are fluent (return `Pack`) and forward to the same APIs on `ResourcePack`.

**`AddParticle` parameters**

| Parameter | Required | Description |
|-----------|----------|-------------|
| `identifier` | yes | Effect id (`namespace:name`). Must match `description.identifier` inside the JSON. This is the string you pass to Script API `dimension.spawnParticle` or `/particle`. |
| `sourceJsonPath` | yes | Path to the source particle JSON on disk. Copied as-is. |
| `rpName` | no | Filename under `particles/` without extension. Defaults to the segment after `:` in `identifier`. Nested paths are allowed (`effects/sparkle` → `rp/particles/effects/sparkle.json`). |

**`AddParticleTexture` parameters**

| Parameter | Required | Description |
|-----------|----------|-------------|
| `key` | yes | Logical key / relative path under `textures/particles/` (for example `sparkle` or `effects/sparkle`). |
| `sourcePngPath` | yes | Path to the source PNG on disk. |
| `rpName` | no | Output relative path under `textures/particles/`. Defaults to `key`. |

Empty `identifier` / `key` or empty source paths throw `ArgumentException` at registration. Missing source files throw `FileNotFoundException` at compile time.

Registered ids are visible on `ResourcePack.ParticleIds` and `ResourcePack.ParticleTextureKeys`.

### Example

```csharp
string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");

Pack pack = Pack.Create(packUuid, "My Addon", "Particles example");

pack.AddParticle(
        "mynamespace:sparkle",
        Path.Combine(dataDir, "particles/sparkle.json"))
    .AddParticleTexture(
        "sparkle",
        Path.Combine(dataDir, "textures/sparkle.png"));

// Custom output names / folders
pack.AddParticle(
    "mynamespace:heavy_smoke",
    Path.Combine(dataDir, "particles/smoke.json"),
    rpName: "effects/heavy_smoke");

pack.Compile("./output");
```

Compile output:

- `rp/particles/sparkle.json`
- `rp/textures/particles/sparkle.png`
- `rp/particles/effects/heavy_smoke.json`

Wire the texture path inside your particle JSON (no file extension):

```json
{
  "format_version": "1.10.0",
  "particle_effect": {
    "description": {
      "identifier": "mynamespace:sparkle",
      "basic_render_parameters": {
        "material": "particles_alpha",
        "texture": "textures/particles/sparkle"
      }
    },
    "components": { }
  }
}
```

### Spawning at runtime

Use the same identifier you registered:

```javascript
// Script API
dimension.spawnParticle("mynamespace:sparkle", { x: 0, y: 64, z: 0 });
```

```
/particle mynamespace:sparkle ~~~
```

> [!IMPORTANT]
> The `identifier` argument to `AddParticle` is only used for registration bookkeeping and default filenames. Minecraft loads the id from `description.identifier` inside the JSON. Keep them identical so Script API, commands, and the pack stay in sync.

> [!NOTE]
> ingot does not generate particle component graphs (emitters, curves, Molang) as C# types. Author the JSON externally, then register the file and any textures it needs.

## Block Geometry

Custom block models are `.geo.json` files exported from tools like [Blockbench](https://blockbench.net). Register them with `Pack.AddGeometry` so they are copied into the resource pack at compile time:

```csharp
pack.AddGeometry("geometry.my_block", Path.Combine(dataDir, "my_block.geo.json"))
    .AddBlock<MyBlock>();
```

The `identifier` must match the geometry referenced from behaviour:

- `public override string? Geometry => "geometry.my_block";` on a `Block`
- `Identifier IGeometry.Identifier => new("geometry.my_block");` on a `BlockPermutation` or via the `IGeometry` trait

Both `geometry.my_block` and `minecraft:geometry.my_block` are accepted. By default, the output filename is derived from the last segment of the identifier (`my_block.geo.json`). Pass an optional `rpName` to override:

```csharp
pack.AddGeometry("geometry.my_block", sourcePath, rpName: "custom_name");
// -> rp/models/blocks/custom_name.geo.json
```

> [!NOTE]
> Vanilla geometry identifiers such as `minecraft:geometry.full_block` do not need to be registered - only custom models you author yourself.

> [!IMPORTANT]
> The identifier inside your `.geo.json` file must match what you reference in behaviour. For example, if your block uses `geometry.my_block`, the geometry description in the JSON should look like:

```json
{
  "format_version": "1.21.0",
  "minecraft:geometry": [
    {
      "description": {
        "identifier": "geometry.my_block",
        "texture_width": 16,
        "texture_height": 16
      }
    }
  ]
}
```

## What Gets Generated in the Resource Pack

When `Pack.Compile` runs, the resource side produces (among other folders):

- `rp/textures/blocks/<key>.png` - block textures (auto-registered or from `AddBlockTexture`)
- `rp/textures/items/<key>.png` - item icons (auto-registered or from `AddItemTexture`)
- `rp/textures/entity/<path>.png` - entity textures (`DefaultTexturePath` or `AddEntityTexture`)
- `rp/models/blocks/<name>.geo.json` - custom block geometry (from `AddGeometry`)
- `rp/particles/<name>.json` - particle effects (from `AddParticle`)
- `rp/textures/particles/<key>.png` - particle textures (from `AddParticleTexture`)
- `rp/entity/<name>.json` - client entities (from `AddClientEntity`)
- `rp/render_controllers/<name>.json` - render controllers (registered or auto-emitted)
- `rp/sounds.json` - `entity_sounds.entities` when any client entity defines `EntitySounds`
- `rp/textures/terrain_texture.json` - maps block texture keys for `minecraft:material_instances`
- `rp/textures/item_texture.json` - maps item icon keys for `minecraft:icon`

A minimal generated `terrain_texture.json` looks like this:

```json
{
  "texture_data": {
    "block_of_dense_lasagna": {
      "textures": "textures/blocks/block_of_dense_lasagna"
    }
  }
}
```

The same shape is used for `item_texture.json`. Note that the path inside `"textures"` does **not** include the `.png` extension (Bedrock convention).

Entity textures are **not** atlas entries - they are plain PNG files referenced by path from the client entity file.

## Organizing Assets in Your Project

A common layout is to keep your source art next to your C# code:

```
MyAddon/
├── MyAddon.csproj
├── Program.cs
├── Data/
│   ├── block_of_dense_lasagna.png
│   ├── lasagna.png
│   └── my_entity.png
├── DenseLasagnaBlock.cs
├── LasagnaItem.cs
└── Entities/
    ├── MyEntity.cs
    └── MyClientEntity.cs
```

Copy `Data/` into the build output from your `.csproj`:

```xml
<ItemGroup>
  <None Include="Data\**\*" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

Then register textures in `Program.cs` using `AppContext.BaseDirectory`:

```csharp
string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");

pack.AddBlockTexture("block_of_dense_lasagna", Path.Combine(dataDir, "dense_lasagna.png"))
    .AddItemTexture("lasagna", Path.Combine(dataDir, "lasagna.png"))
    .AddEntityTexture("my_entity", Path.Combine(dataDir, "my_entity.png"));
```

Alternatively, reference paths from definitions via `MaterialInstance` `SourcePath`, `Item.TexturePath`, or `ClientEntity.DefaultTexturePath`. Paths are resolved with `Path.GetFullPath` at registration time.

You can share the same PNG file under different keys or even register the same key for both a block and an item if that makes sense for your design.

## Putting It All Together

Here is a minimal complete example (the `ingot.Example` project contains a richer version using the lasagna content):

```csharp
using ingot.Core;

const string packUuid = "77f1fef2-bb39-411a-b25c-ae475c21169f";
string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");

Pack pack = Pack.Create(packUuid, "My Addon", "Example with resources")
    .AddBlock<MyBlock>()
    .AddItem<MyItem>()
    .AddEntity<MyEntity>()
    .AddClientEntity<MyClientEntity>();

pack.PackIcon = Path.Combine(dataDir, "pack_icon.png");
pack.AddBlockTexture("my_block", Path.Combine(dataDir, "my_block.png"))
    .AddItemTexture("my_item", Path.Combine(dataDir, "my_item.png"))
    .AddEntityTexture("my_entity", Path.Combine(dataDir, "my_entity.png"))
    .AddGeometry("geometry.my_block", Path.Combine(dataDir, "my_block.geo.json"))
    .AddParticle("mynamespace:sparkle", Path.Combine(dataDir, "particles/sparkle.json"))
    .AddParticleTexture("sparkle", Path.Combine(dataDir, "textures/sparkle.png"));

pack.Compile("./output");
```

After compilation you will have a ready-to-use `bp/` folder and `rp/` folder (plus `manifest.json` files that cross-link them when `LinkPacks` is true). Use `CompileMcaddon` or `CompileComMojang` when you want to skip the manual copy step.

See the [`ingot.Example`](../../ingot.Example) project for a working end-to-end sample that registers textures from `Data/`, includes blocks, items, entities, recipes, states, permutations, and scripts. This repo's example compiles to `./artifacts/example/`.

## Current Scope and Limitations

**Supported**

- Block textures via `terrain_texture.json` (auto or `AddBlockTexture`)
- Item textures via `item_texture.json` (auto or `AddItemTexture`)
- Entity textures as loose PNGs under `textures/entity/` (`DefaultTexturePath` or `AddEntityTexture`)
- Particle effects via `AddParticle` (JSON under `particles/`) and particle textures via `AddParticleTexture` (`textures/particles/`)
- Client entities (`AddClientEntity`) and render controllers (`AddRenderController` / auto-emit)
- Entity sound mappings via `ClientEntity.EntitySounds` → `rp/sounds.json` (`entity_sounds.entities`)
- Sound definitions via `RegisterSoundDefinition` (`sound_definitions.json` + optional source audio copy)
- Block geometry via manual `AddGeometry` (`.geo.json` under `models/blocks/`)
- Animations via `AddAnimation` (JSON under `animations/`)

> [!NOTE]
> **Not yet first-class:** texture variations / weighted atlas textures, flipbook (animated) textures, PBR texture sets, attachable registration, music discs as C# types, generating particle effect JSON from a C# DSL (register authored `.json` files with `AddParticle` instead), and auto-registration of source PNGs for *extra* attributed client-entity textures (only `DefaultTexturePath` auto-registers; use `AddEntityTexture` for the rest).

An empty `ResourcePack` (no textures or client entities) is still valid - it produces the standard folder skeleton without custom content.

## See Also

- [Making a Block](block.md) and [Block Material Instances](block-mat-instances.md)
- [Items](item.md) and [Item Events](item-events.md)
- [Making an Entity](entity.md) (including [client entities & render controllers](entity.md#client-entities--render-controllers))
- [Block Events](block-events.md) and [Script Services](script-services.md)
- [Block Permutations](block-permutations.md)
- API reference for `Pack.AddParticle` / `Pack.AddParticleTexture` and `ResourcePack`

Declare keys/paths on your content types, optionally provide source PNGs, and `Pack.Compile` writes the resource pack.