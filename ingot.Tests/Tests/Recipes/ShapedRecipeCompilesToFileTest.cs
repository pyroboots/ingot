using ingot.Tests.Content.Recipes;
using ingot.Tests.Support;

namespace ingot.Tests.Recipes;

public class ShapedRecipeCompilesToFileTest
{
    [Fact]
    public void Compile_ShapedRecipeCompilesToFile()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            PackTestBuilder.Create()
                .AddRecipe<TestShapedRecipe>()
                .Compile(output.Path, verbose: false);

            Assert.True(File.Exists(Path.Combine(output.Path, "bp", "recipes", "shaped_recipe.json")));
        }
    }
}