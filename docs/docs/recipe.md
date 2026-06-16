# Recipes

Crafting recipes are defined by deriving from `ShapedRecipe` or `ShapelessRecipe` in `ingot.Core.Behaviour.Recipe`. Register them on a `BehaviourPack` with `AddRecipe<T>()`.

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

## Compilation & Registration

```csharp
using ingot.Core;
using ingot.Core.Common;

BehaviourPack bp = BehaviourPack.Create(Guid.NewGuid().ToString())
    .AddItem<LasagnaItem>()
    .AddRecipe<LasagnaRecipe>();

Pack pack = new()
{
    Name = "My Addon",
    Description = "Recipes made with ingot",
    BehaviourPack = bp,
    ResourcePack = ResourcePack.Create(Guid.NewGuid().ToString()),
    LinkPacks = true
};

pack.Compile("./output");
```

`AddRecipe<T>()` returns the `BehaviourPack` for fluent chaining, the same as `AddItem<T>()`, `AddBlock<T>()`, and `AddLootTable<T>()`. Capture identifiers from your recipe class when you need a single source of truth for cross-references.

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

See `LasagnaRecipe.cs` in the [`ingot.Example`](../../ingot.Example) project, which is registered alongside the lasagna block and item in `Program.cs`.

## Tips

- Recipe identifiers are independent from item identifiers, but they often share the same `namespace:name` when the recipe crafts that item.
- `null` entries in a shaped `Pattern` row become spaces in the compiled pattern string.
- Only `ShapedRecipe` and `ShapelessRecipe` are compiled today. Other recipe types are not yet supported.