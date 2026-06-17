using ingot.Core.Behaviour.Block;
using ingot.Tests.Content;

namespace ingot.Tests.Tests.Blocks;

public class BlockJsonContainsIdentifierTest
{
    [Fact]
    public void Compile_blockJsonContainsIdentifier()
    {
        string json = Block.Compile(typeof(TestBlock));
        Assert.Contains("\"identifier\": \"test:test_block\"", json);
    }
}