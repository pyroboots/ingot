using ingot.Tests.Content.Blocks;
using ingot.Tests.Support;

namespace ingot.Tests.Blocks;

public class BlocksJsonSeedsSoundWithoutResourceTextureTest
{
    [Fact]
    public void Compile_BlockWithSoundAndNoResourceTextureWritesBlocksJson()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            PackTestBuilder.Create()
                .AddBlock<SoundOnlyTestBlock>()
                .Compile(output.Path, verbose: false);

            string blocksJson = File.ReadAllText(Path.Combine(output.Path, "rp", "blocks.json"));
            Assert.Contains("test:sound_only_block", blocksJson);
            Assert.Contains("\"sound\": \"copper\"", blocksJson);
            Assert.DoesNotContain("textures", blocksJson);
        }
    }
}
