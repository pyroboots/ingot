using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Compile;

public class CreatesRpFolderStructureTest
{
    [Fact]
    public void Compile_CreatesRpFolderStructure()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            PackTestBuilder.Create()
                .AddBlock<TestBlock>()
                .Compile(output.Path, verbose: false);

            Assert.True(Directory.Exists(Path.Combine(output.Path, "rp", "textures", "blocks")));
            Assert.True(Directory.Exists(Path.Combine(output.Path, "rp", "textures", "items")));
            Assert.True(Directory.Exists(Path.Combine(output.Path, "rp", "textures", "entity")));
            Assert.True(Directory.Exists(Path.Combine(output.Path, "rp", "textures", "particle")));
            Assert.True(Directory.Exists(Path.Combine(output.Path, "rp", "models")));
            Assert.True(Directory.Exists(Path.Combine(output.Path, "rp", "models", "blocks")));
        }
    }
}