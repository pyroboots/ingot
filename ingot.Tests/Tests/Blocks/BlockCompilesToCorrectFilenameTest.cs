using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Blocks;

public class BlockCompilesToCorrectFilenameTest
{
    [Fact]
    public void Compile_blockCompilesToCorrectFilename()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            PackTestBuilder.Create()
                .AddBlock<TestBlock>()
                .Compile(outputDir, verbose: false);

            Assert.True(File.Exists(Path.Combine(outputDir, "bp", "blocks", "test_block.json")));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}