# Compile Your Pack

Content classes do nothing until you register them on a `Pack` and call a compile method. This step wires up `Program.cs`, textures, and a first successful build.

## Textures

Place PNG icons in `Data/` (already copied to the build output by your `.csproj`):

```
Data/
  dirt_soup.png      # item icon (16x16 or 32x32 is fine)
  compact_dirt.png   # block texture
```

If a class already sets `TexturePath` / `MaterialInstance` source paths under `Data/`, auto-registration is enough. You can still register manually for clarity or overrides.

> [!TIP]
> Placeholder art is fine while learning. Solid-colour PNGs will still show up in-game.

## Program.cs

Replace the template `Program.cs` with something like this:

```cs
using ingot.Core;
using MyAddon.Content.Blocks;
using MyAddon.Content.Entities;
using MyAddon.Content.Items;
using MyAddon.Content.Recipes;

// Fixed UUIDs - do not generate a new one every build
const string packUuid = "77f1fef2-bb39-411a-b25c-ae475c21169f";

string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");

Pack pack = Pack.Create(
        packUuid,
        "My Addon",
        "Dirt soup, compact dirt, and a dirtling - made with ingot")
    .AddItem<DirtSoupItem>()
    .AddBlock<CompactDirtBlock>()
    .AddEntity<DirtlingEntity>()
    .AddRecipe<DirtSoupRecipe>();

// Optional manual texture registration (overrides auto paths for the same key)
pack.AddItemTexture("dirt_soup", Path.Combine(dataDir, "dirt_soup.png"))
    .AddBlockTexture("compact_dirt", Path.Combine(dataDir, "compact_dirt.png"));

// Writes output/bp and output/rp (deletes prior pack folders under output first)
pack.Compile("./output");
```

### What each call does

| Call | Effect |
|------|--------|
| `Pack.Create` | Creates linked behaviour + resource packs (`LinkPacks = true` by default) |
| `AddItem` / `AddBlock` / `AddEntity` / `AddRecipe` | Registers content types for compile |
| `AddEntity` | Also discovers nested/matching `ClientEntity` when `discoverClient` is true (default) |
| `AddItemTexture` / `AddBlockTexture` | Manual RP texture entries |
| `Compile` | Writes `bp/`, `rp/`, `ingot.log`, and `.ingot` cache under the output directory |

> [!IMPORTANT]
> Use a **fixed** behaviour-pack UUID. A new UUID every build makes Minecraft treat each compile as a different pack. Generate one once (any UUID v4) and keep it in source control.

Compact Dirt's loot table is registered automatically because the block returns it from `Loot`. You do not need `.AddLootTable<CompactDirtLoot>()` unless you also want the table without the block.

## Build and Run

```bash
dotnet run
```

You should see console lines while content compiles (verbose mode is on by default), then a short summary pointing at `ingot.log`.

### Output layout

| Path | Contents |
|------|----------|
| `output/bp/` | Behaviour pack - items, blocks, entities, recipes, loot, manifests |
| `output/rp/` | Resource pack - textures, `item_texture.json`, `terrain_texture.json`, client entities |
| `output/ingot.log` | Compile warnings and info |
| `output/.ingot` | UUID cache for stable rebuilds |

> [!WARNING]
> `Compile` **deletes** existing `bp/` and `rp/` under the output directory before writing. Do not hand-edit files inside those folders if you expect them to survive the next run.

## Load the Pack in Minecraft

1. Run `dotnet run` so `output/bp` and `output/rp` exist.
2. Copy both folders into your Minecraft development pack directories, **or** zip each as a `.mcpack`, **or** use [Advanced Compiling](advanced-compiling.md) helpers (`CompileMcaddon` / `CompileComMojang`).
3. Create or open a world, enable both the behaviour pack and the resource pack.
4. In creative inventory, look for **Dirt Soup**, **Compact Dirt**, and the **Dirtling** spawn egg.
5. Craft soup with a bowl + dirt; `/summon myaddon:dirtling` should work if commands are enabled.

Typical development pack locations (examples):

| Platform | Behaviour packs | Resource packs |
|----------|-----------------|----------------|
| Windows Store / launcher | `%localappdata%\Packages\Microsoft.MinecraftUWP_...\LocalState\games\com.mojang\development_behavior_packs` | `...\development_resource_packs` |
| MCPelauncher (Linux Flatpak) | `~/.var/app/io.mrarm.mcpelauncher/data/mcpelauncher/games/com.mojang/development_behavior_packs` | `.../development_resource_packs` |

If content is missing, check:

1. In-game **Content Log**
2. `output/ingot.log`
3. That both BP and RP are active on the world
4. That texture PNGs exist at the paths you registered

## Minimum Engine Version

`Pack.MinEngineVersion` defaults to **1.21.90** (aligned with Custom Components V2 on blocks/items). Override only if you need a different floor:

```cs
using Version = ingot.Core.Common.Version;

pack.MinEngineVersion = new Version(1, 21, 0);
```

## Checkpoint

At this point your pack should compile and load with:

- Craftable, edible Dirt Soup
- Placeable Compact Dirt that drops soup
- Summonable Dirtling with a spawn egg and slime-like visuals

**Next:** [5. Adding Scripts](adding-scripts.md) for Script API handlers, or [6. Advanced Compiling](advanced-compiling.md) for `.mcaddon` and direct `com.mojang` output.
