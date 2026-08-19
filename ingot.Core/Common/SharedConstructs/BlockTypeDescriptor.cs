using ingot.Core.Behaviour.Block;

using Newtonsoft.Json;

namespace ingot.Core.Common.SharedConstructs;

/// <summary>
/// References a block type
/// </summary>
public class BlockTypeDescriptor : ICompilableFragment
{
    /// <summary>
    /// Identifier of the block
    /// </summary>
    public required Identifier Name;
    
    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        // name-only block descriptors compile as a bare identifier string
        writer.WriteValue(Name.ToString());
    }

    /// <summary>
    /// <see cref="BlockTypeDescriptor"/> -> <see cref="string"/>
    /// </summary>
    public static implicit operator string(BlockTypeDescriptor descriptor) => descriptor.Name.ToString();
    /// <summary>
    /// <see cref="string"/> -> <see cref="BlockTypeDescriptor"/>
    /// </summary>
    public static implicit operator BlockTypeDescriptor(string str) => new() { Name = str };
    /// <summary>
    /// <see cref="Block"/> -> <see cref="BlockTypeDescriptor"/>
    /// </summary>
    public static implicit operator BlockTypeDescriptor(Block block) => new() { Name = block.Identifier };
}