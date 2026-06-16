using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Loot;
using ingot.Core.Common;

namespace ingot.Example;

public class DenseLasagnaBlock : Block
{
    public override Identifier Identifier => new("test:block_of_dense_lasagna");
    public override LootTable? Loot => new DenseLasagnaLoot();
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
    public override Block Parent => new DenseLasagnaBlock();

    public override int? LightEmission => 7;
}