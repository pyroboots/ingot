using ingot.Core.Behaviour.Block;

using Newtonsoft.Json;

namespace ingot.Core.Common.SharedConstructs;

/// <summary>
/// Used to reference a permutation of a block
/// </summary>
public class BlockPermutationDescriptor : ICompilableFragment
{
    /// <summary>
    /// Identifier of the block
    /// </summary>
    public required Identifier Name;
    
    /// <summary>
    /// Required states to qualify this permutation
    /// </summary>
    public required Dictionary<Identifier, object[]> States;
    
    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Property("name", Name);
            json.Object("states", () =>
            {
                foreach (var kvp in States)
                    json.Property(kvp.Key, kvp.Value);
            });
        });
    }
}