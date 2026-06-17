using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

namespace ingot.Tests.Content;

internal class TestBlock : Block
{
    public override Identifier Identifier => new("test:test_block");

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("test_block", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
    };
}