# Your First Item

In this step, you'll create a custom item called **Dirt Soup** complete with food behaviour, a texture key, and a crafting recipe.

## Class Setup

In `Content/Items/`, create a new class called `DirtSoupItem.cs`:

```cs
namespace MyAddon.Content.Items;

public class DirtSoupItem
{

}
```

To turn this class into an item that can be compiled by **ingot**, inherit from `Item` in `ingot.Core.Behaviour.Item`.

> [!TIP]
> Add `using ingot.Core.Behaviour.Item;` and `using ingot.Core.Common;` at the top of the file.

This exposes the two mandatory properties of an item:

```cs
public override Identifier Identifier { get; }
public override string Texture { get; }
```

Set `Identifier` to `myaddon:dirt_soup` and `Texture` to `dirt_soup` (the texture atlas key - we'll point it at a PNG later):

```cs
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;

namespace MyAddon.Content.Items;

public class DirtSoupItem : Item
{
    public override Identifier Identifier => "myaddon:dirt_soup";
    public override string Texture => "dirt_soup";
    public override string DisplayName => "Dirt Soup";
}
```

> [!TIP]
> `Identifier` converts implicitly from strings, so `"myaddon:dirt_soup"` works without `new Identifier(...)`. Nested arguments stay readable either way.

## Adding Food Behaviour

To make the item edible, implement trait interfaces. Traits are C# interfaces that map to Minecraft `minecraft:*` components - see the [Trait System](../advanced/trait-system.md) for the full story.

For food you typically want three traits together:

| Trait | Why |
|-------|-----|
| `IFood` | Nutrition, saturation, leftover item |
| `IUseAnimation` | Play the eat animation |
| `IUseModifiers` | Non-zero `use_duration` so eating actually works |

> [!IMPORTANT]
> `IFood` requires a non-zero **`use_duration`**. Implement `IUseModifiers` and set `UseDuration` (for example `1.6f` for a normal eat). Without it, the content log warns and eating may not work. `IUseModifiers` also requires `FormatVersion` `1.26.30` or compile throws.

```cs
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits.Item;

using Version = ingot.Core.Common.Version;

namespace MyAddon.Content.Items;

public class DirtSoupItem : Item, IFood, IUseAnimation, IUseModifiers
{
    // IUseModifiers requires format_version >= 1.26.30
    public override Version FormatVersion => new(1, 26, 30);
    public override Identifier Identifier => "myaddon:dirt_soup";
    public override string Texture => "dirt_soup";
    public override string DisplayName => "Dirt Soup";

    // IFood
    int IFood.Nutrition => 3;
    float IFood.SaturationModifier => 0.6f;
    bool IFood.CanAlwaysEat => true;
    // warning: dirt is not as nutritious in real life
    dynamic IFood.UsingConvertsTo => "minecraft:bowl";

    // IUseAnimation
    string IUseAnimation.Value => "eat";

    // IUseModifiers
    float IUseModifiers.UseDuration => 1.6f;
    float IUseModifiers.MovementModifier => 0.35f;
    string IUseModifiers.StartUsing => IUseModifiers.StartUsing_Always;

    // StartSound is abstract; exclude it when you have no start sound
    [IngotExclude]
    string IUseModifiers.StartSound => null!;
}
```

> [!TIP]
> Prefer **explicit interface implementation** (`int IFood.Nutrition => ...`). It avoids name clashes between traits and keeps the public surface of your class clean.

### Texture path (optional but recommended)

If you put a PNG at `Data/dirt_soup.png` and copy `Data/` to the build output (see [Setup](setup.md)), you can auto-register the icon from the class:

```cs
public override string? TexturePath =>
    Path.Combine(AppContext.BaseDirectory, "Data", "dirt_soup.png");
```

You can also register textures manually in `Program.cs` later with `pack.AddItemTexture(...)` - both approaches are valid. Manual registration wins if both are set.

## Adding a Recipe

Dirt Soup should be craftable. In `Content/Recipes/`, create `DirtSoupRecipe.cs` as a shapeless recipe: one bowl and one dirt.

```cs
using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;

namespace MyAddon.Content.Recipes;

public class DirtSoupRecipe : ShapelessRecipe
{
    public override Identifier Identifier => "myaddon:dirt_soup";

    public override RecipeItem[] Ingredients =>
    [
        new() { Item = "minecraft:bowl" },
        new() { Item = "minecraft:dirt" }
    ];

    public override RecipeItem Result => new()
    {
        Item = "myaddon:dirt_soup"
    };
}
```

> [!NOTE]
> `ShapedRecipe` is available when grid position matters. See [Recipes](../item/recipe.md) for shaped, furnace, and brewing recipes.

## Full Item File

```cs
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits.Item;

using Version = ingot.Core.Common.Version;

namespace MyAddon.Content.Items;

public class DirtSoupItem : Item, IFood, IUseAnimation, IUseModifiers
{
    public override Version FormatVersion => new(1, 26, 30);
    public override Identifier Identifier => "myaddon:dirt_soup";
    public override string Texture => "dirt_soup";
    public override string DisplayName => "Dirt Soup";

    public override string? TexturePath =>
        Path.Combine(AppContext.BaseDirectory, "Data", "dirt_soup.png");

    int IFood.Nutrition => 3;
    float IFood.SaturationModifier => 0.6f;
    bool IFood.CanAlwaysEat => true;
    dynamic IFood.UsingConvertsTo => "minecraft:bowl";

    string IUseAnimation.Value => "eat";

    float IUseModifiers.UseDuration => 1.6f;
    float IUseModifiers.MovementModifier => 0.35f;
    string IUseModifiers.StartUsing => IUseModifiers.StartUsing_Always;

    [IngotExclude]
    string IUseModifiers.StartSound => null!;
}
```

## What You Have So Far

| Piece | Type | Identifier |
|-------|------|------------|
| `DirtSoupItem` | Item + food traits | `myaddon:dirt_soup` |
| `DirtSoupRecipe` | Shapeless recipe | `myaddon:dirt_soup` |

You have not registered these on a `Pack` yet - that happens in [Step 4](compile.md). First, add a block and an entity so the pack feels complete.

**Next:** [2. Your First Block](your-first-block.md)

**Also see:** [Making an Item](../item/item.md) for creative categories, durability, block placers, and more traits.
