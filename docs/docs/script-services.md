# Script Services

**Services** are Script API JavaScript files that run continuously while a world is loaded. They are intended for tick-based logic such as timers, global state, or world scans.

Register a service on your `Pack` before compiling:

```csharp
pack.ScriptsEnabled = true;
pack.AddService("./scripts/services/tick_service.js");
pack.Compile("./output");
```

The source file is copied to `bp/scripts/services/` and imported from the generated `bp/scripts/main.js` entry point.

## Writing a Service

A service is a normal Script API module. Use `system.runInterval` (or similar) for recurring logic:

```javascript
import { system, world } from "@minecraft/server";

system.runInterval(() => {
    for (const player of world.getAllPlayers())
        player.onScreenDisplay.setActionBar("Service running");
}, 20);
```

Services are not auto-wrapped by ingot. You own the full file contents.

## Services vs Block/Item Events

| Feature | Block/Item Events | Services |
|---------|-------------------|----------|
| Registration | `BlockEvents` / `ItemEvents` on content classes | `pack.AddService(...)` |
| Output path | `bp/scripts/blocks/` or `bp/scripts/items/` | `bp/scripts/services/` |
| Code generation | ingot generates component registration boilerplate | Source file copied as-is |
| Typical use | Per-block or per-item behaviour | Global tick logic |

## See Also

- [Block Events](block-events.md)
- [Item Events](item-events.md)
- [Resource Packs & Textures](resource-packs.md)