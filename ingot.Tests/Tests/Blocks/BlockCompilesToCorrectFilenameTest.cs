using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Blocks;

public class BlockCompilesToCorrectFilenameTest
{
    [Fact]
    public void Compile_BlockCompilesToCorrectFilename()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            PackTestBuilder.Create()
                .AddBlock<TestBlock>()
                .Compile(output.Path, verbose: false);

            Assert.True(File.Exists(Path.Combine(output.Path, "bp", "blocks", "test_block.json")));
        }
    }
}