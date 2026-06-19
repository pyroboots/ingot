using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Content.Items;
using ingot.Tests.Support;

namespace ingot.Tests.Textures.Override;

public class ManualTextureOverridesBehaviourSourcePathTest
{
    [Theory]
    [InlineData("block", "override_block", "blocks")]
    [InlineData("item", "override_item", "items")]
    public void Compile_ManualTextureOverridesBehaviourSourcePath(string kind, string textureKey, string atlasFolder)
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string manualPath = FixturePaths.Resolve("manual.png");
        byte[] expectedBytes = File.ReadAllBytes(manualPath);

        Pack pack = Pack.Create(TestUuids.Behaviour, "test pack", "override test", TestUuids.Resource);
        if (kind == "block")
            pack.AddBlockTexture(textureKey, manualPath).AddBlock<OverrideTestBlock>();
        else
            pack.AddItemTexture(textureKey, manualPath).AddItem<OverrideTestItem>();

        pack.Compile(output.Path, verbose: false);

        string copiedPath = Path.Combine(output.Path, "rp", "textures", atlasFolder, $"{textureKey}.png");
        Assert.True(File.Exists(copiedPath));
        Assert.Equal(expectedBytes, File.ReadAllBytes(copiedPath));
    }
}