using ingot.Tests.Content.Blocks;
using ingot.Tests.Support;

namespace ingot.Tests.Textures.AutoRegistration;

public class SkipsVanillaTextureKeyTest
{
    [Fact]
    public void Compile_SkipsVanillaTextureKey()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            PackTestBuilder.Create()
                .AddBlock<VanillaTextureTestBlock>()
                .Compile(output.Path, verbose: false);

            Assert.False(File.Exists(Path.Combine(output.Path, "rp", "textures", "terrain_texture.json")));
            Assert.False(File.Exists(Path.Combine(output.Path, "rp", "textures", "blocks", "minecraft:stone.png")));
        }
    }
}