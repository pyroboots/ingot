using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

namespace ingot.Tests.Content.Blocks;

internal class TaggedTestBlock : Block
{
    public override Identifier Identifier => new("test:tagged_block");
    public override string DisplayName => "Tagged Block";
    public override string[] Tags => ["stone", "metal"];

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("tagged_block", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
    };
}