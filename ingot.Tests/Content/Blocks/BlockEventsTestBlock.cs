using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

namespace ingot.Tests.Content.Blocks;

internal class BlockEventsTestBlock : Block
{
    public override Identifier Identifier => new("test:events_block");

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("events_block", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
    };

    public override BlockEvents? BlockEvents => new()
    {
        OnPlaceEvent = "event.block.setType('minecraft:stone');"
    };
}