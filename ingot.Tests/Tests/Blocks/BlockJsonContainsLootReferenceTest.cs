using ingot.Core.Behaviour.Block;
using ingot.Tests.Content.Blocks;

namespace ingot.Tests.Blocks;

public class BlockJsonContainsLootReferenceTest
{
    [Fact]
    public void Compile_BlockJsonContainsLootReference()
    {
        string json = Block.Compile(typeof(LootTableTestBlock));
        Assert.Contains("minecraft:loot", json);
        Assert.Contains("loot_tables/blocks/loot_block.json", json);
    }
}