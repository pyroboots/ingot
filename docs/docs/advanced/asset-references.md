# Asset References

Asset references are typed helpers in `ingot.Core.Resource` that **register** a pack asset and **return the string key or id** you would otherwise hard-code. Construct them while assigning properties (texture keys, geometry ids, recipe links, and so on). During pack compile they side-effect onto the current `Pack`, so related content can compile together without a separate `Add*` call.

They are a more consistent alternative to loose strings: the type carries intent (`TextureReference` vs a bare `"my_tex"`), and groups of content can pull each other into the pack (for example an item can compile its recipe without `AddRecipe<T>()`).

> [!IMPORTANT]
> Reference constructors run only when `CompilerState.CurrentPack` is set (normal pack compile). Constructing them outside compile throws `InvalidOperationException`.

## How they work

1. **Construct** while reading a property during compile (e.g. `Texture => new TextureReference<MyItem>(path)`).
2. The ctor reads `CompilerState.CurrentPack`, registers the asset if needed, and logs `implicitly registered ...` when something new is added.
3. **Implicit conversion to `string`** returns the atlas key, geometry id, recipe identifier, loot reference, or render controller id.
4. **Implicit conversion to the non-generic `FooReference`** boxes the parent type and string for properties such as `Item.Recipe` / `Block.Recipe`.

Registration is **deduplicated**: existing texture keys, geometry ids, recipes, loot tables, and render controllers are left as-is (manual `Pack` registrations win over `TryAdd*` texture paths).

## Available reference types

| Type | Registers | String value | Typical assignment |
|------|-----------|--------------|--------------------|
| [`TextureReference<TParent>`](#texturereference) | Block / item / entity PNG into the matching atlas or entity textures | Atlas key or texture key | `Item.Texture`, `MaterialInstance` texture |
| [`GeometryReference<TParent>`](#geometryreference) | `.geo.json` under `models/{entity\|blocks}/` (entity or block parent only) | Geometry id from the geo JSON | Geometry traits / client geometry strings |
| [`RecipeReference<TRecipe>`](#recipereference) | `IRecipe` on the behaviour pack | Recipe `Identifier` | `Item.Recipe`, `Block.Recipe` |
| [`LootTableReference<TLootTable>`](#loottablereference) | `LootTable` on the behaviour pack | `LootTable.Reference` directory | String properties that need a loot path + registration |
| [`RenderControllerReference<T>`](#rendercontrollerreference) | `RenderController` on the resource pack | `ControllerId` | `ClientEntity.RenderControllers` |

Namespace: `ingot.Core.Resource`.

---

## TextureReference

```csharp
public class TextureReference<TParent>
    where TParent : class, IIdentifiable, ITraitable, IConcreteCompilable<TParent>, new()
```

`TParent` must be an `Entity`, `Item`, or `Block` subclass. It decides **which** resource API is used:

| `TParent` | Registration |
|-----------|----------------|
| `Entity` (or subclass) | `ResourcePack.TryAddEntityTexture` |
| `Item` | `ResourcePack.TryAddItemTexture` |
| `Block` | `ResourcePack.TryAddBlockTexture` |
| anything else | `ArgumentException` |

### Constructor

```csharp
public TextureReference(string path, string? id = null)
```

| Parameter | Description |
|-----------|-------------|
| `path` | Source PNG on disk |
| `id` | Atlas/texture key. If omitted, generated as `{namespace}_{name}_{filenameWithoutExtension}` from `new TParent().Identifier` and the file name |

If the key was already registered (for example via `Pack.AddBlockTexture`), `TryAdd*` is a no-op and no info log is emitted.

### Implicit conversions

```csharp
string key = new TextureReference<MyItem>("assets/cheese.png"); // key only
// or assign where a string is expected:
public override string Texture =>
    new TextureReference<CheeseItem>(Path.Combine(AppContext.BaseDirectory, "Data", "cheese.png"));
```

### Item example

```csharp
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.Resource;

public class CheeseItem : Item
{
    public override Identifier Identifier => new("test:cheese");
    public override string Texture =>
        new TextureReference<CheeseItem>(Path.Combine(AppContext.BaseDirectory, "Data", "cheese.png"));
    public override string DisplayName => "Cheese";
}
```

With `Identifier` `test:cheese` and file `cheese.png`, the default key is `test_cheese_cheese`. Prefer an explicit `id` when you want a stable short atlas name:

```csharp
new TextureReference<CheeseItem>(path, id: "cheese");
```

### Block material instance example

`MaterialInstance` takes a texture **key** (string). Pass a `TextureReference` so the key is registered and written in one place:

```csharp
using ingot.Core.Behaviour.Block;
using ingot.Core.Resource;

public override MaterialInstances MaterialInstances => new()
{
    All = new MaterialInstance(
        new TextureReference<DenseLasagnaBlock>(
            Path.Combine(AppContext.BaseDirectory, "Data", "dense_lasagna.png")),
        MaterialInstance.RenderMethods.AlphaTest)
};
```

### Manual override

You can still call `Pack.AddBlockTexture` / `AddItemTexture` / `AddEntityTexture` first. Those win for the same key; the reference then only supplies the string id.

See also: [Resource Packs & Textures](../resource-packs.md), [Block Material Instances](../block/block-mat-instances.md).

---

## GeometryReference

```csharp
public class GeometryReference<TParent>
    where TParent : class, IIdentifiable, ITraitable, IConcreteCompilable<TParent>, new()
```

`TParent` must be an `Entity` or `Block` subclass (`Item` throws `ArgumentException`). Registers a `.geo.json` source and returns the geometry id string.

### Constructor

```csharp
public GeometryReference(string path, string? id = null)
```

| Parameter | Description |
|-----------|-------------|
| `path` | Source `.geo.json` on disk |
| `id` | Unused. The id is read from `minecraft:geometry[0].description.identifier` in the source JSON |

Subdirectory under `models/` depends on `TParent`:

| `TParent` | `modelsSubdir` |
|-----------|----------------|
| `Entity` | `entity` (`AddEntityGeometry`) |
| `Block` | `blocks` (`AddGeometry`) |

If `GeometrySources` already contains that id, registration is skipped.

> [!IMPORTANT]
> The geometry identifier must already be inside the `.geo.json` file. The constructor does not generate or override it.

### Example

```csharp
public override string? Geometry =>
    new GeometryReference<MyBlock>("assets/my_block.geo.json");
```

---

## RecipeReference

```csharp
public class RecipeReference<TRecipe> where TRecipe : IRecipe, new()
```

Parameterless constructor: instantiates `TRecipe`, takes its `Identifier` as the string id, and if no recipe with that identifier is on the behaviour pack yet, calls `AddRecipe<TRecipe>()`.

### Item / block `Recipe` property

`Item` and `Block` both expose:

```csharp
public virtual RecipeReference? Recipe => null;
```

Override it to attach a crafting (or other) recipe to that content. Compile **touches** the property (`_ = inst.Recipe`) so the lazy property body runs and registration happens even though the value is discarded for JSON.

```csharp
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.Resource;
using ingot.Example.Recipes;

public class LasagnaItem : Item
{
    public override Identifier Identifier => new("test:lasagna");
    public override string Texture => "suspicious_stew";

    public override RecipeReference Recipe => new RecipeReference<LasagnaRecipe>();
}
```

Then you only need:

```csharp
pack.AddItem<LasagnaItem>();
// no separate AddRecipe<LasagnaRecipe>() required
```

Explicit `AddRecipe<T>()` is still fine for recipes that nothing references.

See also: [Recipes](../item/recipe.md).

---

## LootTableReference

```csharp
public class LootTableReference<TLootTable> where TLootTable : LootTable, new()
```

Parameterless constructor: instantiates `TLootTable`, uses its `Reference` directory string (for example `loot_tables/blocks`), and if no loot table with that reference is present, calls `AddLootTable<TLootTable>()`.

```csharp
// Registers DenseLasagnaLoot and yields the directory reference string
string path = new LootTableReference<DenseLasagnaLoot>();
```

### Prefer `Block.Loot` for block drops

For blocks (and permutations), the usual API is still the **instance** shortcut, which writes `minecraft:loot` from `RelativePath` and auto-registers the type:

```csharp
public override LootTable? Loot => new DenseLasagnaLoot();
```

Use `LootTableReference<T>` when you need the **reference-style** pattern (string conversion + registration) similar to recipes, rather than assigning a `LootTable` instance.

See also: [Loot Tables](../item/loot-table.md).

---

## RenderControllerReference

```csharp
public class RenderControllerReference<TRenderController>
    where TRenderController : RenderController, new()
```

Parameterless constructor: takes `ControllerId` from a new instance and, if that id is not already in `RegisteredRenderControllerIds`, calls `AddRenderController<TRenderController>()`.

### Client entity example

```csharp
public override string[] RenderControllers =>
[
    new RenderControllerReference<CowV3RenderController>()
];

public override bool EmitDefaultRenderController => false;
```

The array expects controller id strings; the reference registers the custom controller and supplies that id. Nested render controller types on the behaviour or client entity can also be discovered by `AddEntity` - this reference is the explicit property-side form.

See also: [Client Entities & Render Controllers](../entity/client-entity.md).

---

## Compile-time only

All constructors require an active pack:

```csharp
Pack pack = CompilerState.CurrentPack
    ?? throw new InvalidOperationException("... registration only valid during pack compilation");
```

Implications:

- Property getters that construct references are safe when the type is compiled through `Pack.Compile` / `CompileMcaddon` / `CompileComMojang`.
- Calling `Item.Compile` / `Block.Compile` **without** a current pack will throw if those getters construct references.
- Side effects (file copy, atlas entries, recipe JSON, etc.) run as part of the full pack pipeline after registration.

With `verbose: true` (default), successful first-time registrations appear in the console / `ingot.log` as `implicitly registered ...`.

---

## When to use references vs manual APIs

| Situation | Prefer |
|-----------|--------|
| Texture path lives next to the item/block that uses it | `TextureReference<T>` on `Texture` / `MaterialInstance` |
| Geometry file tied to one block or entity | `GeometryReference<T>` (id comes from the geo JSON) |
| Recipe only exists for one item/block | `Recipe => new RecipeReference<MyRecipe>()` |
| Shared or standalone recipe | `pack.AddRecipe<T>()` |
| Block drops | `Loot => new MyLoot()` (instance) |
| Custom client render controller listed on the client entity | `RenderControllerReference<T>` in `RenderControllers` |
| Global / shared assets, overrides, particles, sounds | Manual `Pack.Add*` APIs |

Asset references do not replace [resource pack](../resource-packs.md) manual registration; they are the **property-local** way to do the same work while keeping keys typed and co-located with the content that needs them.
