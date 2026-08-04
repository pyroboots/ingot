using ingot.Core.Behaviour.Item;

using Newtonsoft.Json;

namespace ingot.Core.Common.SharedConstructs;

/// <summary>
/// References an item type
/// </summary>
public class ItemTypeDescriptor : ICompilableFragment
{
    /// <summary>
    /// Identifier of the item
    /// </summary>
    public required Identifier Name;
    
    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        // name-only item descriptors compile as a bare identifier string
        writer.WriteValue(Name.ToString());
    }

    /// <summary>
    /// <see cref="ItemTypeDescriptor"/> -> <see cref="string"/>
    /// </summary>
    public static implicit operator string(ItemTypeDescriptor descriptor) => descriptor.Name.ToString();
    /// <summary>
    /// <see cref="string"/> -> <see cref="ItemTypeDescriptor"/>
    /// </summary>
    public static implicit operator ItemTypeDescriptor(string str) => new() { Name = str };
    /// <summary>
    /// <see cref="Item"/> -> <see cref="ItemTypeDescriptor"/>
    /// </summary>
    public static implicit operator ItemTypeDescriptor(Item item) => new() { Name = item.Identifier };
}