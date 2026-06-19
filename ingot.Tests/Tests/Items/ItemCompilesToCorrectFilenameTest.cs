using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Items;

public class ItemCompilesToCorrectFilenameTest
{
    [Fact]
    public void Compile_ItemCompilesToCorrectFilename()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            PackTestBuilder.Create()
                .AddItem<TestItem>()
                .Compile(output.Path, verbose: false);

            Assert.True(File.Exists(Path.Combine(output.Path, "bp", "items", "test_item.json")));
        }
    }
}