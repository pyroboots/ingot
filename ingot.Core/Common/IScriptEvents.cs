namespace ingot.Core.Common;

/// <summary>
/// Internal use interface that implements base for Script API bind autogeneration
/// </summary>
public interface IScriptEvents
{
    /// <summary>
    /// Array of possible events to check for null when calling <see cref="HasEvents"/>
    /// </summary>
    private protected object?[] Events { get; }
    /// <summary>
    /// Whether any block event handlers are configured.
    /// </summary>
    public bool HasEvents => Events.Any(e => e is not null);

    /// <summary>
    /// Behaviour-pack relative path for the generated script file
    /// </summary>
    /// <param name="id">Identifier of content to generate for</param>
    public string GetScriptPath(Identifier id);
    
    /// <summary>
    /// Generates the Script API component registration script and JSON component name
    /// </summary>
    /// <param name="id">Identifier of content to generate for</param>
    /// <returns>JSON component name and generated JavaScript source</returns>
    public (string jsonComponentName, string code) Compile(Identifier id);
}