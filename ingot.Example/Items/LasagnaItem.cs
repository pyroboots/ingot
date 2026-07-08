using ingot.Core.Behaviour;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Item;

using Version = ingot.Core.Common.Version;

namespace ingot.Example.Items;

public class LasagnaItem : Item, IFood, IBlockPlacer, IUseAnimation, IUseModifiers
{
    public override Version FormatVersion => new(1, 21, 30);
    public override Identifier Identifier => new("test:lasagna");
    public override string Texture => "suspicious_stew";
    public override string DisplayName => "Bowl of Lasagna";

    int IFood.Nutrition => 5;
    float IFood.SaturationModifier => 0.9f;
    string IFood.UsingConvertsTo => "minecraft:bowl";
    
    string IUseAnimation.Value => "eat";
    float IUseModifiers.MovementModifier => 0.35f;
    float IUseModifiers.UseDuration => 1.6f;
    dynamic? IUseModifiers.StartUsing => "always";
    dynamic? IUseModifiers.StartSound => null;
    
    dynamic IBlockPlacer.Block => "test:block_of_dense_lasagna";
    bool IBlockPlacer.ReplaceBlockItem => true;
}