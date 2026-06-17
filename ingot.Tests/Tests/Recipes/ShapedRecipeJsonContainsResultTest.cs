using ingot.Core.Behaviour.Recipe;
using ingot.Tests.Content.Recipes;

namespace ingot.Tests.Tests.Recipes;

public class ShapedRecipeJsonContainsResultTest
{
    [Fact]
    public void Compile_shapedRecipeJsonContainsResult()
    {
        string json = ShapedRecipe.Compile(typeof(TestShapedRecipe));
        Assert.Contains("minecraft:recipe_shaped", json);
        Assert.Contains("test:equipment_item", json);
        Assert.Contains("\"count\": 2", json);
    }
}