# Your First Entity

In this step, you'll create a **Dirtling** - a small passive mob with behaviour components, client-side visuals, and a coloured spawn egg.

Entities are split into two sides:

| Side | Class | Pack | Responsibility |
|------|-------|------|----------------|
| Behaviour | `Entity` | `bp/` | Health, AI, physics, spawn flags |
| Client | `ClientEntity` / `ClientEntity<T>` | `rp/` | Materials, textures, geometry, spawn egg, sounds |

## Behaviour Entity

In `Content/Entities/`, create `DirtlingEntity.cs` and inherit from `Entity`.

The only required member is `Identifier`. For a walking passive mob, implement `IEntityPresetPassive` - a preset that stacks common land-mob traits (health, movement, navigation, float, random stroll, look around, and more) so you only fill in the abstract bits.

```cs
using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Entity;

namespace MyAddon.Content.Entities;

public class DirtlingEntity : Entity, IEntityPresetPassive
{
    public override Identifier Identifier => "myaddon:dirtling";
    public override bool IsSpawnable => true;
    public override bool IsSummonable => true;

    // client entities can be discovered automatically if nested
    public override Type? ClientEntityType => typeof(Client);

    // abstract members from the passive preset
    dynamic ITypeFamily.Family => new[] { "dirtling", "mob" };
    int IHealth.Max => 8;
    float IMovement.Value => 0.2f;

    float ICollisionBox.Width => 0.6f;
    float ICollisionBox.Height => 0.6f;
}
```

What those mean:

| Override / trait | Purpose |
|------------------|---------|
| `IsSpawnable` | Allows natural/spawn rules to use the entity (and enables spawn egg use patterns) |
| `IsSummonable` | Allows `/summon myaddon:dirtling` |
| `ITypeFamily.Family` | Entity families used by filters and targeting |
| `IHealth.Max` | Max health |
| `IMovement.Value` | Walk speed |
| `ICollisionBox` | Hitbox size (defaults exist; override for a smaller mob) |

> [!TIP]
> Presets such as `IEntityPresetPassive`, `IEntityPresetHostile`, and `IEntityPresetPassiveLand` live in `ingot.Core.Behaviour.Entity`. Start simple with `IEntityPresetPassive`; grow into component groups and events when you need babies, stages, or interactions - see [Entity Component Groups](../entity/entity-component-groups.md) and [Entity Events](../entity/entity-events.md).

## Client Entity (Visuals)

Add a nested class `Client` that inherits `ClientEntity<DirtlingEntity>`. Nesting + `ClientEntityType` (or the conventional nested name `Client`) lets `Pack.AddEntity<DirtlingEntity>()` discover visuals automatically - no separate `AddClientEntity` call.

For a first pack, reuse **vanilla slime** materials/geometry/textures so you do not need a custom model:

```cs
public class Client : ClientEntity<DirtlingEntity>
{
    public override string DefaultMaterial => "slime";
    public override string DefaultTexture => "textures/entity/slime/slime";
    public override string DefaultGeometry => "geometry.slime";

    public override ClientEntitySpawnEgg? SpawnEgg => new()
    {
        BaseColor = "#6b4f2a",
        OverlayColor = "#3d2a14"
    };
}
```

| Property | Role |
|----------|------|
| `DefaultMaterial` | Short-name mapped under `materials.default` |
| `DefaultTexture` | Path-style texture reference (not an item atlas key) |
| `DefaultGeometry` | Geometry short-name |
| `SpawnEgg` | Colours (or texture) for the creative spawn egg |

If you later make a custom skin PNG, set:

```cs
public override string? DefaultTexturePath =>
    Path.Combine(AppContext.BaseDirectory, "Data", "dirtling.png");
```

and point `DefaultTexture` at something like `textures/entity/dirtling`. ingot copies the PNG into the resource pack during compile. You can also call `pack.AddEntityTexture(...)` manually.

> [!NOTE]
> Simple render controllers are emitted automatically when you do not customize them. For multi-variant controllers (climate skins, baby arrays, ...), see [Client Entities](../entity/client-entity.md) and the cow example in `ingot.Example`.

## Full Entity File

```cs
using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Entity;

namespace MyAddon.Content.Entities;

public class DirtlingEntity : Entity, IEntityPresetPassive
{
    public override Identifier Identifier => "myaddon:dirtling";
    public override bool IsSpawnable => true;
    public override bool IsSummonable => true;
    public override Type? ClientEntityType => typeof(Client);

    dynamic ITypeFamily.Family => new[] { "dirtling", "mob" };
    int IHealth.Max => 8;
    float IMovement.Value => 0.2f;

    float ICollisionBox.Width => 0.6f;
    float ICollisionBox.Height => 0.6f;

    // Resource-pack client entity - auto-discovered by AddEntity<DirtlingEntity>()
    public class Client : ClientEntity<DirtlingEntity>
    {
        public override string DefaultMaterial => "slime";
        public override string DefaultTexture => "textures/entity/slime/slime";
        public override string DefaultGeometry => "geometry.slime";

        public override ClientEntitySpawnEgg? SpawnEgg => new()
        {
            BaseColor = "#6b4f2a",
            OverlayColor = "#3d2a14"
        };
    }
}
```

## Optional: Death Loot

To drop Dirt Soup when a Dirtling dies, create an entity loot table and implement `ILoot` on the behaviour entity (or on a component group):

```cs
// Compact sketch - see Loot Tables docs for full pools
string ILoot.Table => "loot_tables/entities/dirtling.json";
```

You would register that table with `pack.AddLootTable<DirtlingLoot>()` because entity loot is not auto-wired the same way as `Block.Loot`. For this tutorial, spawn eggs and summoning are enough.

## What You Have So Far

| Piece | Type | Identifier |
|-------|------|------------|
| `DirtSoupItem` | Item | `myaddon:dirt_soup` |
| `DirtSoupRecipe` | Recipe | `myaddon:dirt_soup` |
| `CompactDirtBlock` | Block | `myaddon:compact_dirt` |
| `CompactDirtLoot` | Loot table | (auto via block) |
| `DirtlingEntity` + `Client` | Entity + client entity | `myaddon:dirtling` |

**Next:** [4. Compile Your Pack](compile.md) - register everything, add textures, and write the packs.

**Also see:** [Making an Entity](../entity/entity.md), [Client Entities](../entity/client-entity.md)
