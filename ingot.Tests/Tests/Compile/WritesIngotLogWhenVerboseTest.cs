using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Compile;

public class WritesIngotLogWhenVerboseTest
{
    [Fact]
    public void Compile_verbose_writesIngotLog()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            PackTestBuilder.Create()
                .AddBlock<TestBlock>()
                .Compile(outputDir, verbose: true);

            Assert.True(File.Exists(Path.Combine(outputDir, "ingot.log")));
            string log = File.ReadAllText(Path.Combine(outputDir, "ingot.log"));
            Assert.Contains("pack compilation started", log);
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}