using ingot.Core.Behaviour.Block;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Block;

namespace ingot.Tests.Content.Blocks;

internal class DestructibleTestBlock : Block, IDestructibleByMining
{
    public override Identifier Identifier => new("test:destructible_block");

    dynamic? IDestructibleByMining.ItemSpecificSpeeds => null;
    float IDestructibleByMining.SecondsToDestroy => 1.5f;

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("destructible_block", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
    };
}