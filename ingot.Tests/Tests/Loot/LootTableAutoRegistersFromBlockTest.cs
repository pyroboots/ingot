using ingot.Tests.Content.Blocks;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Loot;

public class LootTableAutoRegistersFromBlockTest
{
    [Fact]
    public void Compile_lootTableAutoRegistersFromBlock()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            PackTestBuilder.Create()
                .AddBlock<LootTableTestBlock>()
                .Compile(outputDir, verbose: false);

            Assert.True(File.Exists(Path.Combine(outputDir, "bp", "loot_tables", "blocks", "loot_block.json")));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}