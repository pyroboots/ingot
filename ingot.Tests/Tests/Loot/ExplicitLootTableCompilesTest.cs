using ingot.Tests.Content.Loot;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Loot;

public class ExplicitLootTableCompilesTest
{
    [Fact]
    public void Compile_explicitLootTableCompiles()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            PackTestBuilder.Create()
                .AddLootTable<TestBlockLootTable>()
                .Compile(outputDir, verbose: false);

            Assert.True(File.Exists(Path.Combine(outputDir, "bp", "loot_tables", "blocks", "loot_block.json")));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}