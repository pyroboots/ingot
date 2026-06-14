# Block Material Instances

Material instances define which textures (and how they are rendered) are applied to each face of a block. They map directly to the `minecraft:material_instances` component in the generated block JSON.

## The Two Types

- `MaterialInstance` - configuration for a single face (or the wildcard `*`).
- `MaterialInstances` - the container that assigns `MaterialInstance`s to the six faces plus an `All` (wildcard) entry.

## Basic Usage

The most common pattern is to use `All` (the `*` wildcard) so every face uses the same texture and render settings:

```csharp
public override MaterialInstances MaterialInstances => new()
{
    All = new MaterialInstance("my_block_texture", MaterialInstance.RenderMethods.Opaque)
};
```

You can also specify per-face overrides. Any face not explicitly set falls back to `All` (if present):

```csharp
public override MaterialInstances MaterialInstances => new()
{
    All = new MaterialInstance("stone"),
    Up   = new MaterialInstance("stone_top", MaterialInstance.RenderMethods.Opaque),
    Down = new MaterialInstance("stone_bottom")
};
```

## MaterialInstance Constructor & Properties

```csharp
public MaterialInstance(string texture);
public MaterialInstance(string texture, RenderMethods method);
```

Available properties (all public fields on the struct):

| Property            | Type            | Default          | Description |
|---------------------|-----------------|------------------|-----------|
| `Texture`           | `string`        | (required)       | The texture reference (usually without the `textures/` prefix; matches a texture in your resource pack). |
| `RenderMethod`      | `RenderMethods` | `AlphaTest`      | How the texture is blended/alpha tested. |
| `AmbientOcclusion`  | `float?`        | `null`           | Strength of ambient occlusion on this face. |
| `FaceDimming`       | `bool?`         | `null`           | Whether the face is dimmed when not facing a light source. |
| `Isotropic`         | `bool?`         | `null`           | Whether the texture should be rotated randomly per block (good for grass, etc.). |
| `TintMethod`        | `TintMethods`   | `None`           | Color tinting method (foliage, grass, water, etc.). |

`TintMethod` is only written to JSON when it is not `None`.

### RenderMethods

```csharp
public enum RenderMethods
{
    Opaque,
    DoubleSided,
    Blend,
    AlphaTest,
    AlphaTestSingleSided,
    BlendToOpaque,
    AlphaTestToOpaque,
    AlphaTestSingleSidedToOpaque,
}
```

Common choices:
- `Opaque` - fully solid, no transparency.
- `AlphaTest` - classic cutout (leaves, glass panes, etc.).
- `Blend` - full alpha blending (stained glass, water).

### TintMethods

```csharp
public enum TintMethods
{
    None,
    DefaultFoliage,
    BirchFoliage,
    EvergreenFoliage,
    DryFoliage,
    Grass,
    Water
}
```

Use these for leaves, grass, vines, waterlogged blocks, etc.

## Using Material Instances on Permutations

`BlockPermutation` also has a `MaterialInstances` property (nullable). When set, the permutation completely replaces the base block's material instances while the condition is active:

```csharp
public class GlowingPermutation : BlockPermutation
{
    public override string Condition => "q.get_block_state('mynamespace:lit') == true";

    public override MaterialInstances? MaterialInstances => new()
    {
        All = new MaterialInstance("my_block_lit", MaterialInstance.RenderMethods.Blend)
        {
            AmbientOcclusion = 0.0f,
            Isotropic = false
        }
    };

    public override int? LightEmission => 14;
}
```

## Full Struct Example with All Options

```csharp
All = new MaterialInstance("custom_foliage", MaterialInstance.RenderMethods.AlphaTest)
{
    AmbientOcclusion = 0.75f,
    FaceDimming = true,
    Isotropic = true,
    TintMethod = MaterialInstance.TintMethods.Grass
}
```

## In the Generated JSON

The `MaterialInstances.Compile` method produces output like:

```json
"minecraft:material_instances": {
    "*": {
        "texture": "my_block_texture",
        "render_method": "opaque"
    },
    "up": {
        "texture": "my_block_top",
        "render_method": "alpha_test"
    }
}
```

Face names are lower-cased (`up`, `down`, `north`, `south`, `east`, `west`). The wildcard becomes `"*"`.

## Tips

- Always set at least `All` or all six faces. An empty `MaterialInstances` will still emit the component but with no textures (usually not what you want).
- Texture names are resolved by the resource pack. Make sure your RP actually contains the referenced textures (typically under `textures/blocks/` or wherever your `terrain_texture.json` points).
- Per-face materials are useful for things like logs (bark on sides, cut ends on top/bottom), furnaces, etc.
- Changing material instances on a permutation is a very cheap way to have "lit" vs "unlit" appearances without duplicating the whole block.
- `RenderMethod` names are converted to snake_case (`AlphaTest` → `alpha_test`).

See also: [Making a Block](block.md) and the example `DenseLasagnaBlock`.