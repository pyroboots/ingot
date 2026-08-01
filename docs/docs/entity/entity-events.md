# Entity Events

Entity events let you change an entity's behaviour at runtime - most commonly by adding or removing [component groups](entity-component-groups.md). In ingot, events are defined on the entity as a dictionary keyed by event name, with each event containing one or more typed actions.

## Basic Usage

Override the `Events` property on your entity class:

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

Events are written into the `events` section of the generated entity JSON. See [Making an Entity](entity.md) for the full property reference.

## Built-in Event Actions

ingot ships with C# types for common Bedrock event actions:

| Type | JSON key | Purpose |
|------|----------|---------|
| `ComponentGroupAddEntityEventAction` | `add` | Add one or more component groups. |
| `ComponentGroupRemoveEntityEventAction` | `remove` | Remove one or more component groups. |
| `SequenceEntityEventAction` | `sequence` | Run actions in order. |
| `RandomizeEntityEventAction` | `randomize` | Pick from a weighted pool of action sets. |
| `TriggerEntityEventAction` | `trigger` | Fire another entity event (optionally targeting a filter). |
| `DropItemEntityEventAction` | `drop_item` | Drop an item from an inventory slot. |
| `EmitParticleEntityEventAction` | `emit_particle` | Emit a particle effect. |
| `EmitVibrationEntityEventAction` | `emit_vibration` | Emit a sculk vibration. |
| `QueueCommandEntityEventAction` | `queue_command` | Queue one or more commands. |

Each action type can be instantiated directly - create an instance and populate its properties.

> [!TIP]
> For common patterns (spawn adult/baby, grow up, add/remove/swap groups), prefer the `EntityEvents` helpers below instead of hand-building nested action types.

## `EntityEvents` helpers

`EntityEvents` is a static factory for the usual action graphs:

| Helper | Builds |
|--------|--------|
| `Add(groups...)` | `ComponentGroupAddEntityEventAction` |
| `Remove(groups...)` | `ComponentGroupRemoveEntityEventAction` (empty args emit an empty `remove` object) |
| `Swap(remove, add)` | remove then add (array or single-id overloads) |
| `Trigger(eventId, target?)` | `TriggerEntityEventAction` |
| `Sequence(steps...)` | `SequenceEntityEventAction` |
| `Randomize((weight, actions)...)` | `RandomizeEntityEventAction` |
| `SpawnedAdultOrBaby(adultW, babyW, spawnAdultEvent, babyGroup)` | common spawn randomize + trigger adult |
| `GrowUp(baby, adult)` | `Swap` baby to adult |
| `Map((id, actions)...)` | `Dictionary<Identifier, IEntityEventAction[]>` for `Entity.Events` |

```csharp
public override Dictionary<Identifier, IEntityEventAction[]> Events => EntityEvents.Map(
    (Identifier.Vanilla("entity_spawned"),
        EntityEvents.SpawnedAdultOrBaby(95f, 5f, "test:spawn_adult", Baby.Id)),
    (Identifier.Vanilla("ageable_grow_up"), EntityEvents.GrowUp(Baby.Id, Adult.Id)),
    (new Identifier("test", "spawn_adult"), [EntityEvents.Add(Adult.Id)])
);
```

For ageable `grow_up` object fields, use `EntityEventTargets.GrowUpSelf("minecraft:ageable_grow_up")`, which produces `{ "event": "...", "target": "self" }`.

See also [Write less: presets, events, groups](entity.md#write-less-presets-events-groups).

## Adding and Removing Component Groups

The most common event actions toggle [component groups](entity-component-groups.md):

```csharp
[new("mynamespace:calm_down")] =
[
    new ComponentGroupRemoveEntityEventAction
    {
        ComponentGroups = [new Identifier("mynamespace:angry")]
    }
],
[new("mynamespace:enrage")] =
[
    new ComponentGroupAddEntityEventAction
    {
        ComponentGroups = [new Identifier("mynamespace:angry")]
    }
]
```

## Sequence

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

## Randomize

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

Weights are relative - an entry with weight `80` is four times more likely than one with weight `20`.

## Other Actions

### Drop item

```csharp
new DropItemEntityEventAction
{
    Slot = Enums.InventorySlot.Mainhand
}
```

### Queue command

```csharp
new QueueCommandEntityEventAction
{
    Commands = ["say hello"],
    Target = Enums.Target.Other
}
```

### Emit particle

```csharp
new EmitParticleEntityEventAction
{
    Particle = new Identifier("minecraft:heart_particle")
}
```

### Emit vibration

```csharp
new EmitVibrationEntityEventAction
{
    Type = EmitVibrationEntityEventAction.VibrationType.EntityInteract
}
```

## Compiled Output

An event with a component group add action compiles to:

```json
"events": {
    "mynamespace:grow_up": {
        "add": {
            "component_groups": ["mynamespace:adult"]
        }
    }
}
```

## Current Limitations

> [!NOTE]
> Some Bedrock event constructs are not modelled yet (`filters`, `first_valid`, and a few rarer actions). The common set is covered: `add` / `remove` / `sequence` / `randomize` / `trigger` / `drop_item` / `emit_particle` / `emit_vibration` / `queue_command`.

> [!CAUTION]
> Duplicate sibling action keys in a single event (e.g. two `add` blocks) are not merged at compile time yet. Prefer a single action instance with multiple component groups, or wrap steps in `SequenceEntityEventAction`.

## Full Example

See [`CowEntity.cs`](https://github.com/pyroboots/ingot/blob/master/ingot.Example/Entities/CowEntity.cs) in the `ingot.Example` project - spawn adult/baby randomization and grow-up events built with the `EntityEvents` helpers.

## See Also

- [Making an Entity](entity.md) - base entity properties and compilation
- [Client Entities & Render Controllers](client-entity.md) - resource-pack visuals
- [Entity Component Groups](entity-component-groups.md) - named component sets toggled by events
- [Trait System](../trait-system.md) - entity traits and behaviour presets