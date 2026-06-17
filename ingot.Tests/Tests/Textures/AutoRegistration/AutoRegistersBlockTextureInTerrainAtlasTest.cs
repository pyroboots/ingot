using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Textures.AutoRegistration;

public class AutoRegistersBlockTextureInTerrainAtlasTest
{
    [Fact]
    public void Compile_autoRegistersBlockTextureInTerrainAtlas()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            Pack pack = Pack.Create(TestUuids.Behaviour, "test pack", "block texture test", TestUuids.Resource)
                .AddBlock<TestBlock>();

            pack.Compile(outputDir, verbose: false);

            string terrainAtlas = File.ReadAllText(Path.Combine(outputDir, "rp", "textures", "terrain_texture.json"));

            Assert.Contains("test_block", terrainAtlas);
            Assert.True(File.Exists(Path.Combine(outputDir, "rp", "textures", "blocks", "test_block.png")));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}