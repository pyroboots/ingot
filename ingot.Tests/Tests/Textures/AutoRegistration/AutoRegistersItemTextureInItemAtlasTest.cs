using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Textures.AutoRegistration;

public class AutoRegistersItemTextureInItemAtlasTest
{
    [Fact]
    public void Compile_AutoRegistersItemTextureInItemAtlas()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            Pack pack = Pack.Create(TestUuids.Behaviour, "test pack", "item texture test", TestUuids.Resource)
                .AddItem<TestItem>();

            pack.Compile(output.Path, verbose: false);

            string itemAtlas = File.ReadAllText(Path.Combine(output.Path, "rp", "textures", "item_texture.json"));

            Assert.Contains("test_item", itemAtlas);
            Assert.True(File.Exists(Path.Combine(output.Path, "rp", "textures", "items", "test_item.png")));
        }
    }
}
