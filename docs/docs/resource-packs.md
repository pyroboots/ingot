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
    .AddItemTexture("another_key", "assets/another.png");
```

Manual registrations take precedence over auto-discovered paths for the same key.

Key `Pack` members:

- `Create(string behaviourUuid, string name, string description, ...)` - creates linked behaviour and resource packs.
- `AddBlock<T>()`, `AddItem<T>()`, `AddEntity<T>()`, `AddRecipe<T>()`, `AddLootTable<T>()` - register content (fluent, returns `Pack`).
- `AddBlockTexture(string key, string sourcePngPath)` - manual block texture override.
- `AddItemTexture(string key, string sourcePngPath)` - manual item texture override.
- `ScriptsEnabled` - enables Script API in the behaviour pack manifest and generates `scripts/main.js`.
- `ScriptEntry` - script module entry path (defaults to `scripts/main.js`).
- `ScriptApiModules` - Script API module dependencies (defaults to `@minecraft/server` 2.8.0).
- `PackIcon` - optional path to a PNG copied into both `bp/` and `rp/` using the source filename (e.g. `pack_icon.png`).
- `Compile(string outputDir)` - compiles both `bp/` and `rp/`.

Set a pack icon before compiling:

```csharp
pack.PackIcon = "assets/pack_icon.png";
pack.Compile("./output");
```

This copies the file to `{outputDir}/bp/pack_icon.png` and `{outputDir}/rp/pack_icon.png`.

When `ScriptsEnabled` is `true` and blocks or items define [Block Events](block-events.md) or [Item Events](item-events.md), **ingot** also writes per-content scripts under `bp/scripts/blocks/` and `bp/scripts/items/` and imports them from `main.js`.

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

## What Gets Generated in the Resource Pack

When `Pack.Compile` runs, the resource side produces (among other folders):

- `rp/textures/blocks/<key>.png` - your block textures (auto-registered or from `AddBlockTexture`)
- `rp/textures/items/<key>.png` - your item icons (auto-registered or from `AddItemTexture`)
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
├── assets/
│   ├── block_of_dense_lasagna.png
│   └── lasagna.png
├── DenseLasagnaBlock.cs
└── LasagnaItem.cs
```

Then reference them from your block/item definitions:

```csharp
All = new MaterialInstance("block_of_dense_lasagna", MaterialInstance.RenderMethods.AlphaTest, "assets/block_of_dense_lasagna.png")
```

```csharp
public override string? TexturePath => "assets/lasagna.png";
```

Paths are resolved with `Path.GetFullPath` at registration time, so they work regardless of the current working directory when you later run the compiler.

You can share the same PNG file under different keys or even register the same key for both a block and an item if that makes sense for your design.

## Putting It All Together

Here is a minimal complete example (the `ingot.Example` project contains a richer version using the lasagna content):

```csharp
using ingot.Core;

Pack pack = Pack.Create(Guid.NewGuid().ToString(), "My Addon", "Example with resources")
    .AddBlock<MyBlock>()
    .AddItem<MyItem>();

pack.Compile("./output");
```

Textures are declared on `MyBlock` and `MyItem` via `SourcePath` / `TexturePath`. Use `AddBlockTexture` / `AddItemTexture` only when you need manual overrides.

After compilation you will have a ready-to-use `bp/` folder and `rp/` folder (plus `manifest.json` files that cross-link them when `LinkPacks` is true).

See the [`ingot.Example`](../../ingot.Example) project for a working end-to-end sample that includes blocks, items, recipes, states, permutations, and both block and item textures. The docs use `./output` as a generic compile path; this repo's example compiles to `./artifacts/example/`.

## Current Scope and Limitations

- Texture support is currently limited to simple single-image registrations for blocks (via `terrain_texture.json`) and items (via `item_texture.json`).
- No built-in support yet for:
  - Texture variations / random or weighted textures
  - Flipbook (animated) textures
  - PBR texture sets
  - Custom geometry / model files
  - Sound definitions, particles, music, etc.
  - Entity resources (models, textures, render controllers, attachables)
- An empty `ResourcePack` (no textures registered) is perfectly valid - it simply produces no custom atlas entries while still creating the standard folder skeleton.

These areas will expand in future releases. The current design (key-based registration + asset copying) is intended to scale to those features.

## See Also

- [Making a Block](block.md) and [Block Material Instances](block-mat-instances.md)
- [Items](item.md) and [Item Events](item-events.md)
- [Block Events](block-events.md)
- [Block Permutations](block-permutations.md)
- API reference for `ResourcePack`

The texture key is the bridge between behaviour definitions and resource assets. Declare the key in your `Block`/`Item` code, optionally provide a source PNG path, and `Pack.Compile` takes care of the rest.
