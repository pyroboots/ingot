using ingot.Core.Behaviour.Block;

using Newtonsoft.Json;

namespace ingot.Core.Common.SharedConstructs;

/// <summary>
/// References a block type
/// </summary>
[JsonConverter(typeof(CompilableFragmentJsonConverter<BlockTypeDescriptor>))]
public class BlockTypeDescriptor : ICompilableFragment
{
    /// <summary>
    /// Identifier of the block
    /// </summary>
    public required Identifier Name;
    
    /// <summary>
    /// Required states to qualify this permutation
    /// </summary>
    public Dictionary<Identifier, Either<int, float, string, bool>>? States;
    
    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        JsonHelper json = new(ref writer);

        if (States is not null)
        {
            json.Object("", () =>
            {
                json.Property("name", Name);
                json.Object("states", () =>
                {
                    foreach (var state in States)
                        json.Property(state.Key, state.Value.Value);
                });
            });   
        }
        else writer.WriteValue(Name);
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