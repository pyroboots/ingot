using ingot.Core.Behaviour.Block;
using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits;

namespace ingot.Example;

public class DenseLasagnaBlock : Block
{
    public override string Identifier => "test:block_of_dense_lasagna";
    public override List<BlockPermutation> Permutations => new()
    {
        new DenseLasagnaGlowyPermutation()
    };

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("block_of_dense_lasagna", MaterialInstance.RenderMethods.AlphaTest)
    };

    public override Dictionary<string, dynamic[]> States => new()
    {
        { "test:radioactive", [true, false] }
    };
}

public class DenseLasagnaGlowyPermutation : BlockPermutation
{
    public override string Condition => "q.get_block_state('test:radioactive') == true";
    
    public override int? LightEmission => 7;
}