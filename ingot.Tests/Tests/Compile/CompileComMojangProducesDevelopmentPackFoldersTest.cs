using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Compile;

public class CompileComMojangProducesDevelopmentPackFoldersTest
{
    [Fact]
    public void CompileComMojang_ProducesDevelopmentPackFolders()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();

        Pack pack = Pack.Create(TestUuids.Behaviour, "test pack", "compile test", TestUuids.Resource)
            .AddBlock<TestBlock>();

        pack.CompileComMojang(output.Path, verbose: false);

        string behaviourPackDir = Path.Combine(output.Path, "development_behavior_packs", "test pack BP");
        string resourcePackDir = Path.Combine(output.Path, "development_resource_packs", "test pack RP");

        Assert.True(Directory.Exists(behaviourPackDir));
        Assert.True(Directory.Exists(resourcePackDir));
        Assert.True(File.Exists(Path.Combine(behaviourPackDir, "manifest.json")));
        Assert.True(File.Exists(Path.Combine(resourcePackDir, "manifest.json")));
        Assert.True(File.Exists(Path.Combine(behaviourPackDir, "blocks", "test_block.json")));
        Assert.False(Directory.Exists(Path.Combine(output.Path, "bp")));
        Assert.False(Directory.Exists(Path.Combine(output.Path, "rp")));
    }
}