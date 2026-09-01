using ingot.Core;
using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

namespace ingot.Tests.Content.Blocks;

internal class GlowyTestPermutation : BlockPermutation
{
    public override Molang Condition => new Molang().BlockState("test:lit").Eq(true);
    public override Block Parent => new PermutationTestBlock();
    public override int? LightEmission => 10;

    public override MaterialInstances? MaterialInstances => new()
    {
        All = new MaterialInstance("glowy_variant", MaterialInstance.RenderMethods.Blend, FixturePaths.Resolve("auto.png"))
    };
}