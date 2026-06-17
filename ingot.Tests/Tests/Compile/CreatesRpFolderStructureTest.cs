using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Compile;

public class CreatesRpFolderStructureTest
{
    [Fact]
    public void Compile_createsRpFolderStructure()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            PackTestBuilder.Create()
                .AddBlock<TestBlock>()
                .Compile(outputDir, verbose: false);

            Assert.True(Directory.Exists(Path.Combine(outputDir, "rp", "textures", "blocks")));
            Assert.True(Directory.Exists(Path.Combine(outputDir, "rp", "textures", "items")));
            Assert.True(Directory.Exists(Path.Combine(outputDir, "rp", "textures", "entity")));
            Assert.True(Directory.Exists(Path.Combine(outputDir, "rp", "textures", "particle")));
            Assert.True(Directory.Exists(Path.Combine(outputDir, "rp", "models")));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}