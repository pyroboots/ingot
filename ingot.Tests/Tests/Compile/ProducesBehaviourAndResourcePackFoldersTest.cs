using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Compile;

public class ProducesBehaviourAndResourcePackFoldersTest
{
    [Fact]
    public void Compile_ProducesBehaviourAndResourcePackFolders()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            Pack pack = Pack.Create(TestUuids.Behaviour, "test pack", "compile test", TestUuids.Resource)
                .AddBlock<TestBlock>();

            pack.Compile(output.Path, verbose: false);

            Assert.True(Directory.Exists(Path.Combine(output.Path, "bp")));
            Assert.True(Directory.Exists(Path.Combine(output.Path, "rp")));
            Assert.True(File.Exists(Path.Combine(output.Path, "bp", "manifest.json")));
            Assert.True(File.Exists(Path.Combine(output.Path, "rp", "manifest.json")));
            Assert.True(File.Exists(Path.Combine(output.Path, "bp", "blocks", "test_block.json")));
        }
    }
}