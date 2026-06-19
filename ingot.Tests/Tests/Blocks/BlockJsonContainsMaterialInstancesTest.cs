using ingot.Core.Behaviour.Block;
using ingot.Tests.Content;

namespace ingot.Tests.Blocks;

public class BlockJsonContainsMaterialInstancesTest
{
    [Fact]
    public void Compile_BlockJsonContainsMaterialInstances()
    {
        string json = Block.Compile(typeof(TestBlock));
        Assert.Contains("minecraft:material_instances", json);
        Assert.Contains("\"texture\": \"test_block\"", json);
        Assert.Contains("\"render_method\": \"opaque\"", json);
    }
}