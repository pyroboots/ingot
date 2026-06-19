using ingot.Core.Behaviour.Block;
using ingot.Tests.Content.Blocks;

namespace ingot.Tests.Blocks;

public class BlockJsonContainsDestructibleTraitTest
{
    [Fact]
    public void Compile_BlockJsonContainsDestructibleTrait()
    {
        string json = Block.Compile(typeof(DestructibleTestBlock));
        Assert.Contains("minecraft:destructible_by_mining", json);
        Assert.Contains("\"seconds_to_destroy\": 1.5", json);
    }
}