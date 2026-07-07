# Making an Entity

Entities are defined by deriving from the abstract `Entity` class in `ingot.Core.Behaviour.Entity`. Like blocks and items, entities use the [trait system](trait-system.md) for component definitions, and ingot also provides C# types for `component_groups` and `events`.

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
| `ComponentGroups`  | `EntityComponentGroup[]`                  | `[]`        | Named component sets toggled by events. |
| `Events`           | `Dictionary<Identifier, IEntityEventAction[]>` | `{}`     | Event definitions and their actions. |

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

Component groups are the entity equivalent of [block permutations](block-permutations.md). Each group is a named set of components that can be added or removed at runtime through events.

Derive from `EntityComponentGroup`:

```csharp
using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

public class AdultGroup : EntityComponentGroup
{
    public override Identifier Identifier => new("mynamespace:adult");
}
```

Register the group on your entity:

```csharp
public class MyEntity : Entity
{
    public override Identifier Identifier => new("mynamespace:my_entity");
    public override EntityComponentGroup[] ComponentGroups => [new AdultGroup()];
}
```

Any [entity trait](trait-system.md) implemented on the group class is compiled into that group's `components` object.

## Entity Events

Events are defined on the entity as a dictionary keyed by event name. Each event contains one or more actions.

```csharp
using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

public class MyEntity : Entity
{
    public override Identifier Identifier => new("mynamespace:my_entity");
    public override Dictionary<Identifier, IEntityEventAction[]> Events => new()
    {
        [new("mynamespace:grow_up")] =
        [
            new ComponentGroupAddEntityEventAction
            {
                ComponentGroups = [new Identifier("mynamespace:adult")]
            }
        ]
    };
}
```

### Built-in Event Actions

ingot ships with C# types for common Bedrock event actions:

| Type | JSON key | Purpose |
|------|----------|---------|
| `ComponentGroupAddEntityEventAction` | `add` | Add one or more component groups. |
| `ComponentGroupRemoveEntityEventAction` | `remove` | Remove one or more component groups. |
| `SequenceEntityEventAction` | `sequence` | Run actions in order. |
| `RandomizeEntityEventAction` | `randomize` | Pick from a weighted pool of action sets. |
| `DropItemEntityEventAction` | `drop_item` | Drop an item from an inventory slot. |
| `EmitParticleEntityEventAction` | `emit_particle` | Emit a particle effect. |
| `EmitVibrationEntityEventAction` | `emit_vibration` | Emit a sculk vibration. |
| `QueueCommandEntityEventAction` | `queue_command` | Queue one or more commands. |

Each action type can be instantiated directly — create an instance and populate its properties.

### Sequence and Randomize

Use `SequenceEntityEventAction` when multiple steps must run in order:

```csharp
public override Dictionary<Identifier, IEntityEventAction[]> Events => new()
{
    [new("minecraft:entity_spawned")] =
    [
        new SequenceEntityEventAction
        {
            EventActions =
            [
                new ComponentGroupAddEntityEventAction
                {
                    ComponentGroups = [new Identifier("mynamespace:adult")]
                },
                new ComponentGroupAddEntityEventAction
                {
                    ComponentGroups = [new Identifier("mynamespace:baby")]
                }
            ]
        }
    ]
};
```

Use `RandomizeEntityEventAction` for weighted outcomes:

```csharp
new RandomizeEntityEventAction
{
    EventActions =
    [
        new(80, [
            new ComponentGroupAddEntityEventAction
            {
                ComponentGroups = [new Identifier("mynamespace:white")]
            }
        ]),
        new(20, [
            new ComponentGroupAddEntityEventAction
            {
                ComponentGroups = [new Identifier("mynamespace:black")]
            }
        ])
    ]
};
```

## Adding Behavior with Traits

Entity traits use the same pattern as blocks and items. Implement `IEntityTrait` interfaces on your entity class or on `EntityComponentGroup` subclasses:

```csharp
public class MyEntity : Entity, ISomeEntityTrait
{
    public override Identifier Identifier => new("mynamespace:my_entity");

    // ISomeEntityTrait properties...
}
```

Entity trait generation is still in progress. See the [Trait System](trait-system.md) page for the current status.

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

- Entity trait generation is still a work in progress — see [Trait System](trait-system.md).
- Not all Bedrock event action types are modelled yet (`trigger`, `filters`, `first_valid`, etc.).
- Entity resource support (models, textures, render controllers) is not yet available on the `ResourcePack` side.
- Duplicate sibling action keys in a single event (e.g. two `add` blocks) are not merged at compile time yet.

## Full Example

See `LasagnaSpiritEntity.cs` in the [`ingot.Example`](../../ingot.Example) project.