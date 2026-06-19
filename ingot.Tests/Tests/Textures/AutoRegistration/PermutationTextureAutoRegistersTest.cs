using ingot.Tests.Content.Blocks;
using ingot.Tests.Support;

namespace ingot.Tests.Textures.AutoRegistration;

public class PermutationTextureAutoRegistersTest
{
    [Fact]
    public void Compile_PermutationTextureAutoRegisters()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            PackTestBuilder.Create()
                .AddBlock<PermutationTestBlock>()
                .Compile(output.Path, verbose: false);

            string terrainAtlas = File.ReadAllText(Path.Combine(output.Path, "rp", "textures", "terrain_texture.json"));
            Assert.Contains("glowy_variant", terrainAtlas);
            Assert.True(File.Exists(Path.Combine(output.Path, "rp", "textures", "blocks", "glowy_variant.png")));
        }
    }
}