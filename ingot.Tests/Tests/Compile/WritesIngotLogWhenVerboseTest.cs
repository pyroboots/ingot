using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Compile;

public class WritesIngotLogWhenVerboseTest
{
    [Fact]
    public void Compile_Verbose_WritesIngotLog()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            PackTestBuilder.Create()
                .AddBlock<TestBlock>()
                .Compile(output.Path, verbose: true);

            Assert.True(File.Exists(Path.Combine(output.Path, "ingot.log")));
            string log = File.ReadAllText(Path.Combine(output.Path, "ingot.log"));
            Assert.Contains("pack compilation started", log);
        }
    }
}