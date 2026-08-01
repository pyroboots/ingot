namespace ingot.Core.Scripting;

/// <summary>
/// A Script API <c>/scriptevent</c> handler registered on a <see cref="Pack"/>.
/// </summary>
/// <param name="EventId">Minecraft script event id, e.g. <c>mynamespace:hello</c>.</param>
/// <param name="RelativePath">Behaviour-pack relative output path.</param>
/// <param name="Handler">Handler body to run when the event is received.</param>
internal readonly record struct ScriptEventRegistration(
    string EventId,
    string RelativePath,
    ScriptHandler Handler);
