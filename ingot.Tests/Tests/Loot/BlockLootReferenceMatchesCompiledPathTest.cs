using ingot.Tests.Content.Blocks;
using ingot.Tests.Support;

namespace ingot.Tests.Loot;

public class BlockLootReferenceMatchesCompiledPathTest
{
    [Fact]
    public void Compile_BlockLootReferenceMatchesCompiledPath()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            PackTestBuilder.Create()
                .AddBlock<LootTableTestBlock>()
                .Compile(output.Path, verbose: false);

            string blockJson = File.ReadAllText(Path.Combine(output.Path, "bp", "blocks", "loot_block.json"));
            Assert.Contains("loot_tables/blocks/loot_block.json", blockJson);
        }
    }
}