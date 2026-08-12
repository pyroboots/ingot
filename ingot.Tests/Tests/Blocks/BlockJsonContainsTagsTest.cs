using ingot.Core.Behaviour.Block;
using ingot.Tests.Content.Blocks;

namespace ingot.Tests.Blocks;

public class BlockJsonContainsTagsTest
{
    [Fact]
    public void Compile_BlockJsonContainsTags()
    {
        string json = Block.Compile(typeof(TaggedTestBlock));
        Assert.Contains("minecraft:tags", json);
        Assert.Contains("stone", json);
        Assert.Contains("metal", json);
        Assert.Contains("\"minecraft:display_name\": \"Tagged Block\"", json);
    }
}