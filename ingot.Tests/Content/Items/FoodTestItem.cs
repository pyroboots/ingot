using ingot.Core.Behaviour;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Item;

namespace ingot.Tests.Content.Items;

internal class FoodTestItem : Item, IFood
{
    public override Identifier Identifier => new("test:food_item");
    public override string Texture => "food_item";
    public override string? TexturePath => FixturePaths.Resolve("test_item.png");
    public override string DisplayName => "Food Item";

    int IFood.Nutrition => 4;
    float IFood.SaturationModifier => 0.5f;
    string IFood.UsingConvertsTo => "minecraft:bowl";
}