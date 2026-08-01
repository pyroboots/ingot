# Trait Attributes

**ingot** uses a series of C# attributes under the hood mark and specify certain behaviours of traits at compile time. These consist of:

- `[Trait(string identifier, TraitSystem.TraitType constraint)]`
- `[TraitProperty]`
- `[TraitFormatVersion("x.y.z")]`
- `[IngotExclude]`
- `[IngotOverride(value)]`
- `[TraitPropertyConstraint(...)]`
- `[TraitPropertyWarning(...)]`
- `[CompileHooks(typeof(MyHooks))]`

`IngotExclude`, `IngotOverride` and `CompileHooks` are the most common.

## `Trait` Attribute

The `Trait` attribute is placed on the trait interface. It declares the component name and its valid content type.

**Format:** `[Trait(string identifier, TraitSystem.TraitType constraint)]`

```csharp
// IChestObstruction serialises to the minecraft:chest_obstruction json component
// only valid on blocks (TraitSystem.TraitType.Block)
[Trait("minecraft:chest_obstruction", TraitSystem.TraitType.Block)]
public interface IChestObstruction : IBlockTrait
{
    // ...
}
```

> [!NOTE]
> Traits also inherit from their respective content type's trait interface, in this example it being `IBlockTrait` for blocks. Items use `IItemTrait` and entities use `IEntityTrait`. These interfaces act as tags during reflection.

## `TraitProperty` Attribute

The `TraitProperty` attribute is placed on properties inside a trait interface. It marks them to be serialised at reflection time.

**Format**: `[TraitProperty]`

```csharp
// will be serialised
[TraitProperty]
public virtual Vector3 Origin => new Vector3([-8, 0, -8]);

// will be serialised
[TraitProperty]
public virtual Vector3 Size => new Vector3([16, 24, 16]);

// will NOT serialised
public virtual string SomeRandomMember => "pyroboots.vercel.app";
```

Members without the `TraitProperty` attribute will **not** be serialised at compile time.

## `TraitFormatVersion` Attribute

The `TraitFormatVersion` attribute is placed on a trait **interface** to declare its minimum working format version. Reflection **throws** if a content instance that uses this trait has a `FormatVersion` lower than the minimum declared by this attribute (see [Format version requirements](trait-system.md#format-version-requirements)).

**Format:** `[TraitFormatVersion("x.y.z")]`

```csharp
[Trait("minecraft:bundle_interaction", TraitSystem.TraitType.Item)]
[TraitFormatVersion("1.21.40")]
public interface IBundleInteraction : IItemTrait
{
    [TraitProperty]
    public virtual int NumViewableSlots => 12;
}

public class CustomBundle : Item, IBundleInteraction
{
    // ...
    public override FormatVersion => new Version(1,20,0);
    // ...
}
```

An exception will be thrown at compile time here because `CustomBundle.FormatVersion` is lower than the minimum of `IBundleInteraction`.

## `IngotExclude` Attribute

The `IngotExclude` attribute is used on content declaration properties to skip serialising them to JSON at compile time.

**Format:** `[IngotExclude]`

```csharp
// minified version of example projects LasagnaItem for sake of demonstration
public class LasagnaItem : Item, IFood
{
    public override Version FormatVersion => new(1, 26, 0);
    public override Identifier Identifier => new("test:lasagna");
    public override string Texture => "suspicious_stew";
    public override string DisplayName => "Bowl of Lasagna";

    // serialised
    int IFood.Nutrition => 5;
    // serialised
    float IFood.SaturationModifier => 0.9f;

    // NOT serialised
    [IngotExclude]
    string IFood.UsingConvertsTo => "minecraft:bowl";
}
```

This can be useful when required to implement a property that is not required in Minecraft - usually from incorrectly generated traits.

## `IngotOverride` Attribute

The `IngotOverride` attribute is used on content declaration properties to override their value, regardless of type.

**Format:** `[IngotOverride(value)]`

```csharp
[Trait("minecraft:geometry", TraitSystem.TraitType.Block)]
public interface IGeometry : IBlockTrait
{
    // ...

    // can be a string, string array, or bool
    [TraitProperty]
    public abstract string UvLock { get; }
}

public class DenseLasagnaBlock : Block, IDestructibleByMining, IGeometry
{
    public override Identifier Identifier => new("test:block_of_dense_lasagna");
    public override string DisplayName => "Block of Dense Lasagna";
    public override string? Geometry => "minecraft:geometry.full_block";
    public override string? Sound => "shroomlight";

    // ...

    // overrides this to true
    [IngotOverride(true)]
    string IGeometry.UvLock => null; // this value will be ignored, so it doesnt matter what you set it as - usually null
}
```

Useful when a trait property can accept multiple types, but only currently accepts one.

## `TraitPropertyConstraint` Attribute

The `TraitPropertyConstraint` attribute is placed on properties inside a trait interface. It specifies certain constraints that overrides of that property must meet. Constraints are checked at compile time, and if one is not met, an exception will be thrown.

**Format:** `[TraitPropertyConstraint(TraitPropertyConstraintAttribute.Constraint operator, params object[] values)]`

```csharp
[Trait("minecraft:use_animation", TraitSystem.TraitType.Item)]
public interface IUseAnimation : IItemTrait
{
    [TraitProperty]
    [TraitPropertyConstraint(TraitPropertyConstraintAttribute.Constraint.OneOf,
        "eat", 
        "drink",
        "bow", // broken
        "block", // broken
        "camera", // broken
        "crossbow", // broken
        "none", // broken
        "brush",
        "spear",
        "spyglass"
    )]
    public abstract string Value { get; }
}
```

There are multiple different operators that can be used to define constraints:

- `NotEqual` - Value must not equal any of the items in the `values` array.
- `GreaterThan` - Numeric value must be strictly greater than every entry in the `values` array.
- `GreaterThanEq` - Numeric value must be strictly greater or equal to the first entry in the `values` array.
- `LessThan` - Numeric value must be strictly less than every entry in the `values` array.
- `LessThanEq` - Numeric value must be strictly less or equal to the first entry in the `values` array.
- `OneOf` - Value must be one of the values in the `values` array.
- `Range` - Value must be between the first and second item in the `values` array.

The same operators apply to the `TraitPropertyWarning` attribute.

## `TraitPropertyWarning` Attribute

The `TraitPropertyWarning` attribute is placed on properties inside a trait interface. It specifies certain conditions, that if met, will emit a warning to the compilation log.

**Format:** `[TraitPropertyWarning(string warning, TraitPropertyConstraintAttribute.Constraint operator, params object[] values)]`

```csharp
[Trait("minecraft:use_animation", TraitSystem.TraitType.Item)]
public interface IUseAnimation : IItemTrait
{
    [TraitProperty]
    [TraitPropertyWarning("animation '{x}' is broken and will display an incorrect animation", TraitPropertyConstraintAttribute.Constraint.OneOf, 
        "bow",
        "block",
        "camera",
        "crossbow",
        "none"
    )]
    public abstract string Value { get; }
}
```

## `CompileHooks` Attribute

Used to specify events to fire upon certain compilation events. See the dedicated [Compile hooks](compile-hooks.md) page for more details.