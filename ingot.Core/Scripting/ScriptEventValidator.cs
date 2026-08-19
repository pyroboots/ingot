using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Item;
using ingot.Core.TraitSystem.Traits.Block;
using ingot.Core.TraitSystem.Traits.Item;

using Newtonsoft.Json;

namespace ingot.Core.Scripting;

/// <summary>
/// Emits compile-time warnings when Script API events lack required vanilla traits.
/// </summary>
internal static class ScriptEventValidator
{
    private static readonly (string EventName, Type Trait)[] BlockTraitRequirements =
    [
        ("onEntityFallOn", typeof(IEntityFallOn)),
        ("onRedstoneUpdate", typeof(IRedstoneConsumer)),
        ("onTick", typeof(ITick)),
    ];

    private static readonly string[] BlockCollisionEvents =
    [
        "onEntityFallOn",
        "onStepOff",
        "onStepOn",
    ];

    private static readonly (string EventName, Type Trait)[] ItemTraitRequirements =
    [
        ("onCompleteUse", typeof(IUseModifiers)),
        ("onConsume", typeof(IFood)),
    ];

    /// <summary>Validates block event bindings against implemented traits.</summary>
    public static void ValidateBlock(Type blockType, BlockEvents events, ref JsonWriter? writer)
    {
        foreach (ScriptEventBinding binding in events.Bindings)
        {
            foreach ((string eventName, Type trait) in BlockTraitRequirements)
            {
                if (binding.ScriptApiEvent != eventName)
                    continue;

                if (!trait.IsAssignableFrom(blockType))
                {
                    CompilerState.Warn(
                        ref writer,
                        $"block event {eventName} on {blockType.Name} requires trait {trait.Name} to fire in-game");
                }
            }

            if (BlockCollisionEvents.Contains(binding.ScriptApiEvent) && !typeof(ICollisionBox).IsAssignableFrom(blockType))
            {
                CompilerState.Warn(
                    ref writer,
                    $"block event {binding.ScriptApiEvent} on {blockType.Name} requires trait ICollisionBox to fire in-game");
            }
        }

    }

    /// <summary>Validates item event bindings against implemented traits.</summary>
    public static void ValidateItem(Type itemType, ItemEvents events, ref JsonWriter? writer)
    {
        foreach (ScriptEventBinding binding in events.Bindings)
        {
            foreach ((string eventName, Type trait) in ItemTraitRequirements)
            {
                if (binding.ScriptApiEvent != eventName)
                    continue;

                if (!trait.IsAssignableFrom(itemType))
                {
                    CompilerState.Warn(
                        ref writer,
                        $"item event {eventName} on {itemType.Name} requires trait {trait.Name} to fire in-game");
                }
            }
        }

        if (events.Bindings.Any(b => b.ScriptApiEvent == "onConsume") && !typeof(IUseModifiers).IsAssignableFrom(itemType))
        {
            CompilerState.Warn(
                ref writer,
                $"item event onConsume on {itemType.Name} also requires trait IUseModifiers to fire in-game");
        }
    }
}