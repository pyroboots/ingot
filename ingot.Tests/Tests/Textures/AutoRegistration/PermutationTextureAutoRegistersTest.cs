using ingot.Tests.Content.Blocks;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Textures.AutoRegistration;

public class PermutationTextureAutoRegistersTest
{
    [Fact]
    public void Compile_permutationTextureAutoRegisters()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            PackTestBuilder.Create()
                .AddBlock<PermutationTestBlock>()
                .Compile(outputDir, verbose: false);

            string terrainAtlas = File.ReadAllText(Path.Combine(outputDir, "rp", "textures", "terrain_texture.json"));
            Assert.Contains("glowy_variant", terrainAtlas);
            Assert.True(File.Exists(Path.Combine(outputDir, "rp", "textures", "blocks", "glowy_variant.png")));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}