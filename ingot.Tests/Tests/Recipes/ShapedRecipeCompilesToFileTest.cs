using ingot.Tests.Content.Recipes;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Recipes;

public class ShapedRecipeCompilesToFileTest
{
    [Fact]
    public void Compile_shapedRecipeCompilesToFile()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            PackTestBuilder.Create()
                .AddRecipe<TestShapedRecipe>()
                .Compile(outputDir, verbose: false);

            Assert.True(File.Exists(Path.Combine(outputDir, "bp", "recipes", "shaped_recipe.json")));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}