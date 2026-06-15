# Resource Packs & Textures

ingot builds **both** a behaviour pack (`bp/`) and a resource pack (`rp/`) from a single `Pack` object. 

- Behaviour definitions (`Block`, `Item`, traits, etc.) describe *what* your content is and how it behaves.
- Resource assets describe how it *looks* (and eventually sounds, particles, etc.).

For blocks and items, the primary resource concern today is **textures**.

## The ResourcePack Class

Create a `ResourcePack` the same way you create a `BehaviourPack`:

```csharp
using ingot.Core;

BehaviourPack bp = BehaviourPack.Create(Guid.NewGuid().ToString())
    .AddBlock<MyBlock>();

ResourcePack rp = ResourcePack.Create(Guid.NewGuid().ToString())
    .AddBlockTexture("block_of_dense_lasagna", "assets/block_of_dense_lasagna.png")
    .AddItemTexture("lasagna", "assets/lasagna.png");

Pack pack = new()
{
    Name = "My Addon",
    Description = "...",
    BehaviourPack = bp,
    ResourcePack = rp,
    LinkPacks = true
};

pack.Compile("./output");
```

Key members:

- `Create(string uuid, Version? version = null)` - factory (recommended).
- `AddBlockTexture(string key, string sourcePngPath)` - registers a texture that will be copied to `textures/blocks/` and referenced from `terrain_texture.json`.
- `AddItemTexture(string key, string sourcePngPath)` - registers an icon that will be copied to `textures/items/` and referenced from `item_texture.json`.
- `Compile(string dir)` - normally called for you by `Pack.Compile`.

Both `Add*` methods are fluent and return the `ResourcePack` so you can chain calls. The `key` must exactly match the texture string you use on the behaviour side.

## Texture Keys - the Bridge Between Behaviour and Resources

The strings you write in behaviour code are **keys**, not file paths:

- `new MaterialInstance("block_of_dense_lasagna", ...)` in a `Block` or `BlockPermutation`
- `public override string Texture => "lasagna";` on an `Item`
- `string IDestructionParticles.Texture => "my_particles";` (via trait)

These keys only become real images when you register them on the `ResourcePack` using the matching `AddBlockTexture` / `AddItemTexture` call.

If a key used in behaviour is never registered on the resource pack, the generated atlas files will be missing that entry and you will see missing (purple/black) textures in Minecraft. ingot will emit a compile-time warning if the source PNG for a registered key cannot be found.

## What Gets Generated in the Resource Pack

When `Pack.Compile` runs, the resource side produces (among other folders):

- `rp/textures/blocks/<key>.png` - your block textures (from `AddBlockTexture`)
- `rp/textures/items/<key>.png` - your item icons (from `AddItemTexture`)
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

Then register with relative paths:

```csharp
.AddBlockTexture("block_of_dense_lasagna", "assets/block_of_dense_lasagna.png")
```

`ResourcePack` resolves the paths with `Path.GetFullPath` at registration time, so the paths work regardless of the current working directory when you later run the compiler.

You can share the same PNG file under different keys or even register the same key for both a block and an item if that makes sense for your design.

## Putting It All Together

Here is a minimal complete example (the `ingot.Example` project contains a richer version using the lasagna content):

```csharp
using ingot.Core;

BehaviourPack bp = BehaviourPack.Create(Guid.NewGuid().ToString())
    .AddBlock<MyBlock>()
    .AddItem<MyItem>();

ResourcePack rp = ResourcePack.Create(Guid.NewGuid().ToString())
    .AddBlockTexture("my_block", "assets/my_block.png")
    .AddItemTexture("my_item", "assets/my_item.png");

Pack pack = new Pack
{
    Name = "My Addon",
    Description = "Example with resources",
    BehaviourPack = bp,
    ResourcePack = rp,
    LinkPacks = true
};

pack.Compile("./output");
```

After compilation you will have a ready-to-use `bp/` folder and `rp/` folder (plus `manifest.json` files that cross-link them when `LinkPacks` is true).

See the [`ingot.Example`](../ingot.Example) project for a working end-to-end sample that includes states, permutations, traits, and both block and item textures.

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
- [Items](item.md)
- [Block Permutations](block-permutations.md)
- API reference for `ResourcePack`

The texture key system is the main integration point between the behaviour and resource halves of your pack. Register once on the `ResourcePack`, reference by the same string from your `Block`/`Item` definitions, and `Pack.Compile` takes care of the rest.
