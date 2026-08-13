using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

namespace ingot.Tests.Content.Blocks;

internal class StatefulTestBlock : Block
{
    public override Identifier Identifier => new("test:stateful_block");

    public override Dictionary<Identifier, dynamic[]> States => new()
    {
        { "test:powered", [true, false] }
    };

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("stateful_block", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
    };
}