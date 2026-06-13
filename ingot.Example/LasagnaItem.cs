using ingot.Core.Content;
using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits.Item;

namespace ingot.Example;

public class LasagnaItem : Item, IFood, IBlockPlacer
{
    public override string Identifier => "test:lasagna";
    public override string Texture => "lasagna";
    public override string DisplayName => "Lasagna";
    
    int IFood.Nutrition => 5;
    float IFood.SaturationModifier => 0.9f;
    string IFood.UsingConvertsTo => "minecraft:bowl";
    
    dynamic IBlockPlacer.Block => "test:block_of_dense_lasagna";
    bool IBlockPlacer.ReplaceBlockItem => true;
}