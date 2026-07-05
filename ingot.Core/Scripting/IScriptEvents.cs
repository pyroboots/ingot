using ingot.Core.Common;

namespace ingot.Core.Scripting;

/// <summary>
/// Script API custom component event bindings for blocks or items.
/// </summary>
public interface IScriptEvents
{
    /// <summary>Configured event handlers.</summary>
    IReadOnlyList<ScriptEventBinding> Bindings { get; }

    /// <summary>Whether any event handlers are configured.</summary>
    bool HasEvents { get; }

    /// <summary>Behaviour-pack relative path for the generated script file.</summary>
    string GetScriptPath(Identifier id);

    /// <summary>Custom component id written into block or item JSON.</summary>
    string GetJsonComponentName(Identifier id);

    /// <summary>Whether this binding targets block or item component registries.</summary>
    ScriptComponentKind ComponentKind { get; }
}

/// <summary>
/// Target registry for generated Script API custom components.
/// </summary>
public enum ScriptComponentKind
{
    /// <summary>Block custom components.</summary>
    Block,
    /// <summary>Item custom components.</summary>
    Item,
}