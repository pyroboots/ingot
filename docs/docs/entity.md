# Making an Entity

Entities are defined by deriving from the abstract `Entity` class in `ingot.Core.Behaviour.Entity`. Like blocks and items, entities use the [trait system](trait-system.md) for component definitions. ingot also provides C# types for [component groups](entity-component-groups.md) and [events](entity-events.md).

## Minimal Entity

```csharp
using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

public class MyEntity : Entity
{
    public override Identifier Identifier => new("mynamespace:my_entity");
}
```

Every entity **must** implement:

- `Identifier` - the full `namespace:name` string used in Minecraft.

## Key Properties

| Property           | Type                                      | Default     | Description |
|--------------------|-------------------------------------------|-------------|-------------|
| `FormatVersion`    | `Version`                                 | `"1.20.10"` | Target format version. |
| `IsSpawnable`      | `bool`                                    | `false`     | Whether the entity can spawn naturally in the world. |
| `IsSummonable`     | `bool`                                    | `true`      | Whether the entity can be summoned with commands. |
| `IsExperimental`   | `bool`                                    | `false`     | Whether the entity requires experimental gameplay. |
| `ComponentGroups`  | `EntityComponentGroup[]`                  | `[]`        | Named component sets toggled by events. See [Entity Component Groups](entity-component-groups.md). |
| `Events`           | `Dictionary<Identifier, IEntityEventAction[]>` | `{}`     | Event definitions and their actions. See [Entity Events](entity-events.md). |

These are written into the `description`, `component_groups`, `components`, and `events` sections of the generated entity JSON.

## Compiled Output

A minimal entity compiles to:

```json
{
    "format_version": "1.20.10",
    "minecraft:entity": {
        "description": {
            "identifier": "mynamespace:my_entity",
            "is_spawnable": false,
            "is_summonable": true,
            "is_experimental": false
        },
        "component_groups": {},
        "components": {},
        "events": {}
    }
}
```

Override `IsSpawnable`, `IsSummonable`, or `IsExperimental` on your entity class when you need different values.

## Component Groups

Component groups let you attach different sets of traits to an entity at runtime. See the dedicated [Entity Component Groups](entity-component-groups.md) guide.

## Entity Events

Events add or remove component groups, run commands, emit particles, and more. See the dedicated [Entity Events](entity-events.md) guide.

## Adding Behavior with Traits

Entity traits use the same pattern as blocks and items. Implement `IEntityTrait` interfaces on your entity class or on `EntityComponentGroup` subclasses:

```csharp
using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Entity;

public class MyEntity : Entity, IHealth, ITypeFamily
{
    public override Identifier Identifier => new("mynamespace:my_entity");

    int IHealth.Max => 20;
    dynamic ITypeFamily.Family => "mob";
}
```

### Behaviour Presets

For common mob archetypes, ingot provides preset interfaces that bundle the typical components for a mob type. See the [Trait System - Entity Traits and Behaviour Presets](trait-system.md#entity-traits-and-behaviour-presets) section for the full list.

```csharp
public class LasagnaSpiritEntity : Entity, IEntityBehaviourPresetFlying
{
    public override Identifier Identifier => new("test:lasagna_spirit");

    dynamic ITypeFamily.Family => "lasagna";
    int IHealth.Max => 20;
    dynamic IDespawn.DespawnFromDistance => null;
    EntityFilter IDespawn.Filters => null;

    float IMovement.Max => 6;
    float IMovement.Value => 3;
    string[] INavigationFly.BlocksToAvoid => [];
    float IBehaviorFloatWander.FloatDuration => 6f;
}
```

Presets compose many individual traits. You can still add extra traits beyond what a preset provides, or implement traits one at a time without using a preset.

See the [Entity Traits API reference](https://pyroboots.github.io/ingot/api/ingot.Core.TraitSystem.Traits.Entity.html) for the complete list.

## Raw JSON Entities

If you already have hand-authored entity JSON, derive from `JsonEntity` instead of `Entity`:

```csharp
using ingot.Core.Behaviour;
using ingot.Core.Common;

public class HandAuthoredEntity : JsonEntity
{
    public override Identifier Identifier => new("mynamespace:custom_entity");
    protected override string Json => LoadJson("path/to/entity.json");
}
```

`JsonEntity` bypasses trait and event compilation and writes your JSON verbatim.

## Compilation & Registration

Register entities with `Pack.Create`:

```csharp
using ingot.Core;

const string packUuid = "77f1fef2-bb39-411a-b25c-ae475c21169f";

Pack pack = Pack.Create(packUuid, "My Addon", "Entities made with ingot")
    .AddEntity<MyEntity>();

pack.Compile("./output");
```

Capture identifiers from your entity class when you need them for cross-references elsewhere in your project.

This writes `bp/entities/my_entity.json` (filename is the part after the `:` in the identifier).

## Current Limitations

- Entity resource support (models, textures, render controllers) is not yet available on the `ResourcePack` side.

## Full Example

See `LasagnaSpiritEntity.cs` in the [`ingot.Example`](../../ingot.Example) project.

Next: learn about [entity component groups](entity-component-groups.md) and [entity events](entity-events.md).