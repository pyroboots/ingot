using ingot.Tests.Content.Blocks;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Textures.AutoRegistration;

public class SkipsVanillaTextureKeyTest
{
    [Fact]
    public void Compile_skipsVanillaTextureKey()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            PackTestBuilder.Create()
                .AddBlock<VanillaTextureTestBlock>()
                .Compile(outputDir, verbose: false);

            Assert.False(File.Exists(Path.Combine(outputDir, "rp", "textures", "terrain_texture.json")));
            Assert.False(File.Exists(Path.Combine(outputDir, "rp", "textures", "blocks", "minecraft:stone.png")));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}