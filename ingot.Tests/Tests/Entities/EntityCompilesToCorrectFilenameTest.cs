using ingot.Tests.Content.Entities;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Entities;

public class EntityCompilesToCorrectFilenameTest
{
    [Fact]
    public void Compile_entityCompilesToCorrectFilename()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            PackTestBuilder.Create()
                .AddEntity<TestEntity>()
                .Compile(outputDir, verbose: false);

            Assert.True(File.Exists(Path.Combine(outputDir, "bp", "entities", "test_entity.json")));
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}