using ingot.Tests.Content.Loot;
using ingot.Tests.Support;

namespace ingot.Tests.Loot;

public class ExplicitLootTableCompilesTest
{
    [Fact]
    public void Compile_ExplicitLootTableCompiles()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            PackTestBuilder.Create()
                .AddLootTable<TestBlockLootTable>()
                .Compile(output.Path, verbose: false);

            Assert.True(File.Exists(Path.Combine(output.Path, "bp", "loot_tables", "blocks", "loot_block.json")));
        }
    }
}