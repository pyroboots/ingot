# Your First Block

In this step, you'll create **Compact Dirt** - a solid custom block with a texture, mining time, sound, and a loot table that can drop Dirt Soup.

## Class Setup

In `Content/Blocks/`, create `CompactDirtBlock.cs` and inherit from `Block` in `ingot.Core.Behaviour.Block`.

Every block **must** provide:

- `Identifier` - the full `namespace:name` used in Minecraft
- `MaterialInstances` - which textures/render methods apply to each face

```cs
using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

namespace MyAddon.Content.Blocks;

public class CompactDirtBlock : Block
{
    public override Identifier Identifier => "myaddon:compact_dirt";

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance(
            "compact_dirt",
            MaterialInstance.RenderMethods.Opaque,
            Path.Combine(AppContext.BaseDirectory, "Data", "compact_dirt.png"))
    };
}
```

`MaterialInstance` takes:

1. A **texture key** written into block materials and `terrain_texture.json`
2. An optional **render method** (`Opaque`, `AlphaTest`, `Blend`, ...)
3. An optional **source PNG path** for auto-registration during compile

> [!TIP]
> Use `All = ...` for the same texture on every face. For per-face textures, set `Up`, `Down`, `North`, and so on instead - see [Block Material Instances](../block/block-mat-instances.md).

## Display Name, Sound, and Category

Override a few convenience properties so the block shows up nicely:

```cs
public override string? DisplayName => "Compact Dirt";
public override string? ResourceTexture => "compact_dirt";
public override string? Sound => "dirt";
public override Enums.CatalogueCategory Category => Enums.CatalogueCategory.Nature;
```

| Property | Effect |
|----------|--------|
| `DisplayName` | `minecraft:display_name` and default lang string |
| `ResourceTexture` | Texture key written to `rp/blocks.json` |
| `Sound` | Break/step/place sound group in `rp/blocks.json` |
| `Category` | Creative inventory tab |

`Geometry` defaults to `minecraft:geometry.full_block`, which is what you want for a normal cube.

## Mining Behaviour with Traits

Add `IDestructibleByMining` so the block takes a moment to break:

```cs
using ingot.Core.TraitSystem.Traits.Block;

public class CompactDirtBlock : Block, IDestructibleByMining
{
    // ...

    dynamic? IDestructibleByMining.ItemSpecificSpeeds => null;
    float IDestructibleByMining.SecondsToDestroy => 1.25f;
}
```

`ItemSpecificSpeeds` is abstract on the trait - return `null` for a flat destroy time, or pass item-specific speed objects when you care about tools. See [Making a Block](../block/block.md).

Optional tags help vanilla tools treat the block correctly:

```cs
public override string[] Tags =>
[
    "minecraft:is_shovel_item_destructible"
];
```

## Loot Table

By default a custom block may not drop anything useful. Define a loot table that drops Dirt Soup, then attach it with the `Loot` shortcut. When a block references a loot table instance, **ingot** auto-registers that table during compile - you do not need a separate `AddLootTable` call.

In `Content/LootTables/CompactDirtLoot.cs` (or next to the block - folder layout is yours):

```cs
using ingot.Core.Behaviour.Loot;
using ingot.Core.Common;

namespace MyAddon.Content.LootTables;

public class CompactDirtLoot : LootTable
{
    public override Identifier Identifier => "myaddon:compact_dirt";
    public override LootTableCategory Category => LootTableCategory.Blocks;

    public override LootPool[] Pools =>
    [
        new()
        {
            Rolls = 1,
            Entries =
            [
                new ItemLootEntry("myaddon:dirt_soup")
                {
                    Weight = 1,
                    Functions = [new SetCount { Count = new IntRange(1, 2) }]
                }
            ]
        }
    ];
}
```

On the block:

```cs
using ingot.Core.Behaviour.Loot;
using MyAddon.Content.LootTables;

public override LootTable? Loot => new CompactDirtLoot();
```

> [!NOTE]
> Loot table paths compile under `loot_tables/blocks/` when `Category` is `Blocks`. Full details are in [Loot Tables](../item/loot-table.md).

## Full Block File

```cs
using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Loot;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Block;
using MyAddon.Content.LootTables;

namespace MyAddon.Content.Blocks;

public class CompactDirtBlock : Block, IDestructibleByMining
{
    public override Identifier Identifier => "myaddon:compact_dirt";
    public override string? DisplayName => "Compact Dirt";
    public override string? ResourceTexture => "compact_dirt";
    public override string? Sound => "dirt";
    public override Enums.CatalogueCategory Category => Enums.CatalogueCategory.Nature;

    public override string[] Tags =>
    [
        "minecraft:is_shovel_item_destructible"
    ];

    public override LootTable? Loot => new CompactDirtLoot();

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance(
            "compact_dirt",
            MaterialInstance.RenderMethods.Opaque,
            Path.Combine(AppContext.BaseDirectory, "Data", "compact_dirt.png"))
    };

    dynamic? IDestructibleByMining.ItemSpecificSpeeds => null;
    float IDestructibleByMining.SecondsToDestroy => 1.25f;
}
```

## Optional: Place Compact Dirt from Dirt Soup

If you want Dirt Soup to place Compact Dirt when used on a block face, add `IBlockPlacer` to the item and raise its `FormatVersion` (that trait requires `1.26.0`):

```cs
// on DirtSoupItem
using Version = ingot.Core.Common.Version;

public override Version FormatVersion => new(1, 26, 0);

// also implement IBlockPlacer:
string IBlockPlacer.Block => "myaddon:compact_dirt";
bool IBlockPlacer.ReplaceBlockItem => false;
```

That is optional for this tutorial - the pack already works with soup as food and compact dirt as a separate block.

## What You Have So Far

| Piece | Type | Identifier |
|-------|------|------------|
| `DirtSoupItem` | Item | `myaddon:dirt_soup` |
| `DirtSoupRecipe` | Recipe | `myaddon:dirt_soup` |
| `CompactDirtBlock` | Block | `myaddon:compact_dirt` |
| `CompactDirtLoot` | Loot table | `myaddon:compact_dirt` |

**Next:** [3. Your First Entity](your-first-entity.md)

**Also see:** [Block Permutations](../block/block-permutations.md), [Block Events](../block/block-events.md)
