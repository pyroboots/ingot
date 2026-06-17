using ingot.Core.Behaviour.Recipe;
using ingot.Tests.Content.Recipes;

namespace ingot.Tests.Tests.Recipes;

public class ShapelessRecipeJsonContainsIngredientsTest
{
    [Fact]
    public void Compile_shapelessRecipeJsonContainsIngredients()
    {
        string json = ShapelessRecipe.Compile(typeof(TestShapelessRecipe));
        Assert.Contains("minecraft:recipe_shapeless", json);
        Assert.Contains("minecraft:stick", json);
        Assert.Contains("test:test_item", json);
        Assert.Contains("test:food_item", json);
    }
}