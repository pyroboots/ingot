using ingot.Core;
using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Loot;
using ingot.Core.Common;
using ingot.Core.Resource;
using ingot.Core.Scripting;
using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits.Block;

namespace ingot.Example.Blocks;

public class DenseLasagnaBlockHooks : ICompileHooks
{
    public void PreCompile(object inst) => CompilerState.Warn("pre compile hooks!");
    public string? PostCompile(string json) => "// post compile hooks!\n\n" + json;
}

[CompileHooks(typeof(DenseLasagnaBlockHooks))]
public class DenseLasagnaBlock : Block, IDestructibleByMining
{
    public override Identifier Identifier => new("test:block_of_dense_lasagna");
    public override string DisplayName => "Block of Dense Lasagna";
    public override string? Geometry => "minecraft:geometry.full_block";
    public override string? Sound => "shroomlight";
    public override LootTable? Loot => new DenseLasagnaLoot();
    public override string[] Tags => ["minecraft:is_hoe_item_destructible"];
    
    float IDestructibleByMining.SecondsToDestroy => 2f;

    public override BlockPermutation[] Permutations =>
    [
        new DenseLasagnaGlowyPermutation()
    ];

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance(new TextureReference<DenseLasagnaBlock>(Path.Combine(AppContext.BaseDirectory, "Data", "dense_lasagna.png")), MaterialInstance.RenderMethods.AlphaTest)
    };

    public override Dictionary<string, dynamic[]> States => new()
    {
        { "test:radioactive", [false, true] }
    };

    public override BlockEvents? BlockEvents => new()
    {
        OnPlaceEvent = ScriptHandler.FromFile(Path.Combine(AppContext.BaseDirectory, "scripts", "blocks", "dense_lasagna_on_place.js")),
        PlayerInteractEvent = ScriptHandler.Inline(@"event.block.setPermutation(event.block.permutation.withState(""test:radioactive"", true)); event.block.dimension.playSound(""place.lodestone"", event.block.location);")
    };
}

public class DenseLasagnaGlowyPermutation : BlockPermutation
{
    public override Molang Condition => new Molang().BlockState("test:radioactive").Eq(true);
    public override Block Parent => new DenseLasagnaBlock();

    public override int? LightEmission => 7;
}