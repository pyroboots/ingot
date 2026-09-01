using ingot.Core.Behaviour.Block;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Block;

using Version = ingot.Core.Common.Version;

namespace ingot.Tests.Content.Blocks;

internal class DestructibleTestBlock : Block, IDestructibleByMining
{
    public override Version FormatVersion => new(1, 26, 20);

    public override Identifier Identifier => new("test:destructible_block");
    
    float IDestructibleByMining.SecondsToDestroy => 1.5f;

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("destructible_block", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
    };
}