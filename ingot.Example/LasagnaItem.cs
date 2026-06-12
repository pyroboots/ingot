using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits.Item;

namespace ingot.Example;

public class LasagnaItem : Item, IFood, IBlockPlacer, IDisplayName
{
    public override string Identifier => "test:lasagna";
    public override string Texture => "lasagna";
    string IDisplayName.Value => "Bowl of Lasagna";
    
    int IFood.Nutrition => 5;
    float IFood.SaturationModifier => 0.9f;
    string IFood.UsingConvertsTo => "minecraft:bowl";
    
    dynamic IBlockPlacer.Block => "test:block_of_dense_lasagna";
    bool IBlockPlacer.ReplaceBlockItem => true;
}