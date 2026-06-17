using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

namespace ingot.Tests.Content.Blocks;

internal class VanillaTextureTestBlock : Block
{
    public override Identifier Identifier => new("test:vanilla_texture_block");

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("minecraft:stone", MaterialInstance.RenderMethods.Opaque)
    };
}