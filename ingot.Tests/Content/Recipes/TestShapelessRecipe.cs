using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;

namespace ingot.Tests.Content.Recipes;

internal class TestShapelessRecipe : ShapelessRecipe
{
    public override Identifier Identifier => new("test:shapeless_recipe");

    public override RecipeItem[] Ingredients =>
    [
        new() { Item = new Identifier("minecraft:stick") },
        new() { Item = new Identifier("test:test_item") }
    ];

    public override RecipeItem Result => new() { Item = new Identifier("test:food_item"), Count = 1 };
}