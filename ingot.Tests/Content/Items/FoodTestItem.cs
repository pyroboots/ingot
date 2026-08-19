using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.Common.SharedConstructs;
using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits.Item;

using Version = ingot.Core.Common.Version;

namespace ingot.Tests.Content.Items;

internal class FoodTestItem : Item, IFood, IUseAnimation, IUseModifiers
{
    public override Version FormatVersion => new(1, 26, 30);
    public override Identifier Identifier => new("test:food_item");
    public override string Texture => "food_item";
    public override string? TexturePath => FixturePaths.Resolve("test_item.png");
    public override string DisplayName => "Food Item";

    int IFood.Nutrition => 4;
    float IFood.SaturationModifier => 0.5f;
    Identifier IFood.UsingConvertsTo => new("minecraft:bowl");

    string IUseAnimation.Value => "eat";
    float IUseModifiers.UseDuration => 1.6f;
    float IUseModifiers.MovementModifier => 0.35f;
    string IUseModifiers.StartUsing => IUseModifiers.StartUsing_Always;
    
    [IngotExclude]
    string IUseModifiers.StartSound => null;
}