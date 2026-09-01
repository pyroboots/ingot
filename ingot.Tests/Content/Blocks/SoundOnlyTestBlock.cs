using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

namespace ingot.Tests.Content.Blocks;

internal class SoundOnlyTestBlock : Block
{
    public override Identifier Identifier => new("test:sound_only_block");

    public override string? Sound => "copper";

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("test_block", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
    };
}
