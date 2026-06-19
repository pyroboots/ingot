using ingot.Core.Behaviour.Block;
using ingot.Tests.Content;

namespace ingot.Tests.Blocks;

public class BlockJsonContainsIdentifierTest
{
    [Fact]
    public void Compile_BlockJsonContainsIdentifier()
    {
        string json = Block.Compile(typeof(TestBlock));
        Assert.Contains("\"identifier\": \"test:test_block\"", json);
    }
}