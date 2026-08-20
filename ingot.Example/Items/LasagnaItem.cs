using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.Common.SharedConstructs;
using ingot.Core.Resource;
using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits.Item;
using ingot.Example.Recipes;

using Version = ingot.Core.Common.Version;

namespace ingot.Example.Items;

public class LasagnaItem : Item, IFood, IBlockPlacer, IUseAnimation, IUseModifiers
{
    public override Version FormatVersion => new(1, 26, 30);
    public override Identifier Identifier => new("test:lasagna");
    public override string Texture => "suspicious_stew";
    public override string DisplayName => "Bowl of Lasagna";

    public override RecipeReference Recipe => new RecipeReference<LasagnaRecipe>();

    int IFood.Nutrition => 5;
    float IFood.SaturationModifier => 0.9f;
    Either<string, Dictionary<string, string>> IFood.UsingConvertsTo => "minecraft:bowl";
    
    string IUseAnimation.Value => "eat";
    float IUseModifiers.MovementModifier => 0.35f;
    float IUseModifiers.UseDuration => 1.6f;
    string IUseModifiers.StartUsing => IUseModifiers.Startusing_Always;
    [IngotExclude]
    string IUseModifiers.StartSound => null;
    
    Identifier IBlockPlacer.Block => "test:block_of_dense_lasagna";
    bool IBlockPlacer.ReplaceBlockItem => true;
}