using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Items;

public class ItemCompilesToCorrectFilenameTest
{
    [Fact]
    public void Compile_itemCompilesToCorrectFilename()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            PackTestBuilder.Create()
                .AddItem<TestItem>()
                .Compile(outputDir, verbose: false);

            Assert.True(File.Exists(Path.Combine(outputDir, "bp", "items", "test_item.json")));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}