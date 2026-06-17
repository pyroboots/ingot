using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

namespace ingot.Tests.Content.Blocks;

internal class MultiFaceTestBlock : Block
{
    public override Identifier Identifier => new("test:multi_face_block");

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("multi_face_side", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png")),
        Up = new MaterialInstance("multi_face_top", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_item.png"))
    };
}