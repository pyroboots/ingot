using ingot.Tests.Content.Entities;
using ingot.Tests.Support;

namespace ingot.Tests.Entities;

public class EntityCompilesToCorrectFilenameTest
{
    [Fact]
    public void Compile_EntityCompilesToCorrectFilename()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            PackTestBuilder.Create()
                .AddEntity<TestEntity>()
                .Compile(output.Path, verbose: false);

            Assert.True(File.Exists(Path.Combine(output.Path, "bp", "entities", "test_entity.json")));
        }
    }
}