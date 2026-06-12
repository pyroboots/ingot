using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits.Block;

namespace ingot.Example;

public class DenseLasagnaBlock : Block, ILightEmission
{
    public override string Identifier => "test:block_of_dense_lasagna";
    public override List<BlockPermutation> Permutations => new()
    {
        new DenseLasagnaGlowyPermutation()
    };

    public override Dictionary<string, dynamic[]> States => new()
    {
        { "test:radioactive", [true, false] }
    };

    int ILightEmission.Value => 0;
}

public class DenseLasagnaGlowyPermutation : BlockPermutation, ILightEmission
{
    public override string Condition => "q.get_block_state('test:radioactive') == true";
    int ILightEmission.Value => 7;
}