using ingot.Core.Behaviour.Block;
using ingot.Tests.Content.Blocks;

namespace ingot.Tests.Blocks;

public class BlockJsonContainsPermutationTest
{
    [Fact]
    public void Compile_BlockJsonContainsPermutation()
    {
        string json = Block.Compile(typeof(PermutationTestBlock));
        Assert.Contains("\"permutations\"", json);
        Assert.Contains("query.block_state('test:lit') == true", json);
        Assert.Contains("\"minecraft:light_emission\": 10", json);
    }
}