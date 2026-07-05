using ingot.Core.Behaviour.Block;
using ingot.Core.Common;
using ingot.Tests.Support;

namespace ingot.Tests.Content.Blocks;

internal class BlockEventsMissingTraitTestBlock : Block
{
    public override Identifier Identifier => new("test:missing_trait_block");

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("missing_trait_block", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
    };

    public override BlockEvents? BlockEvents => new()
    {
        TickEvent = "event.block.setType('minecraft:stone');",
    };
}