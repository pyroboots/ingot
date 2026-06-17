using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Compile;

public class ProducesBehaviourAndResourcePackFoldersTest
{
    [Fact]
    public void Compile_producesBehaviourAndResourcePackFolders()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            Pack pack = Pack.Create(TestUuids.Behaviour, "test pack", "compile test", TestUuids.Resource)
                .AddBlock<TestBlock>();

            pack.Compile(outputDir, verbose: false);

            Assert.True(Directory.Exists(Path.Combine(outputDir, "bp")));
            Assert.True(Directory.Exists(Path.Combine(outputDir, "rp")));
            Assert.True(File.Exists(Path.Combine(outputDir, "bp", "manifest.json")));
            Assert.True(File.Exists(Path.Combine(outputDir, "rp", "manifest.json")));
            Assert.True(File.Exists(Path.Combine(outputDir, "bp", "blocks", "test_block.json")));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}