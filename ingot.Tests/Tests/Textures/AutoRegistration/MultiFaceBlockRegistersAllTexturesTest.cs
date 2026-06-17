using ingot.Tests.Content.Blocks;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Textures.AutoRegistration;

public class MultiFaceBlockRegistersAllTexturesTest
{
    [Fact]
    public void Compile_multiFaceBlockRegistersAllTextures()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            PackTestBuilder.Create()
                .AddBlock<MultiFaceTestBlock>()
                .Compile(outputDir, verbose: false);

            string terrainAtlas = File.ReadAllText(Path.Combine(outputDir, "rp", "textures", "terrain_texture.json"));
            Assert.Contains("multi_face_side", terrainAtlas);
            Assert.Contains("multi_face_top", terrainAtlas);
            Assert.True(File.Exists(Path.Combine(outputDir, "rp", "textures", "blocks", "multi_face_side.png")));
            Assert.True(File.Exists(Path.Combine(outputDir, "rp", "textures", "blocks", "multi_face_top.png")));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}