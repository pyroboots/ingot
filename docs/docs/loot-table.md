# Loot Tables

Loot tables define what items drop when a block is broken, an entity dies, or other loot events fire. In ingot you derive from `LootTable` in `ingot.Core.Behaviour.Loot`, register them on a `BehaviourPack`, and reference them from blocks via the `Loot` shortcut.

## Minimal Loot Table

```csharp
using ingot.Core.Behaviour.Loot;
using ingot.Core.Common;

public class MyBlockLoot : LootTable
{
    public override Identifier Identifier => new("mynamespace:my_block");
    public override LootTableCategory Category => LootTableCategory.Blocks;

    public override LootPool[] Pools =>
    [
        new()
        {
            Rolls = 1,
            Entries =
            [
                new ItemLootEntry(new Identifier("mynamespace:my_block"))
            ]
        }
    ];
}
```

Every loot table **must** implement:

- `Identifier` - used as the output filename (`{name}.json`).
- `Pools` - one or more pools rolled when the loot table is called.

## Key Members

| Member       | Type                 | Default    | Description |
|--------------|----------------------|------------|-------------|
| `Category`   | `LootTableCategory`  | `General`  | Output subfolder under `loot_tables/`. |
| `Reference`  | `string`             | (computed) | Directory path, e.g. `loot_tables/blocks`. |
| `RelativePath` | `string`           | (computed) | Full path written to `minecraft:loot`, e.g. `loot_tables/blocks/my_block.json`. |

### Categories

| `LootTableCategory` | Output directory        |
|---------------------|-------------------------|
| `Blocks`            | `loot_tables/blocks/`   |
| `Items`             | `loot_tables/items/`    |
| `Entities`          | `loot_tables/entities/` |
| `General`           | `loot_tables/`          |

The compiled filename is always `{Identifier.Name}.json` inside that directory.

## Pools

A `LootPool` rolls its entries one or more times:

```csharp
new LootPool
{
    Rolls = new IntRange(1, 3),
    Entries =
    [
        new ItemLootEntry(new Identifier("test:lasagna")) { Weight = 3 },
        new EmptyLootEntry { Weight = 1 }
    ]
}
```

| Member    | Type           | Default | Description |
|-----------|----------------|---------|-------------|
| `Rolls`   | `IntRange`     | `1`     | How many times to roll this pool. |
| `Entries` | `LootEntry[]`  | (required) | Weighted entries to pick from. |

`IntRange` accepts a single integer or a min/max range. A single value compiles as a plain number; a range compiles as `{ "min": ..., "max": ... }`.

## Entries

### Item entry

Drops a specific item:

```csharp
new ItemLootEntry(new Identifier("test:lasagna"))
{
    Weight = 3,
    Functions =
    [
        new SetCountFunction { Count = new IntRange(1, 3) }
    ]
}
```

| Member      | Type              | Default | Description |
|-------------|-------------------|---------|-------------|
| `Weight`    | `int`             | `1`     | Relative chance. Omitted from JSON when `1`. |
| `Functions` | `LootFunction[]`  | `[]`    | Functions applied when this entry is selected. |

### Empty entry

Represents no loot on a roll:

```csharp
new EmptyLootEntry { Weight = 1 }
```

## Functions

### `SetCountFunction`

Sets how many of the dropped item to return:

```csharp
new SetCountFunction { Count = new IntRange(2, 5) }
```

Compiles to:

```json
{
    "function": "set_count",
    "count": { "min": 2, "max": 5 }
}
```

> [!NOTE]
> ingot currently supports `item` and `empty` entry types and the `set_count` function. Pool conditions, tag entries, nested loot tables, and additional functions are not yet modeled in C#.

## Referencing Loot from Blocks

Set the `Loot` shortcut on your block to a `LootTable` instance:

```csharp
using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Loot;
using ingot.Core.Common;

public class DenseLasagnaBlock : Block
{
    public override Identifier Identifier => new("test:block_of_dense_lasagna");
    public override LootTable? Loot => new DenseLasagnaLoot();

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("block_of_dense_lasagna")
    };
}
```

During `Pack.Compile`, ingot:

1. Writes `minecraft:loot` on the block using `Loot.RelativePath`.
2. Auto-registers the loot table type if it is not already in the pack.

You do **not** need a separate `AddLootTable<T>()` call when the block already references the loot table. Explicit registration is still supported for standalone loot tables (e.g. entity drops added later).

Permutations can also override `Loot` for state-specific drops. Permutations that reference loot require a `Parent` block — see [Block Permutations](block-permutations.md).

## Compilation & Registration

```csharp
using ingot.Core;

BehaviourPack bp = BehaviourPack.Create(Guid.NewGuid().ToString())
    .AddItem<LasagnaItem>()
    .AddBlock<DenseLasagnaBlock>();

Pack pack = new()
{
    Name = "My Addon",
    Description = "Loot tables made with ingot",
    BehaviourPack = bp,
    ResourcePack = ResourcePack.Create(Guid.NewGuid().ToString()),
    LinkPacks = true
};

pack.Compile("./output");
```

Or register a loot table explicitly (deduplicated by type):

```csharp
bp.AddLootTable<DenseLasagnaLoot>();
```

This writes `bp/loot_tables/blocks/block_of_dense_lasagna.json`. The block's `minecraft:loot` component points at that same relative path.

A compiled loot table looks like this:

```json
{
    "pools": [
        {
            "rolls": { "min": 1, "max": 2 },
            "entries": [
                {
                    "type": "item",
                    "name": "test:lasagna",
                    "weight": 3,
                    "functions": [
                        {
                            "function": "set_count",
                            "count": { "min": 1, "max": 3 }
                        }
                    ]
                },
                { "type": "empty" }
            ]
        }
    ]
}
```

And the block reference:

```json
"minecraft:loot": "loot_tables/blocks/block_of_dense_lasagna.json"
```

> [!IMPORTANT]
> The `minecraft:loot` path is derived from the **loot table's** `Identifier.Name`, not the block's identifier. Block and loot identifiers often share the same name, but they are independent.

> [!IMPORTANT]
> `Guid.NewGuid().ToString()` is for demonstration purposes. Use a static UUID at runtime for your pack, otherwise Minecraft will treat every rebuild as a completely different pack.

## Full Example

See `DenseLasagnaLoot.cs` and `DenseLasagnaBlock.cs` in the [`ingot.Example`](../../ingot.Example) project. The block's `Loot` property auto-registers the table during compile — `Program.cs` does not call `AddLootTable<DenseLasagnaLoot>()` explicitly.

## Tips

- Use `LootTableCategory.Blocks` for block drops, `Items` for item-related tables, and `Entities` for mob loot.
- `Weight` on entries controls relative probability within a pool. Higher weight = more likely.
- An `EmptyLootEntry` is useful when you want a chance of dropping nothing.
- `AddLootTable<T>()` and block auto-registration deduplicate by loot table **type** — registering the same class twice is safe.
- Loot tables compile after blocks, so auto-registration during block compile still produces the JSON file in the loot compile pass.

Next: see [Making a Block](block.md) for the `Loot` shortcut and [Block Permutations](block-permutations.md) for state-specific loot.