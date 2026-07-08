namespace ingot.Core.Common;

/// <summary>
/// Container class for common enums
/// </summary>
public abstract class Enums
{
    /// <summary>
    /// Converts <typeparamref name="TEnum"/> to its Minecraft string equivalent
    /// </summary>
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
    /// Converts <see cref="InventorySlot"/> to its Minecraft string equivalent
    /// </summary>
    public static string InventorySlot_AsString(InventorySlot slot)
        => $"slot.{(AsString(slot) == "mainhand" || AsString(slot) == "offhand" ? "weapon" : "armor")}.{AsString(slot)}";

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
    /// <summary>
    /// Converts <see cref="Target"/> to its Minecraft string equivalent
    /// </summary>
    public static string Target_AsString(Target selector) => AsString(selector);
    
    /// <summary>
    /// Creative inventory tabs
    /// </summary>
    public enum CatalogueCategory
    {
        /// <summary>
        /// Construction tab
        /// </summary>
        Construction,
        /// <summary>
        /// Nature tabs
        /// </summary>
        Nature,
        /// <summary>
        /// Equipment tab
        /// </summary>
        Equipment,
        /// <summary>
        /// Items tab
        /// </summary>
        Items,
        /// <summary>
        /// Will not appear in the creative inventory
        /// </summary>
        None
    }
    /// <summary>
    /// Converts <see cref="CatalogueCategory"/> to its Minecraft string equivalent
    /// </summary>
    public static string CatalogueCategory_AsString(Target selector) => AsString(selector);
}