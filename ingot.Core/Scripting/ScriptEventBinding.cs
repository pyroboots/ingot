namespace ingot.Core.Scripting;

/// <summary>
/// Maps a Script API custom component event to a handler body.
/// </summary>
/// <param name="ScriptApiEvent">Script API event name, for example <c>onPlace</c>.</param>
/// <param name="Handler">Handler body to insert into the generated component.</param>
public readonly record struct ScriptEventBinding(string ScriptApiEvent, ScriptHandler Handler);