![ingot Logo](https://raw.githubusercontent.com/pyroboots/ingot/master/ingot.png)

# ingot

[![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**A C# framework to programmatically create Minecraft Bedrock Edition packs** - because writing mountains of JSON sucks.

[API Reference](https://pyroboots.github.io/ingot/api/ingot.Core.html) | [Project Todo List](https://github.com/users/pyroboots/projects/4)

## Features

- **Inheritance-Based, Type-Safe API** - Define items, blocks, entities, and more using clean, strongly-typed C# instead of hand-writing JSON
- **Automatic Manifest Generation** - `manifest.json` files for both behavior packs and resource packs are created automatically
- **Full BP + RP Support** - Create behavior packs, resource packs, and linked packs with ease (including client entities, render controllers, and entity textures)
- **Script API Ready** - File-based or inline [block](docs/docs/block/block-events.md) and [item event](docs/docs/item/item-events.md) handlers, [services](docs/docs/script-services.md) auto-wrapped to run every tick, and [`/scriptevent`](docs/docs/script-services.md#script-events) handlers, with compile-time trait validation
- **CompilerState** - Get clear, helpful compile-time feedback to avoid debugging in the Minecraft content log
- **Extensible & Maintainable** - Designed for large or complex addons where manual JSON becomes painful
- **Iterable & Reusable** - Because its code, you can define variable and easily change things project-wide

## Installation

**ingot** is currently distributed via source. The easiest way to get started is:

### Option 1: Add as a Project Reference (Recommended for now)
1. Clone the repository:
   ```bash
   git clone https://github.com/pyroboots/ingot.git
   ```
2. Add a reference to `ingot.Core` in your .NET project.

### Option 2: Build from Source
```bash
dotnet build ingot.sln
```

> **Future**: **ingot** will be published to NuGet once it reaches a stable API.

## Quick Start

```csharp
using ingot.Core;
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.Common.SharedConstructs;
using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits.Item;

using Version = ingot.Core.Common.Version;

// inherit traits to add behaviour
public class LasagnaItem : Item, IFood, IBlockPlacer, IUseAnimation, IUseModifiers
{
    // IBlockPlacer requires format_version >= 1.26.0 (see TraitFormatVersion)
    public override Version FormatVersion => new(1, 26, 0);
    public override Identifier Identifier => new("test:lasagna");
    public override string Texture => "lasagna";
    public override string DisplayName => "Lasagna";

    // food requires use_modifiers (use duration) and usually an eat animation
    int IFood.Nutrition => 5;
    float IFood.SaturationModifier => 0.9f;
    ItemTypeDescriptor? IFood.UsingConvertsTo => "minecraft:bowl";

    string IUseAnimation.Value => "eat";
    float IUseModifiers.UseDuration => 1.6f;
    float IUseModifiers.MovementModifier => 0.35f;
    string IUseModifiers.StartUsing => IUseModifiers.StartUsing_Always;
    [IngotExclude]
    string IUseModifiers.StartSound => null!;

    BlockTypeDescriptor IBlockPlacer.Block => "test:block_of_dense_lasagna";
    bool IBlockPlacer.ReplaceBlockItem => true;
}

class Program
{
    static void Main(string[] args)
    {
        const string bpUuid = "a8f3c2e1-4b5d-6e7f-8091-a2b3c4d5e6f7";
        const string rpUuid = "b9e4d3c2-5a6b-7c8d-9e0f-b1c2d3e4f5a6";
        string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");

        Pack pack = Pack.Create(bpUuid, "ingot example", "Example pack made with ingot", rpUuid)
            .AddItem<LasagnaItem>()
            .AddBlock<DenseLasagnaBlock>();

        pack.ScriptsEnabled = true;
        pack.AddService(Path.Combine(AppContext.BaseDirectory, "scripts", "services", "tick_service.js"));
        pack.AddBlockTexture("block_of_dense_lasagna", Path.Combine(dataDir, "dense_lasagna.png"))
            .AddItemTexture("lasagna", Path.Combine(dataDir, "lasagna.png"));

        pack.Compile("./artifacts/example/");
        // or: pack.CompileMcaddon("./artifacts/example/ingot example.mcaddon");
        // or: pack.CompileComMojang("/path/to/games/com.mojang");
    }
}
```

See the [`ingot.Example`](./ingot.Example) project for a more complete working example that includes blocks, items, entities (behaviour + client entity), recipes, textures, scripts, and the full resource pack side. A larger procedural sample lives in [`ingot.Example.BricksGalore`](./ingot.Example.BricksGalore). The docs use `./output` as a generic compile path; this repo's lasagna example compiles to `./artifacts/example/` (or directly into `com.mojang` via `CompileComMojang`). See the [Resource Packs & Textures](docs/docs/resource-packs.md), [Making an Entity](docs/docs/entity/entity.md), [Client Entities](docs/docs/entity/client-entity.md), [Block Events](docs/docs/block/block-events.md), [Item Events](docs/docs/item/item-events.md), [Script Services and Events](docs/docs/script-services.md), and [Recipes](docs/docs/item/recipe.md) guides for more detail.

### Bricks Galore (large procedural example)

[`ingot.Example.BricksGalore`](./ingot.Example.BricksGalore) generates a full decorative brick pack from data rather than hand-written classes: materials (`.gpl` palettes + stats) x patterns (base + optional mortar/inlay overlays) become composite textures, configured `Block`/`Recipe` instances registered via `Add*FromInstance`, crafting, gallery functions, and a lore service. Edit `Program.BuildContent()` to add materials or patterns; see [Getting Started - Bricks Galore](docs/docs/getting-started.md#bricks-galore) for a short walkthrough.

```bash
dotnet run --project ingot.Example.BricksGalore
```

## Project Structure

| Folder | Purpose |
|--------|---------|
| `ingot.Core` | Core API |
| `ingot.Example` | Full small example (lasagna content, cow entity, scripts) |
| `ingot.Example.BricksGalore` | Large procedural brick pack (materials x patterns x inlays) |
| `ingot.Generators` | Schema-driven trait generation (`TraitGeneratorV2` from bedrock-samples JSON schemas); see [Creating New Traits](docs/docs/advanced/trait-system.md#creating-new-traits) |
| `ingot.Tests` | xUnit integration and compile tests |

**Made with love for the Minecraft Bedrock addon community.**
