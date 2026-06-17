using ingot.Core.Behaviour.Recipe;
using ingot.Tests.Content.Recipes;

namespace ingot.Tests.Tests.Recipes;

public class FurnaceRecipeJsonContainsInputOutputTest
{
    [Fact]
    public void Compile_furnaceRecipeJsonContainsInputOutput()
    {
        string json = FurnaceRecipe.Compile(typeof(TestFurnaceRecipe));
        Assert.Contains("minecraft:recipe_furnace", json);
        Assert.Contains("test:test_item", json);
        Assert.Contains("test:food_item", json);
    }
}