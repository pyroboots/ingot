using ingot.Core.Common;
using ingot.Core.Scripting;

namespace ingot.Core.Behaviour;

/// <summary>
/// Autogenerates Script API bindings for item events.
/// </summary>
public struct ItemEvents : IScriptEvents
{
    /// <summary>Called when an item containing this component is hitting an entity and about to take durability damage.</summary>
    public ScriptHandler? BeforeDurabilityDamageEvent;

    /// <summary>Called when the use duration of the item containing this component is completed.</summary>
    public ScriptHandler? CompleteUseEvent;

    /// <summary>Called when an item containing this component is eaten by an entity.</summary>
    public ScriptHandler? ConsumeEvent;

    /// <summary>Called when an item containing this component is used to hit another entity.</summary>
    public ScriptHandler? HitEntityEvent;

    /// <summary>Called when an item containing this component is used to mine a block.</summary>
    public ScriptHandler? MineBlockEvent;

    /// <summary>Called when an item containing this component is used by a player.</summary>
    public ScriptHandler? UseEvent;

    /// <summary>Called when an item containing this component is used on a block.</summary>
    public ScriptHandler? UseOnEvent;

    /// <inheritdoc/>
    public bool HasEvents => Bindings.Count > 0;

    /// <inheritdoc/>
    public IReadOnlyList<ScriptEventBinding> Bindings
    {
        get
        {
            List<ScriptEventBinding> bindings = new();
            AddBinding(bindings, "onBeforeDurabilityDamage", BeforeDurabilityDamageEvent);
            AddBinding(bindings, "onCompleteUse", CompleteUseEvent);
            AddBinding(bindings, "onConsume", ConsumeEvent);
            AddBinding(bindings, "onHitEntity", HitEntityEvent);
            AddBinding(bindings, "onMineBlock", MineBlockEvent);
            AddBinding(bindings, "onUse", UseEvent);
            AddBinding(bindings, "onUseOn", UseOnEvent);
            return bindings;
        }
    }

    /// <inheritdoc/>
    public string GetScriptPath(Identifier id) =>
        $"scripts/items/{id.Namespace}_{id.Name}_events.js";

    /// <inheritdoc/>
    public string GetJsonComponentName(Identifier id) =>
        ScriptEventsGenerator.GetJsonComponentName(id, ScriptComponentKind.Item);

    /// <inheritdoc/>
    public ScriptComponentKind ComponentKind => ScriptComponentKind.Item;

    private static void AddBinding(List<ScriptEventBinding> bindings, string scriptApiEvent, ScriptHandler? handler)
    {
        if (handler is { IsConfigured: true } configured)
            bindings.Add(new ScriptEventBinding(scriptApiEvent, configured));
    }
}