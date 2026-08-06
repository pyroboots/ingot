# Adding Scripts

**ingot** can generate Minecraft Bedrock **Script API** custom components and pack-level scripts from C# registrations. You write short JavaScript **handler bodies**; the compiler wraps them, wires JSON components, and builds `bp/scripts/main.js`.

This step is optional. Items, blocks, and entities from earlier steps already work without scripts.

## Enable Scripts

Before compile, set:

```cs
pack.ScriptsEnabled = true;
```

> [!IMPORTANT]
> Without `ScriptsEnabled = true`, block/item event handlers, services, and script events are skipped and **ingot** emits compile-time warnings.

Default module map includes `@minecraft/server` (version `2.8.0`). Adjust `pack.ScriptApiModules` if you need other versions or modules.

## Block Events

Attach handlers on the block via `BlockEvents`. Bodies can be **inline strings** or **files** under `scripts/blocks/`.

### File-based handler

Create `scripts/blocks/compact_dirt_on_place.js` (handler body only - no imports required for `event`):

```javascript
event.dimension.playSound("dig.grass", event.block.location);
```

On `CompactDirtBlock`:

```cs
using ingot.Core.Scripting;

public override BlockEvents? BlockEvents => new()
{
    OnPlaceEvent = ScriptHandler.FromFile(
        Path.Combine(AppContext.BaseDirectory, "scripts", "blocks", "compact_dirt_on_place.js"))
};
```

### Inline handler

```cs
public override BlockEvents? BlockEvents => new()
{
    PlayerInteractEvent = ScriptHandler.Inline(
        """
        event.player.sendMessage("Compact dirt feels... dense.");
        """)
};
```

Common block events include `OnPlaceEvent`, `PlayerInteractEvent`, `PlayerBreakEvent`, and step-on / random-tick style hooks. Full list: [Block Events](../block/block-events.md).

## Item Events

Same pattern on items with `ItemEvents`:

```cs
// on DirtSoupItem
public override ItemEvents? ItemEvents => new()
{
    CompleteUseEvent = ScriptHandler.Inline(
        """
        event.source.sendMessage("That tasted like... dirt.");
        """)
};
```

Or from file:

```cs
UseEvent = ScriptHandler.FromFile(
    Path.Combine(AppContext.BaseDirectory, "scripts", "items", "dirt_soup_use.js"))
```

See [Item Events](../item/item-events.md) for the full event list and trait validation notes.

## Services (Tick Scripts)

**Services** run on a recurring interval for global logic (action bars, scans, timers). Register them on the pack:

```cs
pack.ScriptsEnabled = true;

string scriptsDir = Path.Combine(AppContext.BaseDirectory, "scripts");
pack.AddService(
    Path.Combine(scriptsDir, "services", "tick_service.js"),
    intervalTicks: 40);
```

`scripts/services/tick_service.js` is again a **body only**. **ingot** wraps it in `system.runInterval` and imports `system` / `world` for you:

```javascript
for (const player of world.getAllPlayers())
    player.onScreenDisplay.setActionBar("My Addon is loaded");
```

| Parameter | Default | Meaning |
|-----------|---------|---------|
| `sourceFile` | (required) | Path to the JS body |
| `name` | file name | Output name under `bp/scripts/services/` |
| `intervalTicks` | `1` | Ticks between runs (use `20` for once per second) |

## Script Events (`/scriptevent`)

Bind a handler to a custom script event id:

```cs
pack.AddScriptEvent("myaddon:hello", """
    world.sendMessage(event.message);
    """);
```

In-game (cheats enabled):

```
/scriptevent myaddon:hello Hello from dirt world
```

Details: [Script Services and Events](../script-services.md).

## Updated Program.cs Fragment

```cs
Pack pack = Pack.Create(packUuid, "My Addon", "Dirt soup pack")
    .AddItem<DirtSoupItem>()
    .AddBlock<CompactDirtBlock>()
    .AddEntity<DirtlingEntity>()
    .AddRecipe<DirtSoupRecipe>();

pack.ScriptsEnabled = true;

string scriptsDir = Path.Combine(AppContext.BaseDirectory, "scripts");
pack.AddService(Path.Combine(scriptsDir, "services", "tick_service.js"), intervalTicks: 40);

pack.Compile("./output");
```

Ensure your `.csproj` still copies `scripts/**` to the output directory (from [Setup](setup.md)).

## What Gets Generated

When scripts are enabled and at least one script exists, compile produces roughly:

```
bp/scripts/
  main.js                 # imports all handlers
  blocks/...              # custom block components
  items/...               # custom item components
  services/...            # runInterval wrappers
  events/...              # scriptevent subscriptions
bp/manifest.json          # script module dependency when needed
```

Block/item JSON gains the matching custom component entries automatically.

## Debugging Tips

1. Enable the in-game **Content Log** and watch for script errors.
2. Read `ingot.log` for "scripts disabled" or missing-file warnings.
3. Remember handler files are **bodies**, not full modules - do not wrap them in `system.runInterval` yourself for services.
4. Rebuild after every script edit (`dotnet run`); packs are not hot-reloaded by the compiler.

**Next:** [6. Advanced Compiling](advanced-compiling.md)
