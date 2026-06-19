using ingot.Tests.Content.Recipes;
using ingot.Tests.Support;

namespace ingot.Tests.Recipes;

public class FurnaceRecipeCompilesToFileTest
{
    [Fact]
    public void Compile_FurnaceRecipeCompilesToFile()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            PackTestBuilder.Create()
                .AddRecipe<TestFurnaceRecipe>()
                .Compile(output.Path, verbose: false);

            Assert.True(File.Exists(Path.Combine(output.Path, "bp", "recipes", "furnace_recipe.json")));
        }
    }
}