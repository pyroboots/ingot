namespace ingot.Core.Common;

/// <summary>
/// Internal use class to implement a Minecraft identifier
/// </summary>
public class Identifiable
{
    /// <summary>
    /// Minecraft identifier
    /// </summary>
    public Identifier Identifier { get; }
    public Identifiable(string identifier) => Identifier = new(identifier);
    public Identifiable(Identifier identifier) => Identifier = identifier;
}