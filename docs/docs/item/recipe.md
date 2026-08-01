# Recipes

Crafting recipes are defined by deriving from `ShapedRecipe` or `ShapelessRecipe` in `ingot.Core.Behaviour.Recipe`. Register them on a `Pack` with `AddRecipe<T>()`.

## Shaped Recipe

Use `ShapedRecipe` when ingredient placement matters:

```csharp
using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;

public class LasagnaRecipe : ShapedRecipe
{
    public override Identifier Identifier => new("test:lasagna");

    private Identifier Cheese => new("test:cheese");
    private Identifier Pasta => new("test:pasta");
    private Identifier Sauce => new("test:spooky_special_sauce");

    public override Identifier?[][] Pattern =>
    [
        [Cheese, Pasta, Cheese],
        [Pasta, Sauce, Pasta],
        [Sauce, Sauce, Sauce]
    ];

    public override Identifier Result => new("test:lasagna");
}
```

Key members:

| Member         | Type                  | Required | Description |
|----------------|-----------------------|----------|-------------|
| `Identifier`   | `Identifier`          | Yes      | Full recipe identifier (`namespace:name`). |
| `Pattern`      | `Identifier?[][]`     | Yes      | Crafting grid. Use `null` cells for empty slots. |
| `Result`       | `Identifier`          | Yes      | Output item identifier. |
| `ResultAmount` | `int`                 | No       | Stack size produced. Defaults to `1`. |
| `Tags`         | `string[]`            | No       | Crafting interfaces this recipe works in. Defaults to `["crafting_table"]`. |

The compiler maps distinct `Identifier` values in the pattern to single-character symbols and emits a `minecraft:recipe_shaped` JSON file.

> [!CAUTION]
> Patterns wider than 3 columns or taller than 3 rows produce compile-time warnings.

## Shapeless Recipe

Use `ShapelessRecipe` when ingredient order does not matter:

```csharp
using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;

public class MushroomStewRecipe : ShapelessRecipe
{
    public override Identifier Identifier => new("mynamespace:mushroom_stew");

    public override RecipeItem[] Ingredients =>
    [
        new() { Item = new("minecraft:red_mushroom") },
        new() { Item = new("minecraft:brown_mushroom") },
        new() { Item = new("minecraft:bowl") }
    ];

    public override RecipeItem Result => new() { Item = new("minecraft:mushroom_stew") };
}
```

`RecipeItem` is a record with:

| Member  | Type         | Default | Description |
|---------|--------------|---------|-------------|
| `Item`  | `Identifier` | -       | Item identifier (required). |
| `Count` | `int`        | `1`     | Amount required or produced. |
| `Tag`   | `string?`    | `null`  | Optional item tag matcher. |

## Furnace Recipe

Use `FurnaceRecipe` for smelting recipes:

```csharp
public class SmeltLasagnaRecipe : FurnaceRecipe
{
    public override Identifier Identifier => new("test:smelt_lasagna");
    public override Identifier Input => new("test:raw_lasagna");
    public override Identifier Output => new("test:lasagna");
}
```

| Member     | Type           | Default      | Description |
|------------|----------------|--------------|-------------|
| `Input`    | `Identifier`   | (required)   | Item to smelt. |
| `Output`   | `Identifier`   | (required)   | Result item. |
| `Tags`     | `string[]`     | `["furnace"]`| Smelting interfaces (e.g. `blast_furnace`, `smoker`). |

Compiles to `minecraft:recipe_furnace`.

## Brewing Mix Recipe

Use `BrewingMixRecipe` for brewing stand recipes:

```csharp
public class AwkwardPotionRecipe : BrewingMixRecipe
{
    public override Identifier Identifier => new("test:awkward_potion");
    public override Identifier Input => new("minecraft:potion_type:water");
    public override Identifier Reagent => new("minecraft:nether_wart");
    public override Identifier Output => new("minecraft:potion_type:awkward");
}
```

| Member    | Type           | Default             | Description |
|-----------|----------------|---------------------|-------------|
| `Input`   | `Identifier`   | (required)          | Bottle/potion in the input slot. |
| `Reagent` | `Identifier`   | (required)          | Ingredient added to the stand. |
| `Output`  | `Identifier`   | (required)          | Resulting item. |
| `Tags`    | `string[]`     | `["brewing_stand"]` | Brewing interfaces. |

Compiles to `minecraft:recipe_brewing_mix`. **ingot** warns if potion inputs/outputs are missing auxiliary values.

## Custom Recipe Types

All built-in recipe bases implement `IRecipe` and can be registered with `AddRecipe<T>()` without any special handling in `BehaviourPack`. To add your own recipe category, create an abstract base that implements both `IRecipe` and `IConcreteCompilable<TSelf>`:

```csharp
public abstract class SmithingRecipe : IRecipe, IConcreteCompilable<SmithingRecipe>
{
    public abstract Identifier Identifier { get; }

    // Instance compile used by pack registration
    public string Compile() => CompileFromInstance(this);

    public static string Compile(Type tType)
    {
        SmithingRecipe inst = /* construct from tType */;
        return CompileFromInstance(inst);
    }

    public static string Compile<TConcreteType>() where TConcreteType : SmithingRecipe, new() =>
        Compile(typeof(TConcreteType));

    public static string CompileFromInstance(SmithingRecipe inst)
    {
        // emit JSON for your recipe type from inst
        throw new NotImplementedException();
    }
}
```

User-defined recipes that extend an existing base (e.g. `MyRecipe : ShapedRecipe`) work automatically with `AddRecipe<MyRecipe>()`.

## Compilation & Registration

```csharp
using ingot.Core;

Pack pack = Pack.Create(Guid.NewGuid().ToString(), "My Addon", "Recipes made with ingot")
    .AddItem<LasagnaItem>()
    .AddRecipe<LasagnaRecipe>();

pack.Compile("./output");
```

`AddRecipe<T>()` returns the `Pack` for fluent chaining, the same as `AddItem<T>()`, `AddBlock<T>()`, and `AddLootTable<T>()`. Capture identifiers from your recipe class when you need a single source of truth for cross-references.

For a pre-configured recipe instance, use `pack.BehaviourPack.AddRecipeFromInstance(inst)`.

This writes `bp/recipes/lasagna.json` (filename is the part after the `:` in the identifier). Shaped recipes use `format_version` `"1.12"` and compile to `minecraft:recipe_shaped`; shapeless recipes compile to `minecraft:recipe_shapeless`.

A compiled shaped recipe looks like this:

```json
{
    "format_version": "1.12",
    "minecraft:recipe_shaped": {
        "description": {
            "identifier": "test:lasagna"
        },
        "tags": ["crafting_table"],
        "pattern": [
            "!@!",
            "@#@",
            "###"
        ],
        "key": {
            "!": { "item": "test:cheese" },
            "@": { "item": "test:pasta" },
            "#": { "item": "test:spooky_special_sauce" }
        },
        "result": {
            "item": "test:lasagna",
            "count": 1
        }
    }
}
```

> [!IMPORTANT]
> `Guid.NewGuid().ToString()` is for demonstration purposes. Use a static UUID at runtime for your pack, otherwise Minecraft will treat every rebuild as a completely different pack.

## Full Example

See `LasagnaRecipe.cs` in the [`ingot.Example`](https://github.com/pyroboots/ingot/tree/master/ingot.Example) project, which is registered alongside the lasagna block and item in `Program.cs`.

## Tips

> [!NOTE]
> Recipe identifiers are independent from item identifiers, but they often share the same `namespace:name` when the recipe crafts that item.

> [!TIP]
> `null` entries in a shaped `Pattern` row become spaces in the compiled pattern string.

- Built-in recipe types: `ShapedRecipe`, `ShapelessRecipe`, `FurnaceRecipe`, and `BrewingMixRecipe`. Extend one of these or implement `IRecipe` + `IConcreteCompilable<T>` (with `Compile`, `Compile<TConcrete>()`, and `CompileFromInstance`) for custom types.