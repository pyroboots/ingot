# Getting Started

**ingot** is a C# framework for building Minecraft Bedrock Edition addons. Instead of hand-writing JSON for every block, item, recipe, and loot table, you define your content as strongly-typed C# classes. ingot compiles those classes into a behaviour pack (`bp/`) and resource pack (`rp/`) that Minecraft can load directly.

This guide walks you through setting up a project, defining your first content, compiling a pack, and loading it in-game.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (the repo targets **.NET 10**)
- A Minecraft Bedrock Edition
- A text editor or IDE (Rider, Visual Studio, or VS Code all work well)

## Installation

ingot is distributed as source today. The recommended approach is to add a project reference to `ingot.Core`:

```bash
git clone https://github.com/pyroboots/ingot.git
cd ingot
dotnet build ingot.sln
```

In your own addon project, reference the core library:

```xml
<ItemGroup>
  <ProjectReference Include="path/to/ingot/ingot.Core/ingot.Core.csproj" />
</ItemGroup>
```

> [!NOTE]
> ingot will be published to NuGet once the API stabilizes. Until then, use a project reference to `ingot.Core`.

## Create a Project

Create a console application that will act as your pack compiler:

```bash
dotnet new console -n MyAddon
cd MyAddon
# add the ProjectReference to ingot.Core as shown above
```

> [!TIP]
> Your project only needs to **run once** to generate the pack files. Keep a small `Program.cs` that registers all content and calls `Pack.Compile(...)`.

## Define Your First Item

Items inherit from `Item` and must provide an `Identifier` and `Texture`. Behaviour beyond that comes from the [trait system](advanced/trait-system.md) - C# interfaces that map to Minecraft `minecraft:*` components.

```csharp
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Item;

public class CustomFood : Item, IFood, IUseAnimation, IUseModifiers
{
    public override Identifier Identifier => new("myaddon", "custom_food");
    public override string Texture => "custom_food";

    public override string DisplayName => "Custom Food";

    int IFood.Nutrition => 4;
    bool IFood.CanAlwaysEat => true;

    string IUseAnimation.Value => "eat";
    float IUseModifiers.UseDuration => 1.6f;
}
```

Key points:

- `Identifier` uses a `namespace` and `name` (compiled to `namespace:name` in JSON).
- `Texture` is the icon key referenced in `minecraft:icon` and `item_texture.json`.
- Implement trait interfaces (`IFood`, `IDurability`, etc.) and provide their properties via [explicit interface implementation](advanced/trait-system.md#implementing-trait-properties).

See [Making an Item](item/item.md) for the full property reference.

## Define Your First Block

Blocks inherit from `Block` and must provide an `Identifier` and `MaterialInstances` (textures for each face):

```csharp
using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

public class CustomBlock : Block
{
    public override Identifier Identifier => new("myaddon", "custom_block");
    // Geometry defaults to "minecraft:geometry.full_block"
    public override string? ResourceTexture => "custom_block";
    public override string? Sound => "stone";

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("custom_block")
    };

    public override string? DisplayName => "Custom Block";
}
```

See [Making a Block](block/block.md) for states, permutations, traits, loot tables, creative categories, and more.

## Compile a Pack

Use `Pack.Create` as the single entry point. Register every piece of content, then compile:

```csharp
using ingot.Core;

const string packUuid = "77f1fef2-bb39-411a-b25c-ae475c21169f"; // use a fixed UUID in real projects

Pack pack = Pack.Create(packUuid, "My Addon", "My first ingot pack")
    .AddItem<CustomFood>()
    .AddBlock<CustomBlock>();

pack.Compile("./output");
```

For pre-configured instances (runtime variants, generators), register through the behaviour pack: `pack.BehaviourPack.AddBlockFromInstance(inst)` (and the matching item/entity/recipe/loot helpers). See [Compiling Instances](advanced/trait-system.md#compiling-instances).

> [!IMPORTANT]
> Use a **fixed** behaviour-pack UUID in real projects. Generating a new UUID every build makes Minecraft treat each compile as a different pack.

### Compile targets

`Pack` exposes three compile methods:

| Method | Output |
|--------|--------|
| `Compile(outputDir)` | Deletes any existing `bp/` and `rp/` subfolders, then writes fresh ones under `outputDir` |
| `CompileMcaddon(outputPath)` | Deletes any existing `.mcaddon` file, builds a temporary pack, zips it with `{Name} BP/` and `{Name} RP/` at the archive root, then deletes the temp files |
| `CompileComMojang(comMojangPath)` | Deletes any existing `development_behavior_packs/{Name} BP/` and `development_resource_packs/{Name} RP/` folders, then writes fresh ones under your `com.mojang` folder |

```csharp
// Importable .mcaddon (double-click or open with Minecraft)
pack.CompileMcaddon("./output/my-addon.mcaddon");

// Local development folders (MCPelauncher, Android, Windows, etc.)
pack.CompileComMojang("/path/to/games/com.mojang");
```

`Pack.Compile` writes:

| Output | Contents |
|--------|----------|
| `output/bp/` | Behaviour pack - blocks, items, recipes, loot tables, manifests |
| `output/rp/` | Resource pack - textures, `terrain_texture.json`, `item_texture.json` |
| `output/ingot.log` | Compile-time warnings and info (when `verbose` is `true`, the default) |
| `output/.ingot` | UUID cache so rebuilds keep stable pack IDs |

> [!CAUTION]
> If you use static, pre-generated UUIDs, consider `cache: false` (or deleting `.ingot`) when you intentionally change a pack UUID. A stale cache can keep old UUIDs in place.

`CompileMcaddon` stores `.ingot` and `ingot.log` next to the `.mcaddon` file. `CompileComMojang` stores them in the `com.mojang` directory.

> [!WARNING]
> All three compile methods **delete prior pack output** (`bp/`/`rp/`, development pack folders, or an existing `.mcaddon`) before writing. That keeps rebuilds clean when you remove content, but any hand-edited files inside those folders are wiped. Cache files (`.ingot`) and compile logs (`ingot.log`) in the output directory are preserved.

### Pack UUIDs

The `.ingot` cache file preserves UUIDs across rebuilds when you compile to the same output directory and leave caching enabled.

> [!TIP]
> With `verbose: true` (the default), info lines print to the console during compile and a short summary (warning/info counts, log path, elapsed time) is written at the end. Full detail is always written to `ingot.log` in the output/cache directory. Pass `verbose: false` in tests or automation for quieter compiles.

### Linking Behaviour and Resource Packs

By default, `Pack.Create` sets `LinkPacks = true`, which adds cross-dependencies in both manifests so Minecraft loads them together. Set `pack.LinkPacks = false` if you manage packs separately.

> [!NOTE]
> `Pack.CompileMcaddon` does **not** require `LinkPacks` to be true.

### Engine and format versions

`Pack.MinEngineVersion` defaults to **`1.21.90`**, matching the default `FormatVersion` on `Block` and `Item` (required for Custom Components V2). Override when your content needs a different floor:

```csharp
using Version = ingot.Core.Common.Version;

pack.MinEngineVersion = new Version(1, 21, 0);
```

Some traits also require a higher content `FormatVersion` on the class itself (for example `IBlockPlacer` needs `1.26.0`). See [Trait System - Format version requirements](advanced/trait-system.md#format-version-requirements).

## Add Textures

Textures declared on your content classes are auto-registered during compile:

```csharp
// Item - optional source PNG path (resolved at compile time)
public override string? TexturePath => Path.Combine(AppContext.BaseDirectory, "Data", "custom_food.png");

// Block - source path on the material instance
public override MaterialInstances MaterialInstances => new()
{
    All = new MaterialInstance("custom_block", MaterialInstance.RenderMethods.Opaque,
        Path.Combine(AppContext.BaseDirectory, "Data", "custom_block.png"))
};
```

You can also register textures manually - this is the recommended approach when assets are copied to your build output via the `.csproj`:

```csharp
string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");

pack.AddItemTexture("custom_food", Path.Combine(dataDir, "custom_food.png"))
    .AddBlockTexture("custom_block", Path.Combine(dataDir, "custom_block.png"))
    .AddGeometry("geometry.custom_block", Path.Combine(dataDir, "custom_block.geo.json"));
```

Copy assets into the output directory from your project file:

```xml
<ItemGroup>
  <None Include="Data\**\*" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

See [Resource Packs & Textures](resource-packs.md) for the full texture pipeline.

## Load the Pack in Minecraft

1. Run your project (`dotnet run`) to compile the pack.
2. Load it using one of these methods:
   - **`CompileMcaddon`** - import the generated `.mcaddon` file (behaviour and resource packs are bundled with the correct zip layout).
   - **`CompileComMojang`** - compile straight into `development_behavior_packs/` and `development_resource_packs/` under your `com.mojang` folder (for example MCPelauncher on Linux: `~/.var/app/io.mrarm.mcpelauncher/data/mcpelauncher/games/com.mojang`).
   - **`Compile`** - copy `output/bp/` and `output/rp/` into the development pack folders manually, or zip each folder as a `.mcpack`.
3. Create or open a world, go to **Settings > Behavior Packs** and **Resource Packs**, and activate both packs.
4. If content does not appear, check the in-game **Content Log** and your compile log (`ingot.log`) for warnings.

## Enable Script API (Optional)

To use block or item event scripts and tick-based services:

```csharp
pack.ScriptsEnabled = true;
pack.AddService("./scripts/services/tick_service.js"); // optional global tick logic
```

> [!IMPORTANT]
> Set `ScriptsEnabled = true` before compiling if you define block/item event handlers or services. Otherwise scripts are skipped and compile-time warnings are emitted.

ingot generates custom components, event handler scripts under `bp/scripts/blocks/` and `bp/scripts/items/`, service scripts under `bp/scripts/services/` (wrapped in `system.runInterval` to run every tick), and a `scripts/main.js` entry point. The manifest script module is only added when at least one script exists.

See [Block Events](block/block-events.md), [Item Events](item/item-events.md), and [Script Services](script-services.md).

## Example Projects in This Repo

| Project | Purpose |
|---------|---------|
| [`ingot.Tests`](https://github.com/pyroboots/ingot/tree/master/ingot.Tests) | xUnit integration and compile tests covering blocks, items, entities, recipes, loot tables, textures, and scripts |
| [`ingot.Example`](https://github.com/pyroboots/ingot/tree/master/ingot.Example) | Full example with blocks, items, entities, recipes, loot tables, textures, and scripts |
| [`ingot.Example.BricksGalore`](https://github.com/pyroboots/ingot/tree/master/ingot.Example.BricksGalore) | Large procedural brick pack (materials x patterns x optional inlays) |

Build and run the lasagna example:

```bash
dotnet run --project ingot.Example
# by default the example uses CompileComMojang; switch to Compile("./artifacts/example/") for folder output
```

### Bricks Galore

[`ingot.Example.BricksGalore`](https://github.com/pyroboots/ingot/tree/master/ingot.Example.BricksGalore) shows how to use ingot at **scale** when hand-writing one class per block is impractical. Instead of defining each brick by hand, it:

1. Registers **materials** (palette + stats + craft ingredient) and **patterns** (base texture + optional mortar/inlay overlay + craft catalyst) in `Program.BuildContent()`.
2. **Recolours** greyscale templates with GIMP-style `.gpl` palettes and composites body + overlay textures with SkiaSharp.
3. Builds configured `BrickBlock` / `BrickRecipe` **instances** (one per material x pattern x optional overlay combo) and registers them with `BehaviourPack.AddBlockFromInstance` / `AddRecipeFromInstance`.
4. Adds shapeless crafting (body + catalyst + stone, plus inlay upgrade recipes), MC functions to place/clear a gallery, and a tick service that shows material lore on the action bar.

Current content is roughly **8 materials** (amethyst, copper, diamond, emerald, gold, lapis, netherite, resin) x **20 patterns** (bricks, chiseled, tiles) - hundreds of blocks once same-colour and cross-material inlays are included.

```bash
dotnet run --project ingot.Example.BricksGalore
```

To extend the pack, edit only the registration block in `Program.cs`:

- New material: drop `Palettes/{id}.gpl`, then `reg.AddMaterial("id", ingredient: "minecraft:...", ...)`.
- New pattern: add a base PNG under `Textures/{Bricks|Chiseled|Tiles}/`, optional overlay under `Textures/Overlays/`, then `reg.AddPattern("id", "Folder/name", "minecraft:catalyst")`.

> [!TIP]
> This is the best reference for **instance-based registration**, bulk texture generation, or hundreds of nearly-identical blocks without copy-pasting C# classes. See [Compiling Instances](advanced/trait-system.md#compiling-instances).

## Project Layout (Recommended)

A typical ingot addon solution looks like this:

```
MyAddon/
  MyAddon.csproj          # references ingot.Core
  Program.cs              # Pack.Create + Compile
  Content/
    Items/
    Blocks/
    Recipes/
    Entities/
  Data/                   # PNG textures (copy to output via .csproj)
  scripts/                # Script API sources (optional)
    blocks/               # handler bodies for BlockEvents.FromFile
    items/                # handler bodies for ItemEvents.FromFile
    services/             # tick handler bodies registered with AddService
  output/                 # generated bp/ + rp/ (gitignored)
```

Keep identifiers, traits, and cross-references in C# - recipes can reference item classes, blocks can auto-register loot tables, and refactors stay type-safe.

## Next Steps

- [Trait System](advanced/trait-system.md) - how behaviours are composed from interfaces
- [Making a Block](block/block.md) / [Making an Item](item/item.md) - full content guides
- [Block Events](block/block-events.md), [Item Events](item/item-events.md), and [Script Services](script-services.md)
- [Recipes](item/recipe.md) and [Loot Tables](item/loot-table.md)
- [Trait System - Creating New Traits](advanced/trait-system.md#creating-new-traits) - add custom traits or regenerate from MS docs
- [API Reference](https://pyroboots.github.io/ingot/api/ingot.Core.html)