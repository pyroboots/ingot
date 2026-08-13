# Block Permutations

Block permutations let a single block definition change its components and traits based on runtime conditions (usually the values of [block states](block.md#block-states)). They are the ingot/C# equivalent of the `permutations` array in vanilla `minecraft:block` JSON.

## Creating a Permutation

Derive from `BlockPermutation`:

```csharp
public class DenseLasagnaGlowyPermutation : BlockPermutation
{
    public override Molang Condition => new Molang().BlockState("test:radioactive").Eq(true);
    public override Block Parent => new DenseLasagnaBlock();

    public override int? LightEmission => 7;
}
```

Every permutation **must** implement:

- `Condition` - a `Molang` expression (`ingot.Core`) that must evaluate to true for the permutation's components to be applied.
- `Parent` - the owning `Block` instance.

There is no implicit conversion from `string` to `Molang`. Build the expression with the `Molang` fluent API, or pass a raw string through `new Molang().Raw("query.block_state('test:radioactive') == true")`.

## What You Can Override

A permutation can provide its own versions of the same shortcuts available on `Block`:

- `DisplayName`
- `Friction`
- `LightEmission`
- `LightDampening`
- `Replaceable`
- `Loot` (`LootTable?` - see [Loot Tables](../item/loot-table.md))
- `MaterialInstances` (completely replace the block's materials under this condition)
- `Tags` (block tags applied only when this condition is true)
- `DynamicTraits` (hand-built `Trait` components for this permutation only; see [Dynamic Traits](../advanced/trait-system.md#dynamic-traits))
- `Singles` (scalar single-value components for this permutation only; see [Singles](../advanced/trait-system.md#singles))

In addition, any [block trait](../advanced/trait-system.md) can be implemented directly on the permutation class. The trait components will only be written when the condition matches.

```csharp
using ingot.Core;
using ingot.Core.Behaviour.Block;
using ingot.Core.Common;
using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits.Block;

public class GlowyPermutation : BlockPermutation, IGeometry
{
    public override Molang Condition => new Molang().BlockState("mynamespace:mode").Eq(2);
    public override Block Parent => new MyBlock();

    public override int? LightEmission => 15;

    public override string[] Tags => ["glow_stone"];

    public override MaterialInstances? MaterialInstances => new()
    {
        All = new MaterialInstance("glowy_variant", MaterialInstance.RenderMethods.Blend, "assets/glowy_variant.png")
    };

    // IGeometry abstracts must be implemented (or excluded)
    string IGeometry.Identifier => "geometry.my_glowy_block";
    [IngotExclude]
    string IGeometry.Culling => null!;
    [IngotExclude]
    string IGeometry.CullingLayer => null!;
    [IngotExclude]
    string IGeometry.CullingShape => null!;
    [IngotExclude]
    dynamic IGeometry.NWayVisualRotation => null!;
}
```

Trait format-version checks on a permutation use `Parent.FormatVersion`. Raise the parent block to `1.26.20` (or higher) when the permutation implements a regenerated block trait such as `IGeometry`.

Register the geometry file in `Program.cs` so it is copied into the resource pack:

```csharp
pack.AddGeometry("geometry.my_glowy_block", Path.Combine(dataDir, "my_glowy_block.geo.json"));
```

Tags on a permutation compile the same way as on a base block - each entry becomes an empty `tag:<name>` component inside the permutation's `components` object.

## Registering Permutations

Return them from the `Permutations` property on your block:

```csharp
using ingot.Core.Common;

public class MyBlock : Block
{
    public override Identifier Identifier => new("mynamespace:my_block");

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("my_block")
    };

    public override BlockPermutation[] Permutations =>
    [
        new GlowyPermutation(),
        new AnotherSpecialCasePermutation()
    ];
}
```

## How Permutations Work in Minecraft

> [!IMPORTANT]
> Permutations are evaluated in order. The **first** permutation whose condition is true "wins" for the properties it defines. Properties not mentioned in a matching permutation fall back to the base block definition.

- You can have as many permutations as you like. A block state with more than 16 possible values throws `ArgumentException` at compile time.

## Condition Examples

`Condition` is a `Molang` builder. Chain queries and operators; `ToString()` is what lands in JSON.

```csharp
// Simple boolean state
new Molang().BlockState("test:is_active").Eq(true)
// query.block_state('test:is_active') == true

// Integer comparison
new Molang().BlockState("mynamespace:power").GtEq(3)
// query.block_state('mynamespace:power') >= 3

// String state
new Molang().BlockState("mynamespace:color").Eq("red")
// query.block_state('mynamespace:color') == 'red'

// Multiple conditions
new Molang().BlockState("test:mode").Eq(1).And().BlockState("test:powered").Eq(true)
// query.block_state('test:mode') == 1 && query.block_state('test:powered') == true

// Raw string when you already have the expression
new Molang().Raw("query.block_state('test:is_active') == true")
```

`Molang` also exposes the other Bedrock queries (`HasBlockState`, `AllTags`, ...). Use `Raw(...)` for anything the builder does not cover.

> [!TIP]
> Use the exact state names (including namespace) that you declared in the block's `States` dictionary.

## Full Example

The example project contains a complete permutation:

```csharp
public class DenseLasagnaGlowyPermutation : BlockPermutation
{
    public override Molang Condition => new Molang().BlockState("test:radioactive").Eq(true);
    public override Block Parent => new DenseLasagnaBlock();
    
    public override int? LightEmission => 7;
}
```

See `DenseLasagnaBlock.cs` in the [`ingot.Example`](https://github.com/pyroboots/ingot/tree/master/ingot.Example) project (`DenseLasagnaGlowyPermutation` is defined in the same file).

## Tips

> [!TIP]
> Keep permutations focused. A permutation should only contain the deltas (light level, different geometry, extra destruction particles, etc.).

> [!CAUTION]
> Material instances defined on a permutation **completely replace** the base block's `minecraft:material_instances` for that condition.

- You can implement traits on permutations that the base block does **not** implement.
- Tags on a permutation only apply while the condition is true - useful for state-dependent tool requirements or mining behaviour.
- If you need different loot tables only under certain states, override `Loot` on the permutation with a different `LootTable` instance. The `Parent` property must return the owning block.

> [!NOTE]
> The base block's components are still present; permutations are additive/override, not a full replacement of the block definition.