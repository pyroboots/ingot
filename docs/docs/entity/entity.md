# Making an Entity

Entities are defined by deriving from the abstract `Entity` class in `ingot.Core.Behaviour.Entity`. Like blocks and items, entities use the [trait system](../advanced/trait-system.md) for component definitions. ingot also provides C# types for [component groups](entity-component-groups.md) and [events](entity-events.md).

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
| `RuntimeIdentifier`| `Identifier?`                             | `null`      | Optional vanilla id to imitate hard-coded engine behavior. |
| `Properties`       | `Dictionary<Identifier, IEntityProperty>` | `{}`        | Typed entity properties (server-side state, optional client sync). See [Entity Properties](#entity-properties). |
| `ComponentGroups`  | `EntityComponentGroup[]`                  | `[]`        | Named component sets toggled by events. See [Entity Component Groups](entity-component-groups.md). |
| `Events`           | `Dictionary<Identifier, IEntityEventAction[]>` | `{}`     | Event definitions and their actions. See [Entity Events](entity-events.md). |
| `ClientEntityType` | `Type?`                                   | `null`      | Optional explicit client-entity type for resource-pack visuals. When null, `Pack.AddEntity` may discover a nested `Client` type or matching `ClientEntity<T>` in the same assembly. See [Client Entities](client-entity.md). |
| `DynamicTraits`    | `Trait[]`                                 | `[]`        | Hand-built `Trait` components for identifiers without a generated trait interface. See [Dynamic Traits](../advanced/trait-system.md#dynamic-traits). |
| `Singles`          | `Dictionary<Identifier, object>`          | `{}`        | Components written as a single scalar value instead of an object body (`"namespace:comp": value`). See [Singles](../advanced/trait-system.md#singles). |

These are written into the `description` (including `properties`), `component_groups`, `components`, and `events` sections of the generated entity JSON.

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

## Entity Properties

Entity properties store typed state on an entity without a full component, similar to block states. They compile into `minecraft:entity` / `description` / `properties`.

Override `Properties` with a dictionary of `Identifier` to `IEntityProperty`. Four implementations are available:

| Class | JSON `type` | Fields | Notes |
|-------|-------------|--------|--------|
| `BooleanEntityProperty` | `bool` | `Default` | `true` / `false` |
| `EnumEntityProperty` | `enum` | `Values`, `Default` | `Default` must be one of `Values` or compile throws |
| `FloatEntityProperty` | `float` | `Min`, `Max`, `Default` | Emits `"range": [min, max]`; default must be in range |
| `IntEntityProperty` | `int` | `Min`, `Max`, `Default` | Same range rules as float |

Every property also has `ClientSync` (default `true`), which controls whether clients can read the value.

```csharp
using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

public class MyEntity : Entity
{
    public override Identifier Identifier => new("mynamespace:my_entity");

    public override Dictionary<Identifier, IEntityProperty> Properties => new()
    {
        [new("mynamespace:is_charged")] = new BooleanEntityProperty
        {
            Default = false,
        },
        [new("mynamespace:mood")] = new EnumEntityProperty
        {
            Values = ["calm", "alert", "angry"],
            Default = "calm",
            ClientSync = true,
        },
        [new("mynamespace:power")] = new FloatEntityProperty
        {
            Min = 0f,
            Max = 1f,
            Default = 0.5f,
        },
        [new("mynamespace:level")] = new IntEntityProperty
        {
            Min = 0,
            Max = 10,
            Default = 1,
            ClientSync = false,
        },
    };
}
```

Compiles to (property block only):

```json
"properties": {
    "mynamespace:is_charged": {
        "type": "bool",
        "default": false,
        "client_sync": true
    },
    "mynamespace:mood": {
        "type": "enum",
        "values": ["calm", "alert", "angry"],
        "default": "calm",
        "client_sync": true
    },
    "mynamespace:power": {
        "type": "float",
        "range": [0.0, 1.0],
        "default": 0.5,
        "client_sync": true
    },
    "mynamespace:level": {
        "type": "int",
        "range": [0, 10],
        "default": 1,
        "client_sync": false
    }
}
```

> [!CAUTION]
> Validation runs at compile time and **throws** on invalid property definitions:
>
> - `EnumEntityProperty`: `Default` not listed in `Values` throws `InvalidEnumArgumentException`
> - `FloatEntityProperty` / `IntEntityProperty`: `Default` outside `[Min, Max]` throws `ArgumentOutOfRangeException`

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

For common mob archetypes, ingot provides preset interfaces that bundle the typical components for a mob type. See the [Trait System - Entity Traits and Behaviour Presets](../advanced/trait-system.md#entity-traits-and-behaviour-presets) section for the full list.

```csharp
public class LasagnaSpiritEntity : Entity, IEntityPresetFlying
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
    .AddEntity<MyEntity>(); // discovers ClientEntityType / nested Client / matching ClientEntity

pack.Compile("./output");
```

To register a pre-configured instance (including `JsonEntity`), use `pack.BehaviourPack.AddEntityFromInstance(inst)`. Direct compile helpers are `Entity.Compile(Type)`, `Entity.Compile<T>()`, and `Entity.CompileFromInstance(inst)` (JsonEntity instances are written as raw JSON).

This writes `bp/entities/my_entity.json` (filename is the part after the `:` in the identifier).

By default `AddEntity<T>()` also discovers a matching [client entity](client-entity.md) and nested render controllers. Pass `discoverClient: false` to register behaviour only. Details: [Client Entities - Auto-discovery](client-entity.md#auto-discovery-from-the-behaviour-entity).

## Write less: presets, events, groups

### Behaviour presets

Prefer presets over listing dozens of traits. `IEntityPresetPassiveLand` bundles walk navigation, panic/tempt/breed goals, leash/balloon/nameable, lava hurt, and sensible AI priorities. Override only what differs:

```csharp
public class CowEntity : Entity, IEntityPresetPassiveLand, IEntityPresetSameSpeciesOffspring
{
    public override Identifier Identifier => new("test", "custom_cow");
    string IEntityPresetSameSpeciesOffspring.SpeciesId => Identifier.ToString();
    dynamic ITypeFamily.Family => new[] { "cow", "mob" };
    int IHealth.Max => 10;
    float IMovement.Value => 0.25f;
    string[] IBehaviorTempt.Items => ["wheat"];
}
```

> [!NOTE]
> Optional trait properties default to `null` and are omitted from JSON - you no longer need `=> null!` stubs for unused fields.

### Event helpers

Use `EntityEvents` instead of hand-building nested actions:

```csharp
public override Dictionary<Identifier, IEntityEventAction[]> Events => EntityEvents.Map(
    (Identifier.Vanilla("entity_spawned"),
        EntityEvents.SpawnedAdultOrBaby(95f, 5f, "test:spawn_adult", Baby.Id)),
    (Identifier.Vanilla("ageable_grow_up"), EntityEvents.GrowUp(Baby.Id, Adult.Id)),
    (new Identifier("test", "spawn_adult"), [EntityEvents.Add(Adult.Id)])
);
```

### Component groups

Prefer `EntityComponentGroup<TParent>` so `Parent` is inferred. Nested types under the entity keep groups co-located:

```csharp
public class Baby : EntityComponentGroup<CowEntity>, IIsBaby, IScale, IAgeable
{
    public static Identifier Id { get; } = new("test", "custom_cow_baby");
    public override Identifier Identifier => Id;
    float IScale.Value => 0.5f;
    // ...
}
```

## Client Entities & Visuals

Behaviour entities only define gameplay. Materials, textures, geometry, render controllers, and entity sounds live on a separate `ClientEntity` in the resource pack.

See the dedicated [Client Entities & Render Controllers](client-entity.md) guide.

## Full Example

See [`CowEntity.cs`](https://github.com/pyroboots/ingot/blob/master/ingot.Example/Entities/CowEntity.cs) in `ingot.Example`: presets + event DSL + nested `Baby`/`Adult`/`Client`, `EntitySounds.FromVanilla("cow")`, and `CowV3RenderController`.

Next: [entity component groups](entity-component-groups.md), [entity events](entity-events.md), and [client entities](client-entity.md).
