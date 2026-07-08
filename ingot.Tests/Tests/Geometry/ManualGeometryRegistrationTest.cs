using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Content.Blocks;
using ingot.Tests.Support;

namespace ingot.Tests.Geometry;

public class ManualGeometryRegistrationTest
{
    [Fact]
    public void Compile_AddGeometryCopiesGeoJsonToModelsBlocks()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = FixturePaths.Resolve("test_block.geo.json");
        byte[] expectedBytes = File.ReadAllBytes(sourcePath);

        PackTestBuilder.Create()
            .AddGeometry("geometry.test_block", sourcePath)
            .AddBlock<TestBlock>()
            .Compile(output.Path, verbose: false);

        string copiedPath = Path.Combine(output.Path, "rp", "models", "blocks", "test_block.geo.json");
        Assert.True(File.Exists(copiedPath));
        Assert.Equal(expectedBytes, File.ReadAllBytes(copiedPath));
    }

    [Fact]
    public void Compile_AddGeometryAcceptsMinecraftPrefixedIdentifier()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = FixturePaths.Resolve("test_block.geo.json");

        PackTestBuilder.Create()
            .AddGeometry("minecraft:geometry.test_block", sourcePath)
            .Compile(output.Path, verbose: false);

        Assert.True(File.Exists(Path.Combine(output.Path, "rp", "models", "blocks", "test_block.geo.json")));
    }

    [Fact]
    public void Compile_AddGeometryUsesCustomRpName()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = FixturePaths.Resolve("test_block.geo.json");

        PackTestBuilder.Create()
            .AddGeometry("geometry.custom_block", sourcePath, rpName: "custom_name")
            .Compile(output.Path, verbose: false);

        Assert.True(File.Exists(Path.Combine(output.Path, "rp", "models", "blocks", "custom_name.geo.json")));
    }

    [Fact]
    public void Compile_AddGeometryCreatesModelsBlocksFolder()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = FixturePaths.Resolve("test_block.geo.json");

        PackTestBuilder.Create()
            .AddGeometry("geometry.test_block", sourcePath)
            .Compile(output.Path, verbose: false);

        Assert.True(Directory.Exists(Path.Combine(output.Path, "rp", "models", "blocks")));
    }

    [Fact]
    public void Compile_AddGeometryWithCustomBlockWritesGeometryComponent()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = FixturePaths.Resolve("test_block.geo.json");

        PackTestBuilder.Create()
            .AddGeometry("geometry.test_block", sourcePath)
            .AddBlock<CustomGeometryTestBlock>()
            .Compile(output.Path, verbose: false);

        string blockJson = File.ReadAllText(Path.Combine(output.Path, "bp", "blocks", "custom_geometry_block.json"));
        Assert.Contains("\"minecraft:geometry\": \"geometry.test_block\"", blockJson);
        Assert.True(File.Exists(Path.Combine(output.Path, "rp", "models", "blocks", "test_block.geo.json")));
    }

    [Fact]
    public void Compile_AddGeometryMissingSourceThrows()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string missingPath = Path.Combine(output.Path, "does_not_exist.geo.json");

        FileNotFoundException ex = Assert.Throws<FileNotFoundException>(() =>
            PackTestBuilder.Create()
                .AddGeometry("geometry.missing", missingPath)
                .Compile(output.Path, verbose: false));

        Assert.Contains("geometry.missing", ex.Message);
        Assert.False(File.Exists(Path.Combine(output.Path, "rp", "models", "blocks", "missing.geo.json")));
    }

    [Fact]
    public void AddGeometry_ReturnsPackForFluentChaining()
    {
        Pack pack = PackTestBuilder.Create();
        string sourcePath = FixturePaths.Resolve("test_block.geo.json");

        Pack result = pack
            .AddGeometry("geometry.test_block", sourcePath)
            .AddBlock<TestBlock>();

        Assert.Same(pack, result);
    }

    [Theory]
    [InlineData("", "path.geo.json", "identifier")]
    [InlineData("geometry.test", "", "sourceGeoJsonPath")]
    public void AddGeometry_ThrowsWhenArgumentIsEmpty(string identifier, string sourcePath, string paramName)
    {
        Pack pack = PackTestBuilder.Create();

        ArgumentException ex = Assert.Throws<ArgumentException>(() => pack.AddGeometry(identifier, sourcePath));
        Assert.Equal(paramName, ex.ParamName);
    }
}