using ingot.Core.Behaviour.Block;
using ingot.Tests.Content.Blocks;

namespace ingot.Tests.Blocks;

public class BlockJsonContainsStatesTest
{
    [Fact]
    public void Compile_BlockJsonContainsStates()
    {
        string json = Block.Compile(typeof(StatefulTestBlock));
        Assert.Contains("test:powered", json);
        Assert.Contains("true", json);
        Assert.Contains("false", json);
    }
}