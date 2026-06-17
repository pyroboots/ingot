using ingot.Core.Behaviour.Block;
using ingot.Tests.Content.Blocks;

namespace ingot.Tests.Tests.Blocks;

public class BlockJsonContainsTagsTest
{
    [Fact]
    public void Compile_blockJsonContainsTags()
    {
        string json = Block.Compile(typeof(TaggedTestBlock));
        Assert.Contains("tag:stone", json);
        Assert.Contains("tag:metal", json);
        Assert.Contains("\"minecraft:display_name\": \"Tagged Block\"", json);
    }
}