using ingot.Tests.Content.Recipes;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Recipes;

public class ShapelessRecipeCompilesToFileTest
{
    [Fact]
    public void Compile_shapelessRecipeCompilesToFile()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            PackTestBuilder.Create()
                .AddRecipe<TestShapelessRecipe>()
                .Compile(outputDir, verbose: false);

            Assert.True(File.Exists(Path.Combine(outputDir, "bp", "recipes", "shapeless_recipe.json")));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}