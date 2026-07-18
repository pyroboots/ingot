# Block Permutations

Block permutations let a single block definition change its components and traits based on runtime conditions (usually the values of [block states](block.md#block-states)). They are the ingot/C# equivalent of the `permutations` array in vanilla `minecraft:block` JSON.

## Creating a Permutation

Derive from `BlockPermutation`:

```csharp
public class DenseLasagnaGlowyPermutation : BlockPermutation
{
    public override string Condition => "query.block_state('test:radioactive') == true";
    public override Block Parent => new DenseLasagnaBlock();

    public override int? LightEmission => 7;
}
```

Every permutation **must** implement:

- `Condition` - a Molang expression that must evaluate to true for the permutation's components to be applied.
- `Parent` - the owning `Block` instance.

## What You Can Override

A permutation can provide its own versions of the same shortcuts available on `Block`:

- `DisplayName`
- `Friction`
- `LightEmission`
- `LightDampening`
- `Replaceable`
- `Loot` (`LootTable?` - see [Loot Tables](loot-table.md))
- `MaterialInstances` (completely replace the block's materials under this condition)
- `Tags` (block tags applied only when this condition is true)

In addition, any [block trait](trait-system.md) can be implemented directly on the permutation class. The trait components will only be written when the condition matches.

```csharp
using ingot.Core.Behaviour.Block;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Block;

public class GlowyPermutation : BlockPermutation, IGeometry
{
    public override string Condition => "query.block_state('mynamespace:mode') == 2";
    public override Block Parent => new MyBlock();

    public override int? LightEmission => 15;

    public override string[] Tags => ["glow_stone"];

    public override MaterialInstances? MaterialInstances => new()
    {
        All = new MaterialInstance("glowy_variant", MaterialInstance.RenderMethods.Blend, "assets/glowy_variant.png")
    };

    // IGeometry via trait
    bool IGeometry.BoneVisibility => true;
    string IGeometry.Culling => "my_culling";
    Identifier IGeometry.Identifier => new("geometry.my_glowy_block");
    string IGeometry.UvLock => "true";
}
```

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

    public override List<BlockPermutation> Permutations => new()
    {
        new GlowyPermutation(),
        new AnotherSpecialCasePermutation()
    };
}
```

## How Permutations Work in Minecraft

> [!IMPORTANT]
> Permutations are evaluated in order. The **first** permutation whose condition is true "wins" for the properties it defines. Properties not mentioned in a matching permutation fall back to the base block definition.

- You can have as many permutations as you like. A block state with more than 16 possible values throws `ArgumentException` at compile time.

## Condition Examples

```csharp
// Simple boolean state
"query.block_state('test:is_active') == true"

// Integer comparison
"query.block_state('mynamespace:power') >= 3"

// String state
"query.block_state('mynamespace:color') == 'red'"

// Multiple conditions
"query.block_state('test:mode') == 1 && query.block_state('test:powered') == true"
```

> [!TIP]
> Use the exact state names (including namespace) that you declared in the block's `States` dictionary.

## Full Example

The example project contains a complete permutation:

```csharp
public class DenseLasagnaGlowyPermutation : BlockPermutation
{
    public override string Condition => "query.block_state('test:radioactive') == true";
    public override Block Parent => new DenseLasagnaBlock();
    
    public override int? LightEmission => 7;
}
```

See `DenseLasagnaBlock.cs` in the [`ingot.Example`](../../ingot.Example) project (`DenseLasagnaGlowyPermutation` is defined in the same file).

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