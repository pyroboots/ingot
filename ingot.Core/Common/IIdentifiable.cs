namespace ingot.Core.Common;

/// <summary>
/// Internal use interface to implement a Minecraft identifier
/// </summary>
public interface IIdentifiable
{
    /// <summary>
    /// Minecraft identifier
    /// </summary>
    public abstract Identifier Identifier { get; }
}