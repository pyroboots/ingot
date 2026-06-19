using ingot.Tests.Content.Blocks;
using ingot.Tests.Support;

namespace ingot.Tests.Textures.AutoRegistration;

public class MultiFaceBlockRegistersAllTexturesTest
{
    [Fact]
    public void Compile_MultiFaceBlockRegistersAllTextures()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            PackTestBuilder.Create()
                .AddBlock<MultiFaceTestBlock>()
                .Compile(output.Path, verbose: false);

            string terrainAtlas = File.ReadAllText(Path.Combine(output.Path, "rp", "textures", "terrain_texture.json"));
            Assert.Contains("multi_face_side", terrainAtlas);
            Assert.Contains("multi_face_top", terrainAtlas);
            Assert.True(File.Exists(Path.Combine(output.Path, "rp", "textures", "blocks", "multi_face_side.png")));
            Assert.True(File.Exists(Path.Combine(output.Path, "rp", "textures", "blocks", "multi_face_top.png")));
        }
    }
}