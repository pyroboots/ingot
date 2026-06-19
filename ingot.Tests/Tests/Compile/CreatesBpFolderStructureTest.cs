using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Compile;

public class CreatesBpFolderStructureTest
{
    [Fact]
    public void Compile_CreatesBpFolderStructure()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            PackTestBuilder.Create()
                .AddBlock<TestBlock>()
                .AddItem<TestItem>()
                .AddEntity<Content.Entities.TestEntity>()
                .AddRecipe<Content.Recipes.TestShapedRecipe>()
                .AddLootTable<Content.Loot.TestBlockLootTable>()
                .Compile(output.Path, verbose: false);

            Assert.True(Directory.Exists(Path.Combine(output.Path, "bp", "entities")));
            Assert.True(Directory.Exists(Path.Combine(output.Path, "bp", "blocks")));
            Assert.True(Directory.Exists(Path.Combine(output.Path, "bp", "items")));
            Assert.True(Directory.Exists(Path.Combine(output.Path, "bp", "recipes")));
            Assert.True(Directory.Exists(Path.Combine(output.Path, "bp", "loot_tables")));
            Assert.True(Directory.Exists(Path.Combine(output.Path, "bp", "scripts")));
        }
    }
}