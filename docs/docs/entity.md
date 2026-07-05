# Making an Entity

Entities in ingot are defined by deriving from the abstract `Entity` class in `ingot.Core.Behaviour`. Entity support is still early — compilation produces a minimal `minecraft:entity` JSON shell with a `description` object, but no components or traits yet.

## Minimal Entity

```csharp
using ingot.Core.Behaviour;
using ingot.Core.Common;

public class MyEntity : Entity
{
    public override Identifier Identifier => new("mynamespace:my_entity");
}
```

Every entity **must** implement:

- `Identifier` - the full `namespace:name` string used in Minecraft.

## Key Properties

| Property        | Type      | Default     | Description |
|-----------------|-----------|-------------|-------------|
| `FormatVersion` | `Version` | `"1.20.10"` | Target format version. |
| `IsSpawnable`   | `bool`    | `false`     | Whether the entity can spawn naturally in the world. |
| `IsSummonable`  | `bool`    | `true`      | Whether the entity can be summoned with commands. |

## Compiled Output

A minimal entity compiles to:

```json
{
    "format_version": "1.20.10",
    "minecraft:entity": {
        "description": {
            "identifier": "mynamespace:my_entity",
            "is_spawnable": false,
            "is_summonable": true
        }
    }
}
```

Override `IsSpawnable` or `IsSummonable` on your entity class when you need different values.

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

- No entity traits are generated or compiled yet. See the [Trait System](trait-system.md) page for the work-in-progress note on entity traits.
- The compiled JSON contains `format_version`, `description`, and spawn flags only — no gameplay components yet.
- Entity resource support (models, textures, render controllers) is not yet available on the `ResourcePack` side.

As entity support expands, this page will be updated with trait usage, components, and resource pack integration.

## Full Example

See `LasagnaSpiritEntity.cs` in the [`ingot.Example`](../../ingot.Example) project.