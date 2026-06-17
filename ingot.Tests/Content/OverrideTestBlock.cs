using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

namespace ingot.Tests.Content;

internal class OverrideTestBlock : Block
{
    public override Identifier Identifier => new("test:override_block");

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("override_block", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("auto.png"))
    };
}