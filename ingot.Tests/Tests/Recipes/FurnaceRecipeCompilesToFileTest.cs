using ingot.Tests.Content.Recipes;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Recipes;

public class FurnaceRecipeCompilesToFileTest
{
    [Fact]
    public void Compile_furnaceRecipeCompilesToFile()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            PackTestBuilder.Create()
                .AddRecipe<TestFurnaceRecipe>()
                .Compile(outputDir, verbose: false);

            Assert.True(File.Exists(Path.Combine(outputDir, "bp", "recipes", "furnace_recipe.json")));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}