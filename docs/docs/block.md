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
| `DisplayName`       | `string?`                   | No       | Shortcut for `minecraft:display_name`. |
| `Friction`          | `float?`                    | No       | Shortcut for `minecraft:friction`. |
| `LightEmission`     | `int?`                      | No       | Shortcut for `minecraft:light_emission` (0-15). |
| `LightDampening`    | `int?`                      | No       | Shortcut for `minecraft:light_dampening`. |
| `Replaceable`       | `bool?`                     | No       | Shortcut for `minecraft:replaceable`. |
| `Loot`              | `string?`                   | No       | Loot table identifier for `minecraft:loot`. |

All of the shortcut properties are written directly into the `components` object of the generated `minecraft:block` JSON.

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
using ingot.Core.Common;

public class DenseLasagnaBlock : Block, 
    IDestructibleByMining, 
    IFlammable, 
    ITick,
    IGeometry
{
    public override Identifier Identifier => new("test:block_of_dense_lasagna");

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("block_of_dense_lasagna", MaterialInstance.RenderMethods.AlphaTest)
    };

    public override Dictionary<string, dynamic[]> States => new()
    {
        { "test:radioactive", [true, false] }
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

    // IGeometry (some abstract)
    bool IGeometry.BoneVisibility => false;
    string IGeometry.Culling => "";
    Identifier IGeometry.Identifier => new("geometry.lasagna_block");  // careful with name clashes!
    string IGeometry.UvLock => "";
}
```

> [!TIP]
> Because some traits will have common property names, its recommended to implement the properties explicitly to be more readable, less ambiguous and it also looks prettier.

## Permutations

Permutations allow different components/traits to apply only when a Molang condition is true. See the dedicated [Block Permutations](block-permutations.md) page.

## Compilation

You rarely call `Block.Compile<T>()` directly. Instead you register blocks with a `BehaviourPack` and supply the visual assets via a `ResourcePack`:

```csharp
using ingot.Core;

BehaviourPack bp = BehaviourPack.Create(Guid.NewGuid().ToString())
    .AddBlock<DenseLasagnaBlock>()
    .AddBlock<AnotherBlock>();

ResourcePack rp = ResourcePack.Create(Guid.NewGuid().ToString())
    .AddBlockTexture("block_of_dense_lasagna", "assets/block_of_dense_lasagna.png");

Pack pack = new()
{
    Name = "My Addon",
    Description = "Blocks made with ingot",
    BehaviourPack = bp,
    ResourcePack = rp,
    LinkPacks = true
};

pack.Compile("./output");
```

This writes the full behaviour pack under `bp/` (including `bp/blocks/test:block_of_dense_lasagna.json`) and the resource pack under `rp/` (including copied textures and the generated `terrain_texture.json` that maps your texture keys).

See the [Resource Packs & Textures](resource-packs.md) guide for details on asset organization, the generated atlas files, and how texture keys bridge behaviour and resources.

> [!NOTE]
> Textures for blocks (and items) are provided on the resource pack side. The strings you return from `MaterialInstances` (and `Item.Texture`) are **keys** that must be registered with `ResourcePack.AddBlockTexture` / `AddItemTexture` so that `pack.Compile` can copy the PNGs and emit the correct `terrain_texture.json` / `item_texture.json`.

## Full Example

See `DenseLasagnaBlock.cs` in the [`ingot.Example`](../ingot.Example) project for a working block that combines states, permutations, and material instances.

## Tips & Gotchas

- Always provide a `MaterialInstances` - it is abstract.
- Block state values are serialized verbatim; make sure your Molang conditions in permutations match the exact values and state names.
- Many traits have a mixture of required (`abstract`) and optional (`virtual`) members - the compiler will happily emit null/empty values for missing abstracts, but you will get warnings.
- Traits are discovered only on the concrete type you pass to `AddBlock<T>`. Inheritance of your own block base classes works as long as the interfaces are implemented somewhere in the hierarchy.
- For complex blocks, prefer many small focused traits over one giant class.

Next: learn about [block permutations](block-permutations.md) and [material instances](block-mat-instances.md).