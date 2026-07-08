# Making an Item

Items are defined by deriving from the abstract `Item` class. Like blocks, items use a combination of simple virtual properties and the [trait system](trait-system.md) to describe their behavior.

## Minimal Item

```csharp
using ingot.Core.Behaviour;
using ingot.Core.Common;

public class MyItem : Item
{
    public override Identifier Identifier => new("mynamespace:my_item");
    public override string Texture => "my_item_icon";
}
```

Every item **must** implement:

- `Identifier` - full `namespace:name`.
- `Texture` - the icon texture reference (used inside `minecraft:icon`).

Optionally override `TexturePath` to provide the source PNG. When set, ingot auto-registers the icon during compile unless already added manually.

## Key Properties

| Property            | Type               | Default          | Description |
|---------------------|--------------------|------------------|-----------|
| `FormatVersion`     | `Version`          | `"1.20.10"`      | Target format version. |
| `Category`          | `Item.CatalogueCategory`| `Items`     | Which creative inventory tab the item appears in (`Construction`, `Nature`, `Equipment`, `Items`, or `None`). |
| `Group`             | `string?`          | `null`           | Sub-group inside the chosen category (max 256 characters). |
| `HiddenInCommands`  | `bool`             | `false`          | If true, the item cannot be used in commands that take item arguments. |
| `MaxStackSize`      | `int`              | `64`             | Shortcut for `minecraft:max_stack_size`. |
| `DisplayName`       | `string`           | `Identifier`     | Shortcut for `minecraft:display_name`. |
| `AllowOffhand`      | `bool`             | `false`          | Shortcut for `minecraft:allow_off_hand`. |
| `TexturePath`       | `string?`          | `null`           | Optional source PNG for `Texture`. Auto-registered during compile. |
| `ItemEvents`        | `ItemEvents?`      | `null`           | Script API event handlers (`ScriptHandler` inline or `FromFile`). See [Item Events](item-events.md). |

These are written into the `description.menu_category` and `components` sections of the generated item JSON.

## Adding Behavior with Traits

The vast majority of interesting item features come from implementing `IItemTrait` interfaces:

```csharp
public class LasagnaItem : Item, IFood, IBlockPlacer, IUseAnimation, IUseModifiers
{
    public override Identifier Identifier => new("test:lasagna");
    public override string Texture => "lasagna";
    public override string DisplayName => "Lasagna";

    // IFood
    int IFood.Nutrition => 5;
    float IFood.SaturationModifier => 0.9f;
    string IFood.UsingConvertsTo => "minecraft:bowl";

    // Required with food: non-zero use duration (and usually an eat animation)
    string IUseAnimation.Value => "eat";
    float IUseModifiers.UseDuration => 1.6f;
    float IUseModifiers.MovementModifier => 0.35f;
    dynamic? IUseModifiers.StartUsing => "always";
    dynamic? IUseModifiers.StartSound => null;

    // IBlockPlacer
    dynamic IBlockPlacer.Block => "test:block_of_dense_lasagna";
    bool IBlockPlacer.ReplaceBlockItem => true;
}
```

> [!IMPORTANT]
> `IFood` requires a non-zero **`minecraft:use_modifiers` → `use_duration`**. Implement `IUseModifiers` and set `UseDuration` (e.g. `1.6f` for a normal eat). Without it, the content log warns and eating may not work correctly.

> [!TIP]
> Because some traits will have common property names, its recommended to implement the properties explicitly to be more readable, less ambiguous and it also looks prettier.

Common item traits include:

- `IFood` - makes the item edible (pair with `IUseModifiers` + usually `IUseAnimation`)
- `IUseModifiers` - `use_duration`, movement while using, `start_using`
- `IUseAnimation` - e.g. `"eat"`, `"drink"`
- `IBlockPlacer` - places a block when used
- `IDurability` + `IDamage` - tools/weapons
- `IDigger` - mining speed on different blocks
- `IThrowable`, `IProjectile`, `IShooter` - ranged items
- `IWearable`, `IEnchantable`, `IRarity`, etc.

See the [Item Traits API reference](https://pyroboots.github.io/ingot/api/ingot.Core.TraitSystem.Traits.Item.html) for the complete list.

## Creative Menu Placement

```csharp
public class FancyTool : Item
{
    public override Identifier Identifier => new("mynamespace:fancy_tool");
    public override string Texture => "fancy_tool";

    public override Item.CatalogueCategory Category => Item.CatalogueCategory.Equipment;
    public override string? Group => "itemGroup.name.tools";   // or your own group
}
```

Set `Category = Item.CatalogueCategory.None` (and optionally `HiddenInCommands = true`) for purely technical items that should not appear in the creative inventory or be summonable easily.

## Item Events (Script API)

Use `ItemEvents` to attach Script API custom component handlers without hand-writing JavaScript registration code:

```csharp
using ingot.Core.Scripting;

public override ItemEvents? ItemEvents => new()
{
    UseEvent = "event.source.sendMessage('You used the item!');",
    // or load handler bodies from files:
    UseOnEvent = ScriptHandler.FromFile("./scripts/items/magic_wand_use_on.js"),
};
```

Set `pack.ScriptsEnabled = true` before compiling. **ingot** writes handler scripts to `bp/scripts/items/`, adds the custom component to your item JSON, and imports them from a generated `bp/scripts/main.js`. For global tick logic, use [services](script-services.md) via `pack.AddService(...)`.

See the dedicated [Item Events](item-events.md) guide for the full event list, trait validation warnings, and compile pipeline details.

## Compilation & Registration

Register items with `Pack.Create` and declare icon paths on the item class:

```csharp
using ingot.Core;

public class LasagnaItem : Item
{
    public override Identifier Identifier => new("test:lasagna");
    public override string Texture => "lasagna";
    public override string? TexturePath => "assets/lasagna.png";
}

Pack pack = Pack.Create(Guid.NewGuid().ToString(), "My Addon", "Items made with ingot")
    .AddItem<LasagnaItem>()
    .AddItem<FancyTool>();

pack.Compile("./output");
```

Use `pack.AddItemTexture(key, path)` only when you need a manual override. Capture identifiers from your item class for cross-references (recipes, loot tables, scripts, etc.) without repeating string literals.

This produces `bp/items/lasagna.json` (filename is the part after the `:` in the identifier) and the corresponding resources under `rp/textures/items/` plus `rp/textures/item_texture.json`.

See the [Resource Packs & Textures](resource-packs.md) guide for more on supplying assets and the texture key contract.

> [!IMPORTANT]
> `Guid.NewGuid().ToString()` is for demonstration purposes. You will want to have a static UUID at runtime for your pack otherwise Minecraft will see every new version of your pack as a completely different pack because the UUIDs change.

## Full Example

The example project contains a complete item that uses both food and block placer traits:

```csharp
public class LasagnaItem : Item, IFood, IBlockPlacer { ... }
```

See `LasagnaItem.cs` in the [`ingot.Example`](../../ingot.Example) project.

## Tips & Gotchas

- `Texture` is required and is the only abstract member besides `Identifier`.
- `DisplayName` defaults to the raw identifier - always override it for player-facing items.
- Many traits have required (`abstract`) properties. Leaving them unimplemented will result in `null` or empty values and compile-time warnings.
- Block placer items (`IBlockPlacer`) are a very common pattern when you also have a custom block.
- For durability items, you usually combine `IDurability`, `IDamage` (or weapon traits), and optionally `IDigger`.
- Item traits are only discovered on the exact type passed to `AddItem<T>`. You can use a base item class and have derived classes add more traits.
- The generated item JSON always includes `minecraft:icon`, `minecraft:display_name`, `minecraft:max_stack_size`, and `minecraft:allow_off_hand` even if you left the defaults.

For blocks that these items place, see the [Blocks documentation](block.md). To craft items in a crafting table, see [Recipes](recipe.md). For Script API event handlers and services, see [Item Events](item-events.md) and [Script Services](script-services.md).