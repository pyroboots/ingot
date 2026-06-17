using ingot.Tests.Content.Blocks;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Loot;

public class BlockLootReferenceMatchesCompiledPathTest
{
    [Fact]
    public void Compile_blockLootReferenceMatchesCompiledPath()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            PackTestBuilder.Create()
                .AddBlock<LootTableTestBlock>()
                .Compile(outputDir, verbose: false);

            string blockJson = File.ReadAllText(Path.Combine(outputDir, "bp", "blocks", "loot_block.json"));
            Assert.Contains("loot_tables/blocks/loot_block.json", blockJson);
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}