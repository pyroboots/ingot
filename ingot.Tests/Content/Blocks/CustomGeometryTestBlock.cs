using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

namespace ingot.Tests.Content.Blocks;

internal class CustomGeometryTestBlock : Block
{
    public override Identifier Identifier => new("test:custom_geometry_block");

    public override string? Geometry => "geometry.test_block";

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("test_block", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
    };
}