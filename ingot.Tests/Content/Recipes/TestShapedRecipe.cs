using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;

namespace ingot.Tests.Content.Recipes;

internal class TestShapedRecipe : ShapedRecipe
{
    public override Identifier Identifier => new("test:shaped_recipe");

    public override Identifier?[][] Pattern =>
    [
        [new Identifier("test:test_item"), new Identifier("test:food_item")]
    ];

    public override Identifier Result => new("test:equipment_item");
    public override int ResultAmount => 2;
}