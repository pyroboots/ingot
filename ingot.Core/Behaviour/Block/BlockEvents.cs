using ingot.Core.Common;
using ingot.Core.Scripting;

namespace ingot.Core.Behaviour.Block;

/// <summary>
/// Autogenerates Script API bindings for block events.
/// </summary>
public struct BlockEvents : IScriptEvents
{
    /// <summary>Called when an entity falls on the block.</summary>
    public ScriptHandler? EntityFallOnEvent;

    /// <summary>Called when the block is placed or snow-logged.</summary>
    public ScriptHandler? OnPlaceEvent;

    /// <summary>Called when the player breaks the block.</summary>
    public ScriptHandler? PlayerBreakEvent;

    /// <summary>Called when the player interacts with / uses the block.</summary>
    public ScriptHandler? PlayerInteractEvent;

    /// <summary>Called before a player places the block, preventing the client-side placement of the block.</summary>
    public ScriptHandler? PlayerPlaceBeforeEvent;

    /// <summary>Triggered on every random tick, allowing for behaviour like random crop growth.</summary>
    public ScriptHandler? RandomTickEvent;

    /// <summary>Triggers every time the block receives a redstone update.</summary>
    public ScriptHandler? RedstoneUpdateEvent;

    /// <summary>Called when an entity steps off the block.</summary>
    public ScriptHandler? StepOffEvent;

    /// <summary>Called when an entity steps onto the block.</summary>
    public ScriptHandler? StepOnEvent;

    /// <summary>Triggers between ticks inside the block's minecraft:tick component interval range.</summary>
    public ScriptHandler? TickEvent;

    /// <summary>Called whenever the block permutation is changed to another permutation of the same block type.</summary>
    public ScriptHandler? BlockStateChangeEvent;

    /// <summary>Called when an entity executes an event on the block.</summary>
    public ScriptHandler? EntityEvent;

    /// <inheritdoc/>
    public bool HasEvents => Bindings.Count > 0;

    /// <inheritdoc/>
    public IReadOnlyList<ScriptEventBinding> Bindings
    {
        get
        {
            List<ScriptEventBinding> bindings = new();
            AddBinding(bindings, "onEntityFallOn", EntityFallOnEvent);
            AddBinding(bindings, "onPlace", OnPlaceEvent);
            AddBinding(bindings, "onPlayerBreak", PlayerBreakEvent);
            AddBinding(bindings, "onPlayerInteract", PlayerInteractEvent);
            AddBinding(bindings, "beforeOnPlayerPlace", PlayerPlaceBeforeEvent);
            AddBinding(bindings, "onRandomTick", RandomTickEvent);
            AddBinding(bindings, "onRedstoneUpdate", RedstoneUpdateEvent);
            AddBinding(bindings, "onStepOff", StepOffEvent);
            AddBinding(bindings, "onStepOn", StepOnEvent);
            AddBinding(bindings, "onTick", TickEvent);
            AddBinding(bindings, "onBlockStateChange", BlockStateChangeEvent);
            AddBinding(bindings, "onEntity", EntityEvent);
            return bindings;
        }
    }

    /// <inheritdoc/>
    public string GetScriptPath(Identifier id) =>
        $"scripts/blocks/{id.Namespace}_{id.Name}_events.js";

    /// <inheritdoc/>
    public string GetJsonComponentName(Identifier id) =>
        ScriptEventsGenerator.GetJsonComponentName(id, ScriptComponentKind.Block);

    /// <inheritdoc/>
    public ScriptComponentKind ComponentKind => ScriptComponentKind.Block;

    private static void AddBinding(List<ScriptEventBinding> bindings, string scriptApiEvent, ScriptHandler? handler)
    {
        if (handler is { IsConfigured: true } configured)
            bindings.Add(new ScriptEventBinding(scriptApiEvent, configured));
    }
}