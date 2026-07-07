# Entity Component Groups

Component groups are the entity equivalent of [block permutations](block-permutations.md). Each group is a named set of components that can be added or removed at runtime through [entity events](entity-events.md).

## Creating a Component Group

Derive from `EntityComponentGroup`:

```csharp
using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

public class AdultGroup : EntityComponentGroup
{
    public override Identifier Identifier => new("mynamespace:adult");
    public override Entity Parent => new MyEntity();
}
```

Every component group **must** implement:

- `Identifier` - the group name used in events and JSON.
- `Parent` - the owning `Entity` instance (same pattern as `BlockPermutation.Parent`).

## Registering Component Groups

Return groups from the `ComponentGroups` property on your entity:

```csharp
public class MyEntity : Entity
{
    public override Identifier Identifier => new("mynamespace:my_entity");
    public override EntityComponentGroup[] ComponentGroups => [new AdultGroup()];
}
```

See [Making an Entity](entity.md) for the full entity property reference.

## Adding Traits to Groups

Any [entity trait](trait-system.md) implemented on the group class is compiled into that group's `components` object - not the entity's root `components` object:

```csharp
using ingot.Core.TraitSystem.Traits.Entity;

public class AngryGroup : EntityComponentGroup, IHealth, IAttack
{
    public override Identifier Identifier => new("mynamespace:angry");
    public override Entity Parent => new MyEntity();

    int IHealth.Max => 30;
    FloatRange IAttack.Damage => new() { RangeMin = 3, RangeMax = 6 };
    string IAttack.EffectName => "weakness";
}
```

This is useful for stateful mobs - a peaceful base entity with an "angry" group that adds combat behaviours, switched on by an event.

## Compiled Output

A component group is written into the `component_groups` section of the entity JSON:

```json
"component_groups": {
    "mynamespace:adult": {
        "minecraft:health": {
            "max": 20
        }
    }
}
```

Groups are inert until an [entity event](entity-events.md) adds or removes them at runtime.

## Full Example

See `LasagnaSpiritEntityAngry` in `LasagnaSpiritEntity.cs` in the [`ingot.Example`](../../ingot.Example) project - an angry component group with hostile flying preset traits, toggled by a `minecraft:entity_spawned` event.

## See Also

- [Making an Entity](entity.md) - base entity properties and compilation
- [Entity Events](entity-events.md) - add and remove groups at runtime
- [Trait System](trait-system.md) - entity traits and behaviour presets
- [Block Permutations](block-permutations.md) - the block-side equivalent