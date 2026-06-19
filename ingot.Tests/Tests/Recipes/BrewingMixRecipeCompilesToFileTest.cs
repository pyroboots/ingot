using ingot.Tests.Content.Recipes;
using ingot.Tests.Support;

namespace ingot.Tests.Recipes;

public class BrewingMixRecipeCompilesToFileTest
{
    [Fact]
    public void Compile_BrewingMixRecipeCompilesToFile()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();

        PackTestBuilder.Create()
            .AddRecipe<TestBrewingMixRecipe>()
            .Compile(output.Path, verbose: false);

        Assert.True(File.Exists(Path.Combine(output.Path, "bp", "recipes", "brewing_mix_recipe.json")));
    }
}