using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Textures.AutoRegistration;

public class AutoRegistersItemTextureInItemAtlasTest
{
    [Fact]
    public void Compile_autoRegistersItemTextureInItemAtlas()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            Pack pack = Pack.Create(TestUuids.Behaviour, "test pack", "item texture test", TestUuids.Resource)
                .AddItem<TestItem>();

            pack.Compile(outputDir, verbose: false);

            string itemAtlas = File.ReadAllText(Path.Combine(outputDir, "rp", "textures", "item_texture.json"));

            Assert.Contains("test_item", itemAtlas);
            Assert.True(File.Exists(Path.Combine(outputDir, "rp", "textures", "items", "test_item.png")));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}