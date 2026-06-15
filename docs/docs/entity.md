# Entities

Entities in ingot are defined by deriving from the abstract `Entity` class in `ingot.Core.Behaviour`. Entity support is still early — compilation produces a minimal `minecraft:entity` JSON shell with no components or traits yet.

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

## Compilation & Registration

Register entities with a `BehaviourPack`:

```csharp
using ingot.Core;

BehaviourPack bp = BehaviourPack.Create(Guid.NewGuid().ToString())
    .AddEntity<MyEntity>();

Pack pack = new()
{
    Name = "My Addon",
    Description = "Entities made with ingot",
    BehaviourPack = bp,
    ResourcePack = ResourcePack.Create(Guid.NewGuid().ToString()),
    LinkPacks = true
};

pack.Compile("./output");
```

This writes `bp/entities/my_entity.json` (filename is the part after the `:` in the identifier).

## Current Limitations

- No entity traits are generated or compiled yet. See the [Trait System](trait-system.md) page for the work-in-progress note on entity traits.
- The compiled JSON contains only `format_version` and an empty `minecraft:entity` object.
- Entity resource support (models, textures, render controllers) is not yet available on the `ResourcePack` side.

As entity support expands, this page will be updated with trait usage, components, and resource pack integration.