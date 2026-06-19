using ingot.Tests.Content.Recipes;
using ingot.Tests.Support;

namespace ingot.Tests.Recipes;

public class ShapelessRecipeCompilesToFileTest
{
    [Fact]
    public void Compile_ShapelessRecipeCompilesToFile()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            PackTestBuilder.Create()
                .AddRecipe<TestShapelessRecipe>()
                .Compile(output.Path, verbose: false);

            Assert.True(File.Exists(Path.Combine(output.Path, "bp", "recipes", "shapeless_recipe.json")));
        }
    }
}