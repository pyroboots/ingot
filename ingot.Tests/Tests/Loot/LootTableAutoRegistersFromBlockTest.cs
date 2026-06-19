using ingot.Tests.Content.Blocks;
using ingot.Tests.Support;

namespace ingot.Tests.Loot;

public class LootTableAutoRegistersFromBlockTest
{
    [Fact]
    public void Compile_LootTableAutoRegistersFromBlock()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            PackTestBuilder.Create()
                .AddBlock<LootTableTestBlock>()
                .Compile(output.Path, verbose: false);

            Assert.True(File.Exists(Path.Combine(output.Path, "bp", "loot_tables", "blocks", "loot_block.json")));
        }
    }
}