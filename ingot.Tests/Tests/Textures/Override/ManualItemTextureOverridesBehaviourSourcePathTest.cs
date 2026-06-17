using ingot.Tests.Content;
using ingot.Tests.Content.Items;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Textures.Override;

public class ManualItemTextureOverridesBehaviourSourcePathTest
{
    [Fact]
    public void Compile_manualItemTextureOverridesBehaviourSourcePath()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        string manualPath = FixturePaths.Resolve("manual.png");
        byte[] expectedBytes = File.ReadAllBytes(manualPath);

        try
        {
            PackTestBuilder.Create()
                .AddItemTexture("override_item", manualPath)
                .AddItem<OverrideTestItem>()
                .Compile(outputDir, verbose: false);

            string copiedPath = Path.Combine(outputDir, "rp", "textures", "items", "override_item.png");
            Assert.True(File.Exists(copiedPath));
            Assert.Equal(expectedBytes, File.ReadAllBytes(copiedPath));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}