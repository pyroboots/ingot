using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

namespace ingot.Tests.Content.Blocks;

internal class PermutationTestBlock : Block
{
    public override Identifier Identifier => new("test:permutation_block");

    public override BlockPermutation[] Permutations => [new GlowyTestPermutation()];

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("permutation_block", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
    };
}