# Resource Packs & Textures

ingot builds **both** a behaviour pack (`bp/`) and a resource pack (`rp/`) from a single `Pack` object. 

- Behaviour definitions (`Block`, `Item`, traits, etc.) describe *what* your content is and how it behaves.
- Resource assets describe how it *looks* (and eventually sounds, particles, etc.).

For blocks and items, the primary resource concern today is **textures**.

## The Pack Class (recommended)

Use `Pack.Create` as the single entry point. Textures declared on your `Block` and `Item` classes are auto-registered during compile:

```csharp
using ingot.Core;

Pack pack = Pack.Create(Guid.NewGuid().ToString(), "My Addon", "Example with resources")
    .AddBlock<MyBlock>()   // textures declared on the block class
    .AddItem<MyItem>();    // icon path declared on the item class

pack.Compile("./output");
```

On the behaviour side, provide optional source PNG paths:

```csharp
// MyBlock.cs
public override MaterialInstances MaterialInstances => new()
{
    All = new MaterialInstance("my_block", MaterialInstance.RenderMethods.Opaque, "assets/my_block.png")
};

// MyItem.cs
public override string Texture => "my_item";
public override string? TexturePath => "assets/my_item.png";
```

Manual texture registration is still available when you need overrides or assets not tied to a specific block/item:

```csharp
pack.AddBlockTexture("custom_key", "assets/custom.png")
    .AddItemTexture("another_key", "assets/another.png")
    .AddGeometry("geometry.my_block", "assets/my_block.geo.json");
```

Manual registrations take precedence over auto-discovered paths for the same key.

Key `Pack` members:

- `Create(string behaviourUuid, string name, string description, ...)` - creates linked behaviour and resource packs.
- `AddBlock<T>()`, `AddItem<T>()`, `AddEntity<T>()`, `AddRecipe<T>()`, `AddLootTable<T>()` - register content (fluent, returns `Pack`).
- `AddBlockTexture(string key, string sourcePngPath)` - manual block texture override.
- `AddItemTexture(string key, string sourcePngPath)` - manual item texture override.
- `AddGeometry(string identifier, string sourceGeoJsonPath)` - register a block geometry file (`.geo.json`).
- `ScriptsEnabled` - enables Script API script generation during compile.
- `AddService(sourceFile, name?, intervalTicks?)` - registers a [service](script-services.md) whose tick body is wrapped in `system.runInterval` (default every tick) and written to `bp/scripts/services/`.
- `ScriptEntry` - script module entry path (defaults to `scripts/main.js`).
- `ScriptApiModules` - Script API module dependencies (defaults to `@minecraft/server` 2.8.0).

The behaviour pack manifest includes a script module only when block/item events or services produce at least one script file.

- `PackIcon` - optional path to a PNG copied into both `bp/` and `rp/` using the source filename (e.g. `pack_icon.png`).
- `Compile(string outputDir)` - deletes any existing `bp/` and `rp/` subfolders, then compiles fresh ones under the output directory.
- `CompileMcaddon(string outputPath)` - deletes any existing `.mcaddon` file, compiles to a temporary directory, zips a `.mcaddon` with `{Name} BP/` and `{Name} RP/` at the archive root, then deletes the temp files.
- `CompileComMojang(string comMojangPath)` - deletes any existing development pack folders, then compiles directly into `development_behavior_packs/{Name} BP/` and `development_resource_packs/{Name} RP/` under a `com.mojang` folder.

All three methods remove prior pack output before compiling so stale files are not left behind. `.ingot` cache files and `ingot.log` in the output directory are preserved.

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
| `CompileComMojang("/path/to/games/com.mojang")` | Fast iteration — writes straight into development pack folders |

```csharp
pack.CompileMcaddon("./artifacts/example/ingot example.mcaddon");

pack.CompileComMojang(
    "/home/user/.var/app/io.mrarm.mcpelauncher/data/mcpelauncher/games/com.mojang");
```

> [!NOTE]
> The lower-level `BehaviourPack` and `ResourcePack` types are still available if you need direct access. `Pack` is the recommended developer surface.

## Texture Keys - the Bridge Between Behaviour and Resources

The strings you write in behaviour code are **keys**, not file paths:

- `new MaterialInstance("block_of_dense_lasagna", ...)` in a `Block` or `BlockPermutation`
- `public override string Texture => "lasagna";` on an `Item`
- `string IDestructionParticles.Texture => "my_particles";` (via trait)

These keys become real images when you either:

1. Provide a `SourcePath` on the `MaterialInstance` or `TexturePath` on the `Item` (auto-registered during compile), or
2. Register them manually with `Pack.AddBlockTexture` / `Pack.AddItemTexture`.

If a key has no source PNG (auto or manual), ingot still emits an atlas entry but warns at compile time. Missing assets show as purple/black textures in Minecraft.

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

Vanilla geometry identifiers such as `minecraft:geometry.full_block` do not need to be registered — only custom models you author yourself.

The identifier inside your `.geo.json` file must match what you reference in behaviour. For example, if your block uses `geometry.my_block`, the geometry description in the JSON should look like:

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

- `rp/textures/blocks/<key>.png` - your block textures (auto-registered or from `AddBlockTexture`)
- `rp/textures/items/<key>.png` - your item icons (auto-registered or from `AddItemTexture`)
- `rp/models/blocks/<name>.geo.json` - custom block geometry (from `AddGeometry`)
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

ingot also creates the other standard texture subfolders (`entity`, `particle`, etc.) so you have a complete skeleton for future resource features.

## Organizing Assets in Your Project

A common layout is to keep your source art next to your C# code:

```
MyAddon/
├── MyAddon.csproj
├── Program.cs
├── Data/
│   ├── block_of_dense_lasagna.png
│   └── lasagna.png
├── DenseLasagnaBlock.cs
└── LasagnaItem.cs
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
    .AddItemTexture("lasagna", Path.Combine(dataDir, "lasagna.png"));
```

Alternatively, reference paths from your block/item definitions via `MaterialInstance` `SourcePath` or `Item.TexturePath`. Paths are resolved with `Path.GetFullPath` at registration time.

You can share the same PNG file under different keys or even register the same key for both a block and an item if that makes sense for your design.

## Putting It All Together

Here is a minimal complete example (the `ingot.Example` project contains a richer version using the lasagna content):

```csharp
using ingot.Core;

const string packUuid = "77f1fef2-bb39-411a-b25c-ae475c21169f";
string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");

Pack pack = Pack.Create(packUuid, "My Addon", "Example with resources")
    .AddBlock<MyBlock>()
    .AddItem<MyItem>();

pack.PackIcon = Path.Combine(dataDir, "pack_icon.png");
pack.AddBlockTexture("my_block", Path.Combine(dataDir, "my_block.png"))
    .AddItemTexture("my_item", Path.Combine(dataDir, "my_item.png"))
    .AddGeometry("geometry.my_block", Path.Combine(dataDir, "my_block.geo.json"));

pack.Compile("./output");
```

After compilation you will have a ready-to-use `bp/` folder and `rp/` folder (plus `manifest.json` files that cross-link them when `LinkPacks` is true). Use `CompileMcaddon` or `CompileComMojang` when you want to skip the manual copy step.

See the [`ingot.Example`](../../ingot.Example) project for a working end-to-end sample that registers textures from `Data/`, includes blocks, items, entities, recipes, states, permutations, and scripts. This repo's example compiles to `./artifacts/example/`.

## Current Scope and Limitations

- Texture support is currently limited to simple single-image registrations for blocks (via `terrain_texture.json`) and items (via `item_texture.json`).
- Geometry support is limited to manual block `.geo.json` registration via `AddGeometry` (no auto-discovery from block classes yet).
- No built-in support yet for:
  - Texture variations / random or weighted textures
  - Flipbook (animated) textures
  - PBR texture sets
  - Entity geometry / model files
  - Sound definitions, particles, music, etc.
  - Entity resources (models, textures, render controllers, attachables)
- An empty `ResourcePack` (no textures or geometry registered) is perfectly valid - it simply produces no custom atlas entries while still creating the standard folder skeleton.

These areas will expand in future releases. The current design (key-based registration + asset copying) is intended to scale to those features.

## See Also

- [Making a Block](block.md) and [Block Material Instances](block-mat-instances.md)
- [Items](item.md) and [Item Events](item-events.md)
- [Block Events](block-events.md) and [Script Services](script-services.md)
- [Block Permutations](block-permutations.md)
- API reference for `ResourcePack`

The texture key is the bridge between behaviour definitions and resource assets. Declare the key in your `Block`/`Item` code, optionally provide a source PNG path, and `Pack.Compile` takes care of the rest.
