using ingot.Core.Common;

using Newtonsoft.Json;

namespace ingot.Core.TraitSystem.Traits.Item;

/// <summary>
/// Determines which items can be used to repair the item, as well as the amount of durability specified items will repair.
/// </summary>
public interface IRepairable
{
    /// <summary>
    /// Object containing valid repair items and their repair amounts
    /// </summary>
    /// <param name="Items">Items that repair by <see cref="RepairAmount"/></param>
    /// <param name="RepairAmount">Molang expression or integer to repair by</param>
    public record RepairItem(Identifier[] Items, dynamic RepairAmount) : ICompilableFragment
    {
        /// <inheritdoc/>
        public void Compile(ref JsonTextWriter writer)
        {
            if (RepairAmount is not string or int)
                throw new InvalidCastException("repair amount must be a Molang expression or an integer");
            
            JsonHelper json = new(ref writer);
            json.Object("", () =>
            {
                json.Array("items", () =>
                {
                    foreach (Identifier item in Items)
                        item.Compile(ref json.Writer);
                });
                json.Property("repair_amount", RepairAmount);
            });
        }
    }
    
    /// <summary>
    /// Array of items that can be used to repair
    /// </summary>
    [TraitProperty]
    public abstract RepairItem[] RepairItems { get; }
}