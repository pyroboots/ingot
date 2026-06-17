using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Loot;
using ingot.Core.Common;
using ingot.Tests.Content.Loot;

namespace ingot.Tests.Content.Blocks;

internal class LootTableTestBlock : Block
{
    public override Identifier Identifier => new("test:loot_block");
    public override LootTable? Loot => new TestBlockLootTable();

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("loot_block", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
    };
}