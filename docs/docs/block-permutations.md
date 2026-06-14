# Block Permutations

Block permutations let a single block definition change its components and traits based on runtime conditions (usually the values of [block states](block.md#block-states)). They are the ingot/C# equivalent of the `permutations` array in vanilla `minecraft:block` JSON.

## Creating a Permutation

Derive from `BlockPermutation`:

```csharp
public class DenseLasagnaGlowyPermutation : BlockPermutation
{
    public override string Condition => "q.get_block_state('test:radioactive') == true";

    public override int? LightEmission => 7;
}
```

The only **required** member is `Condition` - a Molang expression that must evaluate to true for the permutation's components to be applied.

## What You Can Override

A permutation can provide its own versions of the same shortcuts available on `Block`:

- `DisplayName`
- `Friction`
- `LightEmission`
- `LightDampening`
- `Replaceable`
- `Loot`
- `MaterialInstances` (completely replace the block's materials under this condition)

In addition, any [block trait](trait-system.md) can be implemented directly on the permutation class. The trait components will only be written when the condition matches.

```csharp
public class GlowyPermutation : BlockPermutation, IGeometry
{
    public override string Condition => "q.get_block_state('mynamespace:mode') == 2";

    public override int? LightEmission => 15;

    public override MaterialInstances? MaterialInstances => new()
    {
        All = new MaterialInstance("glowy_variant", MaterialInstance.RenderMethods.Blend)
    };

    // IGeometry via trait
    bool IGeometry.BoneVisibility => true;
    string IGeometry.Culling => "my_culling";
    string IGeometry.Identifier => "geometry.my_glowy_block";
    string IGeometry.UvLock => "true";
}
```

## Registering Permutations

Return them from the `Permutations` property on your block:

```csharp
public class MyBlock : Block
{
    public override string Identifier => "mynamespace:my_block";

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

- Permutations are evaluated in the order they appear in the JSON.
- The first permutation whose condition is true "wins" for the properties it defines.
- Properties not mentioned in a matching permutation fall back to the base block definition.
- You can have as many permutations as you like; the compiler will warn you if state combinations exceed 16.

## Condition Examples

```csharp
// Simple boolean state
"q.get_block_state('test:is_active') == true"

// Integer comparison
"q.get_block_state('mynamespace:power') >= 3"

// String state
"q.get_block_state('mynamespace:color') == 'red'"

// Multiple conditions
"q.get_block_state('test:mode') == 1 && q.get_block_state('test:powered') == true"
```

Use the exact state names (including namespace) that you declared in the block's `States` dictionary.

## Full Example

The example project contains a complete permutation:

```csharp
public class DenseLasagnaGlowyPermutation : BlockPermutation
{
    public override string Condition => "q.get_block_state('test:radioactive') == true";
    
    public override int? LightEmission => 7;
}
```

See `DenseLasagnaBlock.cs` in the [`ingot.Example`](../ingot.Example) project.

## Tips

- Keep permutations focused. A permutation should only contain the deltas (light level, different geometry, extra destruction particles, etc.).
- You can implement traits on permutations that the base block does **not** implement.
- Material instances defined on a permutation completely replace the base block's `minecraft:material_instances` for that condition.
- If you need different loot tables or different destructible values only under certain states, put those on the permutation via shortcuts or traits.
- Remember that the base block's components are still present; permutations are additive/override, not a full replacement of the block definition.