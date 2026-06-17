using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Compile;

public class CreatesBpFolderStructureTest
{
    [Fact]
    public void Compile_createsBpFolderStructure()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            PackTestBuilder.Create()
                .AddBlock<TestBlock>()
                .AddItem<TestItem>()
                .AddEntity<Content.Entities.TestEntity>()
                .AddRecipe<Content.Recipes.TestShapedRecipe>()
                .AddLootTable<Content.Loot.TestBlockLootTable>()
                .Compile(outputDir, verbose: false);

            Assert.True(Directory.Exists(Path.Combine(outputDir, "bp", "entities")));
            Assert.True(Directory.Exists(Path.Combine(outputDir, "bp", "blocks")));
            Assert.True(Directory.Exists(Path.Combine(outputDir, "bp", "items")));
            Assert.True(Directory.Exists(Path.Combine(outputDir, "bp", "recipes")));
            Assert.True(Directory.Exists(Path.Combine(outputDir, "bp", "loot_tables")));
            Assert.True(Directory.Exists(Path.Combine(outputDir, "bp", "scripts")));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}