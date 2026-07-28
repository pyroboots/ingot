using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Particles;

public class ParticleRegistrationTest
{
    [Fact]
    public void Compile_AddParticleCopiesJsonToParticlesFolder()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = FixturePaths.Resolve("test_sparkle.json");
        byte[] expectedBytes = File.ReadAllBytes(sourcePath);

        PackTestBuilder.Create()
            .AddParticle("test:sparkle", sourcePath)
            .Compile(output.Path, verbose: false);

        string copiedPath = Path.Combine(output.Path, "rp", "particles", "sparkle.json");
        Assert.True(File.Exists(copiedPath));
        Assert.Equal(expectedBytes, File.ReadAllBytes(copiedPath));
    }

    [Fact]
    public void Compile_AddParticleUsesCustomRpName()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = FixturePaths.Resolve("test_sparkle.json");

        PackTestBuilder.Create()
            .AddParticle("test:sparkle", sourcePath, rpName: "custom_sparkle")
            .Compile(output.Path, verbose: false);

        Assert.True(File.Exists(Path.Combine(output.Path, "rp", "particles", "custom_sparkle.json")));
        Assert.False(File.Exists(Path.Combine(output.Path, "rp", "particles", "sparkle.json")));
    }

    [Fact]
    public void Compile_AddParticleSupportsNestedRpName()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = FixturePaths.Resolve("test_sparkle.json");

        PackTestBuilder.Create()
            .AddParticle("test:sparkle", sourcePath, rpName: "effects/sparkle")
            .Compile(output.Path, verbose: false);

        Assert.True(File.Exists(Path.Combine(output.Path, "rp", "particles", "effects", "sparkle.json")));
    }

    [Fact]
    public void Compile_AddParticleTextureCopiesPng()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string texturePath = FixturePaths.Resolve("auto.png");
        byte[] expectedBytes = File.ReadAllBytes(texturePath);

        PackTestBuilder.Create()
            .AddParticleTexture("sparkle", texturePath)
            .Compile(output.Path, verbose: false);

        string copiedPath = Path.Combine(output.Path, "rp", "textures", "particles", "sparkle.png");
        Assert.True(File.Exists(copiedPath));
        Assert.Equal(expectedBytes, File.ReadAllBytes(copiedPath));
    }

    [Fact]
    public void Compile_AddParticleTextureSupportsNestedKey()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string texturePath = FixturePaths.Resolve("auto.png");

        PackTestBuilder.Create()
            .AddParticleTexture("effects/sparkle", texturePath)
            .Compile(output.Path, verbose: false);

        Assert.True(File.Exists(Path.Combine(output.Path, "rp", "textures", "particles", "effects", "sparkle.png")));
    }

    [Fact]
    public void Compile_AddParticleMissingSourceThrows()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string missingPath = Path.Combine(output.Path, "does_not_exist.json");

        FileNotFoundException ex = Assert.Throws<FileNotFoundException>(() =>
            PackTestBuilder.Create()
                .AddParticle("test:missing", missingPath)
                .Compile(output.Path, verbose: false));

        Assert.Contains("test:missing", ex.Message);
        Assert.False(File.Exists(Path.Combine(output.Path, "rp", "particles", "missing.json")));
    }

    [Fact]
    public void Compile_AddParticleTextureMissingSourceThrows()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string missingPath = Path.Combine(output.Path, "does_not_exist.png");

        FileNotFoundException ex = Assert.Throws<FileNotFoundException>(() =>
            PackTestBuilder.Create()
                .AddParticleTexture("missing", missingPath)
                .Compile(output.Path, verbose: false));

        Assert.Contains("missing", ex.Message);
    }

    [Fact]
    public void AddParticle_ReturnsPackForFluentChaining()
    {
        Pack pack = PackTestBuilder.Create();
        string sourcePath = FixturePaths.Resolve("test_sparkle.json");

        Pack result = pack
            .AddParticle("test:sparkle", sourcePath)
            .AddParticleTexture("sparkle", FixturePaths.Resolve("auto.png"));

        Assert.Same(pack, result);
        Assert.Contains("test:sparkle", pack.ResourcePack.ParticleIds);
        Assert.Contains("sparkle", pack.ResourcePack.ParticleTextureKeys);
    }

    [Theory]
    [InlineData("", "path.json", "identifier")]
    [InlineData("test:sparkle", "", "sourceJsonPath")]
    public void AddParticle_ThrowsWhenArgumentIsEmpty(string identifier, string sourcePath, string paramName)
    {
        Pack pack = PackTestBuilder.Create();

        ArgumentException ex = Assert.Throws<ArgumentException>(() => pack.AddParticle(identifier, sourcePath));
        Assert.Equal(paramName, ex.ParamName);
    }

    [Theory]
    [InlineData("", "path.png", "key")]
    [InlineData("sparkle", "", "sourcePngPath")]
    public void AddParticleTexture_ThrowsWhenArgumentIsEmpty(string key, string sourcePath, string paramName)
    {
        Pack pack = PackTestBuilder.Create();

        ArgumentException ex = Assert.Throws<ArgumentException>(() => pack.AddParticleTexture(key, sourcePath));
        Assert.Equal(paramName, ex.ParamName);
    }

    [Fact]
    public void Compile_CreatesParticlesFolderStructure()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();

        PackTestBuilder.Create()
            .Compile(output.Path, verbose: false);

        Assert.True(Directory.Exists(Path.Combine(output.Path, "rp", "particles")));
        Assert.True(Directory.Exists(Path.Combine(output.Path, "rp", "textures", "particles")));
    }
}
