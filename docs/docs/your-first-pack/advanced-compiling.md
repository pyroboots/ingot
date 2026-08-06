# Advanced Compiling

You already used `Pack.Compile("./output")` in [Step 4](compile.md). This page covers the other compile targets, pack linking, UUID caching, and light extension points.

## Compile Targets

| Method | Output | Good for |
|--------|--------|----------|
| `Compile(outputDir)` | `outputDir/bp/`, `outputDir/rp/` | Inspecting JSON, CI artifacts, manual copy |
| `CompileMcaddon(path)` | A single `.mcaddon` zip | Sharing / double-click import |
| `CompileComMojang(comMojangPath)` | `development_behavior_packs/{Name} BP/` and `development_resource_packs/{Name} RP/` | Local iteration |

```cs
// Importable archive (BP + RP folders inside the zip)
pack.CompileMcaddon("./output/my-addon.mcaddon");

// Straight into Minecraft development folders
pack.CompileComMojang(
    "/path/to/games/com.mojang");
```

MCPelauncher on Linux Flatpak often uses:

```text
~/.var/app/io.mrarm.mcpelauncher/data/mcpelauncher/games/com.mojang
```

> [!WARNING]
> All three methods **delete prior pack output** for that target before writing (existing `bp`/`rp`, development pack folders with the same name, or an existing `.mcaddon`). Cache files (`.ingot`) and `ingot.log` are preserved in the output/cache directory.

### Verbose logging

```cs
pack.Compile("./output", verbose: true);  // default
pack.Compile("./output", verbose: false); // quieter (tests/CI)
```

Full detail always lands in `ingot.log` next to the pack output (or beside the `.mcaddon` / under `com.mojang` depending on the target).

## Behaviour and Resource Pack Linking

`Pack.Create` sets `LinkPacks = true`, which adds cross-dependencies in both manifests so Minecraft loads them together.

```cs
pack.LinkPacks = false; // manage BP/RP activation separately
```

> [!NOTE]
> `CompileMcaddon` does not require `LinkPacks` to be true - the zip still contains both packs.

## UUIDs and the `.ingot` Cache

| Concern | Practice |
|---------|----------|
| Behaviour pack UUID | Pass a fixed string to `Pack.Create` |
| Resource pack UUID | Optional 4th argument; otherwise generated and then cached |
| Rebuild stability | Leave caching on when compiling to the same directory |

```cs
Pack pack = Pack.Create(
    behaviourUuid: "77f1fef2-bb39-411a-b25c-ae475c21169f",
    name: "My Addon",
    description: "Dirt soup pack",
    resourceUuid: "88a2fef2-bb39-411a-b25c-ae475c21169f");
```

```cs
// Opt out of UUID cache when you intentionally change IDs
pack.Compile("./output", cache: false);
```

> [!CAUTION]
> If you use static UUIDs, consider `cache: false` (or deleting `.ingot`) when you intentionally change a pack UUID. A stale cache can keep old IDs in place.

## Pack Icon and Metadata

```cs
pack.PackIcon = Path.Combine(dataDir, "pack_icon.png");
pack.Authors = ["you"];
pack.PackVersion = new ingot.Core.Common.Version(1, 0, 1);
pack.OmitMetadata = false; // default
```

## Instance Registration

Generic `AddBlock<T>()` / `AddItem<T>()` construct types with a parameterless constructor. When you build **configured instances** at runtime (generators, variants), register through the behaviour pack:

```cs
var block = new CompactDirtBlock(/* if you add constructors later */);
pack.BehaviourPack.AddBlockFromInstance(block);
// Same idea: AddItemFromInstance, AddEntityFromInstance, AddRecipeFromInstance, ...
```

See [Compiling Instances](../advanced/trait-system.md#compiling-instances) and the Bricks Galore example for bulk generation.

## Compile Hooks

Run logic before/after a content type is written by implementing `ICompileHooks` and attaching `[CompileHooks]`:

```cs
using ingot.Core.TraitSystem;

public class CompactDirtHooks : ICompileHooks
{
    public void PreCompile(object inst)
    {
        // e.g. CompilerState.Warn("compiling compact dirt");
    }

    public string? PostCompile(string json) => json; // or return modified JSON
}

[CompileHooks(typeof(CompactDirtHooks))]
public class CompactDirtBlock : Block, IDestructibleByMining
{
    // ...
}
```

Full guide: [Compile Hooks](../advanced/compile-hooks.md).

## Putting It Together

A development-oriented entry point might look like:

```cs
using ingot.Core;
using MyAddon.Content.Blocks;
using MyAddon.Content.Entities;
using MyAddon.Content.Items;
using MyAddon.Content.Recipes;

const string packUuid = "77f1fef2-bb39-411a-b25c-ae475c21169f";
string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");

Pack pack = Pack.Create(packUuid, "My Addon", "Dirt soup pack")
    .AddItem<DirtSoupItem>()
    .AddBlock<CompactDirtBlock>()
    .AddEntity<DirtlingEntity>()
    .AddRecipe<DirtSoupRecipe>();

pack.PackIcon = Path.Combine(dataDir, "pack_icon.png");
pack.ScriptsEnabled = true;
pack.AddService(
    Path.Combine(AppContext.BaseDirectory, "scripts", "services", "tick_service.js"),
    intervalTicks: 40);

// Pick one:
// pack.Compile("./output");
// pack.CompileMcaddon("./output/my-addon.mcaddon");
pack.CompileComMojang("/path/to/games/com.mojang");
```

## Where to Go Next

You now have a start-to-finish path: project setup, item, block, entity, compile, scripts, and packaging.

| Topic | Doc |
|-------|-----|
| Trait composition and custom traits | [Trait System](../advanced/trait-system.md) |
| Block states and permutations | [Block Permutations](../block/block-permutations.md) |
| Full item / block / entity references | [Item](../item/item.md), [Block](../block/block.md), [Entity](../entity/entity.md) |
| Recipes and loot | [Recipes](../item/recipe.md), [Loot Tables](../item/loot-table.md) |
| Textures, geometry, particles | [Resource Packs](../resource-packs.md) |
| Working examples in the repo | `ingot.Example`, `ingot.Example.BricksGalore` |

> [!SUCCESS]
> Your first pack is complete. Extend Dirt Soup with more traits, swap the Dirtling to custom geometry, or generate whole material sets the way Bricks Galore does.
