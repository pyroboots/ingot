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

> **Future:** ingot will be published to NuGet once the API stabilizes.

## Create a Project

Create a console application that will act as your pack compiler:

```bash
dotnet new console -n MyAddon
cd MyAddon
# add the ProjectReference to ingot.Core as shown above
```

Your project only needs to **run once** to generate the pack files. Keep a small `Program.cs` that registers all content and calls `Pack.Compile(...)`.

## Define Your First Item

Items inherit from `Item` and must provide an `Identifier` and `Texture`. Behaviour beyond that comes from the [trait system](trait-system.md) - C# interfaces that map to Minecraft `minecraft:*` components.

```csharp
using ingot.Core.Behaviour;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Item;

public class CustomFood : Item, IFood, IUseAnimation
{
    public override Identifier Identifier => new("myaddon", "custom_food");
    public override string Texture => "custom_food";

    public override string DisplayName => "Custom Food";

    int IFood.Nutrition => 4;
    bool IFood.CanAlwaysEat => true;

    string IUseAnimation.Value => "eat";
}
```

Key points:

- `Identifier` uses a `namespace` and `name` (compiled to `namespace:name` in JSON).
- `Texture` is the icon key referenced in `minecraft:icon` and `item_texture.json`.
- Implement trait interfaces (`IFood`, `IDurability`, etc.) and provide their properties via [explicit interface implementation](trait-system.md#implementing-trait-properties).

See [Making an Item](item.md) for the full property reference.

## Define Your First Block

Blocks inherit from `Block` and must provide an `Identifier` and `MaterialInstances` (textures for each face):

```csharp
using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

public class CustomBlock : Block
{
    public override Identifier Identifier => new("myaddon", "custom_block");
    public override string? Geometry => "minecraft:geometry.full_block";
    public override string? ResourceTexture => "custom_block";
    public override string? Sound => "stone";

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("custom_block")
    };

    public override string? DisplayName => "Custom Block";
}
```

See [Making a Block](block.md) for states, permutations, traits, loot tables, and more.

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

> [!WARNING]
> It is recommended that you turn caching off if you use static, pre-generated UUIDs for your packs to avoid using stale caches in the event you need to update the UUID

`CompileMcaddon` stores `.ingot` and `ingot.log` next to the `.mcaddon` file. `CompileComMojang` stores them in the `com.mojang` directory.

All three methods delete prior pack output before compiling. That keeps rebuilds clean when you remove blocks, items, textures, or other content - stale files from an earlier compile are not left behind. Cache files (`.ingot`) and compile logs (`ingot.log`) in the output directory are preserved.

### Pack UUIDs

Use a **fixed behaviour-pack UUID** in real projects. If you generate a new UUID on every build, Minecraft treats each compile as a completely different pack. The `.ingot` cache file preserves UUIDs across rebuilds when you compile to the same output directory.

### Linking Behaviour and Resource Packs

By default, `Pack.Create` sets `LinkPacks = true`, which adds cross-dependencies in both manifests so Minecraft loads them together. Set `pack.LinkPacks = false` if you manage packs separately.

> [!NOTE]
> `Pack.CompileMcaddon` does **not** require `LinkPacks` to be true.

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
3. Create or open a world, go to **Settings → Behavior Packs** and **Resource Packs**, and activate both packs.
4. If content does not appear, check the in-game **Content Log** and your compile log (`ingot.log`) for warnings.

## Enable Script API (Optional)

To use block or item event scripts and tick-based services:

```csharp
pack.ScriptsEnabled = true;
pack.AddService("./scripts/services/tick_service.js"); // optional global tick logic
```

ingot generates custom components, event handler scripts under `bp/scripts/blocks/` and `bp/scripts/items/`, service scripts under `bp/scripts/services/` (wrapped in `system.runInterval` to run every tick), and a `scripts/main.js` entry point. The manifest script module is only added when at least one script exists.

See [Block Events](block-events.md), [Item Events](item-events.md), and [Script Services](script-services.md).

## Example Projects in This Repo

| Project | Purpose |
|---------|---------|
| [`ingot.Tests`](../../ingot.Tests) | xUnit integration and compile tests covering blocks, items, entities, recipes, loot tables, textures, and scripts |
| [`ingot.Example`](../../ingot.Example) | Full example with blocks, items, entities, recipes, loot tables, textures, and scripts; compiles to `./artifacts/example/` |

Build and run the example:

```bash
dotnet run --project ingot.Example
# output: ./artifacts/example/bp/ and ./artifacts/example/rp/
```

## Project Layout (Recommended)

A typical ingot addon solution looks like this:

```
MyAddon/
├── MyAddon.csproj          # references ingot.Core
├── Program.cs              # Pack.Create + Compile
├── Content/
│   ├── Items/
│   ├── Blocks/
│   ├── Recipes/
│   └── Entities/
├── Data/                   # PNG textures (copy to output via .csproj)
├── scripts/                # Script API sources (optional)
│   ├── blocks/             # handler bodies for BlockEvents.FromFile
│   ├── items/              # handler bodies for ItemEvents.FromFile
│   └── services/           # tick handler bodies registered with AddService
└── output/                 # generated bp/ + rp/ (gitignored)
```

Keep identifiers, traits, and cross-references in C# - recipes can reference item classes, blocks can auto-register loot tables, and refactors stay type-safe.

## Next Steps

- [Trait System](trait-system.md) - how behaviours are composed from interfaces
- [Making a Block](block.md) / [Making an Item](item.md) - full content guides
- [Block Events](block-events.md), [Item Events](item-events.md), and [Script Services](script-services.md)
- [Recipes](recipe.md) and [Loot Tables](loot-table.md)
- [Trait System - Creating New Traits](trait-system.md#creating-new-traits) - add custom traits or regenerate from MS docs
- [API Reference](https://pyroboots.github.io/ingot/api/ingot.Core.html)