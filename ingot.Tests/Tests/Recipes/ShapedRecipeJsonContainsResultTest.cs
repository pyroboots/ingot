using ingot.Core.Behaviour.Recipe;
using ingot.Tests.Content.Recipes;

namespace ingot.Tests.Recipes;

public class ShapedRecipeJsonContainsResultTest
{
    [Fact]
    public void Compile_ShapedRecipeJsonContainsResult()
    {
        string json = ShapedRecipe.Compile(typeof(TestShapedRecipe));
        Assert.Contains("minecraft:recipe_shaped", json);
        Assert.Contains("test:equipment_item", json);
        Assert.Contains("\"count\": 2", json);
    }
}