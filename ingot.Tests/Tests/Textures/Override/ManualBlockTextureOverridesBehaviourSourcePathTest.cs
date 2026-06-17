using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Textures.Override;

public class ManualBlockTextureOverridesBehaviourSourcePathTest
{
    [Fact]
    public void Compile_manualBlockTextureOverridesBehaviourSourcePath()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        string manualPath = FixturePaths.Resolve("manual.png");
        byte[] expectedBytes = File.ReadAllBytes(manualPath);

        try
        {
            Pack pack = Pack.Create(TestUuids.Behaviour, "test pack", "override test", TestUuids.Resource)
                .AddBlockTexture("override_block", manualPath)
                .AddBlock<OverrideTestBlock>();

            pack.Compile(outputDir, verbose: false);

            string copiedPath = Path.Combine(outputDir, "rp", "textures", "blocks", "override_block.png");
            Assert.True(File.Exists(copiedPath));
            Assert.Equal(expectedBytes, File.ReadAllBytes(copiedPath));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}