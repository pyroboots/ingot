namespace ingot.Core.Common;

/// <summary>
/// Container class for common enums
/// </summary>
public abstract class Enums
{
    /// <summary>
    /// Returns the enum as its typical lowercase name
    /// </summary>
    /// <param name="value">Enumeration value</param>
    /// <typeparam name="TEnum">Enumeration</typeparam>
    public static string AsString<TEnum>(TEnum value) =>
        Formatting.PascalToSnakeCase(Enum.GetName(typeof(TEnum), value));
    
    /// <summary>
    /// Enumeration of valid inventory slots
    /// </summary>
    public enum InventorySlot
    {
        /// <summary>Equates to <c>slot.armor.body</c></summary>
        Body,
        /// <summary>Equates to <c>slot.armor.chest</c></summary>
        Chest,
        /// <summary>Equates to <c>slot.armor.feet</c></summary>
        Feet,
        /// <summary>Equates to <c>slot.armor.head</c></summary>
        Head,
        /// <summary>Equates to <c>slot.armor.legs</c></summary>
        Legs,
        /// <summary>Equates to <c>slot.weapon.mainhand</c></summary>
        Mainhand,
        /// <summary>Equates to <c>slot.weapon.offhand</c></summary>
        Offhand
    }

    /// <summary>
    /// Enumeration of target selectors
    /// </summary>
    public enum Target
    {
        /// <summary>Equates to <c>self</c></summary>
        Self,
        /// <summary>Equates to <c>other</c></summary>
        Other,
        /// <summary>Equates to <c>target</c></summary>
        Target,
        /// <summary>Equates to <c>baby</c></summary>
        Baby,
        /// <summary>Equates to <c>parent</c></summary>
        Parent,
        /// <summary>Equates to <c>holder</c></summary>
        Holder,
        /// <summary>Equates to <c>block</c></summary>
        Block,
        /// <summary>Equates to <c>damager</c></summary>
        Damager,
        /// <summary>Equates to <c>player</c></summary>
        Player,
    }
}