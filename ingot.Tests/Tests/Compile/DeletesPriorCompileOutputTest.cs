using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Compile;

public class DeletesPriorCompileOutputTest
{
    [Fact]
    public void Compile_DeletesPriorOutputBeforeRecompiling()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        Pack pack = Pack.Create(TestUuids.Behaviour, "test pack", "compile test", TestUuids.Resource);

        pack.AddBlock<TestBlock>();
        pack.Compile(output.Path, verbose: false);

        string staleBlockPath = Path.Combine(output.Path, "bp", "blocks", "test_block.json");
        Assert.True(File.Exists(staleBlockPath));

        pack.BehaviourPack.Blocks.Clear();
        pack.Compile(output.Path, verbose: false);

        Assert.False(File.Exists(staleBlockPath));
    }

    [Fact]
    public void CompileComMojang_DeletesPriorOutputBeforeRecompiling()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        Pack pack = Pack.Create(TestUuids.Behaviour, "test pack", "compile test", TestUuids.Resource);

        pack.AddBlock<TestBlock>();
        pack.CompileComMojang(output.Path, verbose: false);

        string staleBlockPath = Path.Combine(
            output.Path,
            "development_behavior_packs",
            "test pack BP",
            "blocks",
            "test_block.json");
        Assert.True(File.Exists(staleBlockPath));

        pack.BehaviourPack.Blocks.Clear();
        pack.CompileComMojang(output.Path, verbose: false);

        Assert.False(File.Exists(staleBlockPath));
    }
}