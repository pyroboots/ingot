# Making a Block

Blocks in ingot are created by deriving from the abstract `Block` class in `ingot.Core.Behaviour.Block`. Your derived class provides an identifier, material configuration, optional block states, permutations, and behavior via the [trait system](../advanced/trait-system.md).

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

| Member              | Type                        | Default / Required | Description |
|---------------------|-----------------------------|--------------------|-----------|
| `FormatVersion`     | `Version`                   | `"1.21.90"`        | Target format version. Required for Custom Components V2 (custom components as direct `components` entries). |
| `Category`          | `Enums.CatalogueCategory`   | `Items`            | Creative inventory tab (`Construction`, `Nature`, `Equipment`, `Items`, or `None`). |
| `Group`             | `string?`                   | `null`             | Sub-group inside the chosen category (max 256 characters). |
| `States`            | `Dictionary<string, object[]>` | `{}`           | Custom block states (see below). |
| `Permutations`      | `List<BlockPermutation>`    | `[]`               | Conditional variants of the block (see [Block Permutations](block-permutations.md)). |
| `Tags`              | `string[]`                  | `[]`               | Block tags written as empty `tag:<name>` components. |
| `DisplayName`       | `string?`                   | `null`             | Shortcut for `minecraft:display_name`. |
| `LangName`          | `string?`                   | `DisplayName`      | Localized name written to `texts/en_US.lang`. Defaults to `DisplayName`. |
| `Geometry`          | `string?`                   | `"minecraft:geometry.full_block"` | Shortcut for `minecraft:geometry`. Override for custom models or `"minecraft:geometry.cross"`. |
| `ResourceTexture`   | `string?`                   | `null`             | Texture key written to `rp/blocks.json`. |
| `Sound`             | `string?`                   | `null`             | Sound identifier written to `rp/blocks.json`. |
| `Friction`          | `float?`                    | `null`             | Shortcut for `minecraft:friction`. |
| `LightEmission`     | `int?`                      | `null`             | Shortcut for `minecraft:light_emission` (0-15). |
| `LightDampening`    | `int?`                      | `null`             | Shortcut for `minecraft:light_dampening`. |
| `Replaceable`       | `bool?`                     | `null`             | Shortcut for `minecraft:replaceable`. |
| `Loot`              | `LootTable?`                | `null`             | Loot table reference for `minecraft:loot`. Auto-registers the table during compile. See [Loot Tables](../item/loot-table.md). |
| `BlockEvents`       | `BlockEvents?`              | `null`             | Script API event handlers (`ScriptHandler` inline or `FromFile`). See [Block Events](block-events.md). |
| `DynamicTraits`     | `Trait[]`                   | `[]`               | Hand-built `Trait` components for identifiers without a generated trait interface. See [Dynamic Traits](../advanced/trait-system.md#dynamic-traits). |
| `Singles`           | `Dictionary<Identifier, object>` | `{}`          | Components written as a single scalar value instead of an object body (`"namespace:comp": value`). See [Singles](../advanced/trait-system.md#singles). |

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
public override Dictionary<string, object[]> States => new()
{
    { "mynamespace:power_level", [0, 1, 2, 3, 4] },
    { "mynamespace:is_active", [true, false] }
};
```

> [!CAUTION]
> Although the state dictionary's value type is `object[]`, Minecraft will only accept `int`, `float`, `bool`, and `string` values. Keep each array homogeneous and limited to those types.

> [!WARNING]
> Minecraft limits a state to **16** possible values. ingot **throws** `ArgumentException` at compile time if a state exceeds that limit.

> [!TIP]
> State names should be fully qualified (`namespace:state_name`) for best compatibility.

## Adding Behavior with Traits

Most block functionality comes from implementing [traits](../advanced/trait-system.md):

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

## Block Events (Script API)

Use `BlockEvents` to attach Script API custom component handlers without hand-writing JavaScript registration code:

```csharp
using ingot.Core.Scripting;

public override BlockEvents? BlockEvents => new()
{
    OnPlaceEvent = "event.dimension.playSound('random.click', event.block.location);",
    // or load handler bodies from files:
    PlayerInteractEvent = ScriptHandler.FromFile("./scripts/blocks/pressure_plate_interact.js"),
};
```

Set `pack.ScriptsEnabled = true` before compiling. **ingot** writes handler scripts to `bp/scripts/blocks/`, adds the custom component to your block JSON, and imports them from a generated `bp/scripts/main.js`. For global tick logic, use [services](../script-services.md) via `pack.AddService(...)`.

> [!IMPORTANT]
> Event scripts are not generated unless `ScriptsEnabled` is `true`. Without it, ingot warns at compile time and skips script output.

See the dedicated [Block Events](block-events.md) guide for the full event list, trait validation warnings, and compile pipeline details.

## Compilation

You rarely call `Block.Compile` directly. Instead register blocks with `Pack.Create` and declare textures on the block class via `MaterialInstance.SourcePath`:

```csharp
using ingot.Core;

const string packUuid = "77f1fef2-bb39-411a-b25c-ae475c21169f";
string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");

Pack pack = Pack.Create(packUuid, "My Addon", "Blocks made with ingot")
    .AddBlock<DenseLasagnaBlock>()
    .AddBlock<AnotherBlock>();

pack.AddBlockTexture("block_of_dense_lasagna", Path.Combine(dataDir, "dense_lasagna.png"));

pack.Compile("./output");
```

To register a pre-configured instance (for example after mutating fields at runtime), use the behaviour pack:

```csharp
DenseLasagnaBlock variant = new DenseLasagnaBlock();
// ... configure variant ...
pack.BehaviourPack.AddBlockFromInstance(variant);
```

`Block.Compile(Type)`, `Block.Compile<T>()`, and `Block.CompileFromInstance(inst)` are available when you need JSON without a full pack compile.

```csharp
public override string? Geometry => "minecraft:geometry.full_block";
public override string? ResourceTexture => "block_of_dense_lasagna";
public override string? Sound => "stone";

public override MaterialInstances MaterialInstances => new()
{
    All = new MaterialInstance("block_of_dense_lasagna", MaterialInstance.RenderMethods.AlphaTest)
};
```

Register textures with `pack.AddBlockTexture(key, path)` or provide a `SourcePath` on the `MaterialInstance`. Capture identifiers from your block class when you need them for cross-references.

For custom block models (anything other than vanilla `minecraft:geometry.full_block` or `minecraft:geometry.cross`), register the `.geo.json` file alongside your textures:

```csharp
pack.AddGeometry("geometry.my_block", Path.Combine(dataDir, "my_block.geo.json"));
```

```csharp
public override string? Geometry => "geometry.my_block";
```

This writes the full behaviour pack under `bp/` (including `bp/blocks/block_of_dense_lasagna.json` - the filename is the part after the `:` in the identifier) and the resource pack under `rp/` (including copied textures, geometry files, and the generated `terrain_texture.json` that maps your texture keys).

See the [Resource Packs & Textures](../resource-packs.md) guide for details on asset organization, the generated atlas files, and how texture keys bridge behaviour and resources.

## Full Example

See `DenseLasagnaBlock.cs` in the [`ingot.Example`](https://github.com/pyroboots/ingot/tree/master/ingot.Example) project for a working block that combines states, permutations, material instances, and a [loot table](../item/loot-table.md).

## Tips & Gotchas

> [!IMPORTANT]
> Always provide `MaterialInstances` (it is abstract). `Geometry` defaults to `"minecraft:geometry.full_block"` for standard cubes. For non-vanilla shapes, set `Geometry` to your custom identifier and register the `.geo.json` with `Pack.AddGeometry`.

- Set `ResourceTexture` and `Sound` when you want entries in `rp/blocks.json`.
- Block state values are serialized verbatim; make sure your Molang conditions in permutations match the exact values and state names.
- Many traits have a mixture of required (`abstract`) and optional (`virtual`) members - the compiler will emit null/empty values for missing abstracts, but you will get warnings.
- Traits are discovered only on the concrete type you pass to `AddBlock<T>`. Inheritance of your own block base classes works as long as the interfaces are implemented somewhere in the hierarchy.

> [!TIP]
> For complex blocks, prefer many small focused traits over one giant class.

Next: learn about [block events](block-events.md), [script services](../script-services.md), [block permutations](block-permutations.md), and [material instances](block-mat-instances.md).