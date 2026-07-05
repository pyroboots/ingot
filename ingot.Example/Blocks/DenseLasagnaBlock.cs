using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Loot;
using ingot.Core.Common;
using ingot.Core.Scripting;
using ingot.Core.TraitSystem.Traits.Block;

using Version = ingot.Core.Common.Version;

namespace ingot.Example.Blocks;

public class DenseLasagnaBlock : Block, IDestructibleByMining
{
    public override Version FormatVersion => new(1, 20, 80);
    public override Identifier Identifier => new("test:block_of_dense_lasagna");
    public override string DisplayName => "Block of Dense Lasagna";
    public override string? Geometry => "minecraft:geometry.full_block";
    public override string? Sound => "shroomlight";
    public override LootTable? Loot => new DenseLasagnaLoot();
    public override string[] Tags => ["minecraft:is_hoe_item_destructible"];

    dynamic? IDestructibleByMining.ItemSpecificSpeeds => null;
    float IDestructibleByMining.SecondsToDestroy => 2f;
    public override List<BlockPermutation> Permutations => new()
    {
        new DenseLasagnaGlowyPermutation()
    };

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("shroomlight", MaterialInstance.RenderMethods.AlphaTest)
    };

    public override Dictionary<string, dynamic[]> States => new()
    {
        { "test:radioactive", [false, true] }
    };

    public override BlockEvents? BlockEvents => new()
    {
        OnPlaceEvent = ScriptHandler.FromFile(Path.Combine(AppContext.BaseDirectory, "scripts", "blocks", "dense_lasagna_on_place.js")),
    };
}

public class DenseLasagnaGlowyPermutation : BlockPermutation
{
    public override string Condition => "query.block_state('test:radioactive') == true";
    public override Block Parent => new DenseLasagnaBlock();

    public override int? LightEmission => 7;
}