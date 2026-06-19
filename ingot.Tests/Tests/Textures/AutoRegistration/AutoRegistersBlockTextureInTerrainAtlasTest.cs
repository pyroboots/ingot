using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Textures.AutoRegistration;

public class AutoRegistersBlockTextureInTerrainAtlasTest
{
    [Fact]
    public void Compile_AutoRegistersBlockTextureInTerrainAtlas()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            Pack pack = Pack.Create(TestUuids.Behaviour, "test pack", "block texture test", TestUuids.Resource)
                .AddBlock<TestBlock>();

            pack.Compile(output.Path, verbose: false);

            string terrainAtlas = File.ReadAllText(Path.Combine(output.Path, "rp", "textures", "terrain_texture.json"));

            Assert.Contains("test_block", terrainAtlas);
            Assert.True(File.Exists(Path.Combine(output.Path, "rp", "textures", "blocks", "test_block.png")));
        }
    }
}