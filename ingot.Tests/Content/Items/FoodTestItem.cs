using ingot.Core.Behaviour;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Item;

namespace ingot.Tests.Content.Items;

internal class FoodTestItem : Item, IFood, IUseAnimation, IUseModifiers
{
    public override Identifier Identifier => new("test:food_item");
    public override string Texture => "food_item";
    public override string? TexturePath => FixturePaths.Resolve("test_item.png");
    public override string DisplayName => "Food Item";

    int IFood.Nutrition => 4;
    float IFood.SaturationModifier => 0.5f;
    string IFood.UsingConvertsTo => "minecraft:bowl";

    string IUseAnimation.Value => "eat";
    float IUseModifiers.UseDuration => 1.6f;
    float IUseModifiers.MovementModifier => 0.35f;
    dynamic? IUseModifiers.StartUsing => "always";
    dynamic? IUseModifiers.StartSound => null;
}