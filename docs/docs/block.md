# Making a Block

Blocks in ingot are created by deriving from the abstract `Block` class in `ingot.Core.Behaviour.Block`. Your derived class provides an identifier, material configuration, optional block states, permutations, and behavior via the [trait system](trait-system.md).

## Minimal Block

```csharp
using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

public class MyBlock : Block
{
    public override Identifier Identifier => new("mynamespace:my_block");

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("my_block_texture")
    };
}
```

Every block **must** implement:

- `Identifier` - the full `namespace:name` string used in Minecraft.
- `MaterialInstances` - defines the textures/rendering for each face of the block (see [Block Material Instances](block-mat-instances.md)).

## Other Important Members

| Member              | Type                        | Required | Description |
|---------------------|-----------------------------|----------|-----------|
| `FormatVersion`     | `Version`                   | No       | Defaults to `"1.20.10"`. Controls the minimum format the block JSON targets. |
| `States`            | `Dictionary<string, dynamic[]>` | No   | Custom block states (see below). |
| `Permutations`      | `List<BlockPermutation>`    | No       | Conditional variants of the block (see [Block Permutations](block-permutations.md)). |
| `Tags`              | `string[]`                  | No       | Block tags written as empty `tag:<name>` components. Defaults to an empty array. |
| `DisplayName`       | `string?`                   | No       | Shortcut for `minecraft:display_name`. |
| `Friction`          | `float?`                    | No       | Shortcut for `minecraft:friction`. |
| `LightEmission`     | `int?`                      | No       | Shortcut for `minecraft:light_emission` (0-15). |
| `LightDampening`    | `int?`                      | No       | Shortcut for `minecraft:light_dampening`. |
| `Replaceable`       | `bool?`                     | No       | Shortcut for `minecraft:replaceable`. |
| `Loot`              | `LootTable?`                | No       | Loot table reference for `minecraft:loot`. Auto-registers the table during compile. See [Loot Tables](loot-table.md). |

All of the shortcut properties are written directly into the `components` object of the generated `minecraft:block` JSON.

## Block Tags

Override `Tags` to opt into vanilla block tag behaviour (mining speed, tool requirements, etc.):

```csharp
public override string[] Tags =>
[
    "stone",
    "diamond_pick_diggable"
];
```

Each entry is compiled as an empty component keyed `tag:<name>`:

```json
"tag:stone": {},
"tag:diamond_pick_diggable": {}
```

Permutations can also declare their own `Tags` - see [Block Permutations](block-permutations.md).

## Block States

Custom states let you drive permutations and Molang queries. Declare them by overriding `States`:

```csharp
public override Dictionary<string, dynamic[]> States => new()
{
    { "mynamespace:power_level", [0, 1, 2, 3, 4] },
    { "mynamespace:is_active", [true, false] }
};
```

> [!CAUTION]
> Although the state dictionary's value type is `dynamic`, Minecraft will only accept `int`, `float`, `bool` and `string`. Make sure your array is one of those types.

**Important notes**:
- Minecraft limits a state to have **16** possible states. **ingot** will throw a warning for you if a state exceeds that limit.
- State names should be fully qualified (`namespace:state_name`) for best compatibility.

## Adding Behavior with Traits

Most block functionality comes from implementing [traits](trait-system.md):

```csharp
using ingot.Core.Behaviour.Block;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Block;

public class TickableOreBlock : Block, IDestructibleByMining, IFlammable, ITick
{
    public override Identifier Identifier => new("mynamespace:tickable_ore");

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("tickable_ore", MaterialInstance.RenderMethods.Opaque)
    };

    // IDestructibleByMining (abstract property requires implementation)
    dynamic? IDestructibleByMining.ItemSpecificSpeeds => null;
    float IDestructibleByMining.SecondsToDestroy => 1.5f;

    // IFlammable (all virtual, only override what you need)
    int IFlammable.CatchChanceModifier => 15;
    int IFlammable.DestroyChanceModifier => 30;

    // ITick
    int[] ITick.IntervalRange => [20, 40];
    bool ITick.Looping => true;
}
```

> [!TIP]
> Because some traits will have common property names, its recommended to implement the properties explicitly to be more readable, less ambiguous and it also looks prettier.

## Permutations

Permutations allow different components/traits to apply only when a Molang condition is true. See the dedicated [Block Permutations](block-permutations.md) page.

## Compilation

You rarely call `Block.Compile(Type)` directly. Instead register blocks with `Pack.Create` and declare textures on the block class via `MaterialInstance.SourcePath`:

```csharp
using ingot.Core;

Pack pack = Pack.Create(Guid.NewGuid().ToString(), "My Addon", "Blocks made with ingot")
    .AddBlock<DenseLasagnaBlock>()
    .AddBlock<AnotherBlock>();

pack.Compile("./output");
```

```csharp
public override MaterialInstances MaterialInstances => new()
{
    All = new MaterialInstance("block_of_dense_lasagna", MaterialInstance.RenderMethods.AlphaTest, "assets/block_of_dense_lasagna.png")
};
```

Use `pack.AddBlockTexture(key, path)` only when you need a manual override. Capture identifiers from your block class when you need them for cross-references.

This writes the full behaviour pack under `bp/` (including `bp/blocks/block_of_dense_lasagna.json` - the filename is the part after the `:` in the identifier) and the resource pack under `rp/` (including copied textures and the generated `terrain_texture.json` that maps your texture keys).

See the [Resource Packs & Textures](resource-packs.md) guide for details on asset organization, the generated atlas files, and how texture keys bridge behaviour and resources.

## Full Example

See `DenseLasagnaBlock.cs` in the [`ingot.Example`](../../ingot.Example) project for a working block that combines states, permutations, material instances, and a [loot table](loot-table.md).

## Tips & Gotchas

- Always provide a `MaterialInstances` - it is abstract.
- Block state values are serialized verbatim; make sure your Molang conditions in permutations match the exact values and state names.
- Many traits have a mixture of required (`abstract`) and optional (`virtual`) members - the compiler will happily emit null/empty values for missing abstracts, but you will get warnings.
- Traits are discovered only on the concrete type you pass to `AddBlock<T>`. Inheritance of your own block base classes works as long as the interfaces are implemented somewhere in the hierarchy.
- For complex blocks, prefer many small focused traits over one giant class.

Next: learn about [block permutations](block-permutations.md) and [material instances](block-mat-instances.md).