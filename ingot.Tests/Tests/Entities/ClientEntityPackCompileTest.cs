using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Content.Entities;
using ingot.Tests.Support;

namespace ingot.Tests.Entities;

public class ClientEntityPackCompileTest
{
    [Fact]
    public void Compile_ClientEntityWritesRpEntityAndDefaultRenderController()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();

        PackTestBuilder.Create()
            .AddEntity<TestEntity>()
            .AddClientEntity<TestClientEntity>()
            .Compile(output.Path, verbose: false);

        string entityPath = Path.Combine(output.Path, "rp", "entity", "test_entity.json");
        string rcPath = Path.Combine(output.Path, "rp", "render_controllers", "test_entity.json");

        Assert.True(File.Exists(entityPath), "expected rp/entity/test_entity.json");
        Assert.True(File.Exists(rcPath), "expected auto-emitted rp/render_controllers/test_entity.json");

        string entityJson = File.ReadAllText(entityPath);
        Assert.Contains("minecraft:client_entity", entityJson);
        Assert.Contains("controller.render.test_entity", entityJson);

        string rcJson = File.ReadAllText(rcPath);
        Assert.Contains("controller.render.test_entity", rcJson);
        Assert.Contains("Geometry.default", rcJson);
    }

    [Fact]
    public void Compile_CustomRenderControllerWritesToRp()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();

        PackTestBuilder.Create()
            .AddRenderController<TestRenderController>()
            .Compile(output.Path, verbose: false);

        string rcPath = Path.Combine(output.Path, "rp", "render_controllers", "test_entity_custom.json");
        Assert.True(File.Exists(rcPath));
        Assert.Contains("controller.render.test_entity_custom", File.ReadAllText(rcPath));
    }

    [Fact]
    public void Compile_EntitySoundsWritesSoundsJson()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();

        PackTestBuilder.Create()
            .AddEntity<TestEntity>()
            .AddClientEntity<TestClientEntity>()
            .Compile(output.Path, verbose: false);

        string soundsPath = Path.Combine(output.Path, "rp", "sounds.json");
        Assert.True(File.Exists(soundsPath), "expected rp/sounds.json");

        string soundsJson = File.ReadAllText(soundsPath);
        Assert.Contains("entity_sounds", soundsJson);
        Assert.Contains("\"test:test_entity\"", soundsJson);
        Assert.Contains("mob.test.say", soundsJson);
        Assert.Contains("mob.test.hurt", soundsJson);
        Assert.Contains("\"ambient\"", soundsJson);
        Assert.Contains("\"hurt\"", soundsJson);
    }

    [Fact]
    public void Compile_WithoutEntitySoundsWritesEmptySoundsJson()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();

        PackTestBuilder.Create()
            .AddEntity<TestEntity>(discoverClient: false)
            .AddClientEntity<TestClientEntityWithTexturePath>()
            .Compile(output.Path, verbose: false);

        string soundsPath = Path.Combine(output.Path, "rp", "sounds.json");
        Assert.True(File.Exists(soundsPath));
        string soundsJson = File.ReadAllText(soundsPath).Trim();
        Assert.DoesNotContain("entity_sounds", soundsJson);
    }

    [Fact]
    public void Compile_DefaultTexturePathCopiesEntityPng()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = FixturePaths.Resolve("auto.png");
        byte[] expectedBytes = File.ReadAllBytes(sourcePath);

        PackTestBuilder.Create()
            .AddEntity<TestEntity>()
            .AddClientEntity<TestClientEntityWithTexturePath>()
            .Compile(output.Path, verbose: false);

        string copiedPath = Path.Combine(output.Path, "rp", "textures", "entity", "test_entity.png");
        Assert.True(File.Exists(copiedPath), "expected rp/textures/entity/test_entity.png from DefaultTexturePath");
        Assert.Equal(expectedBytes, File.ReadAllBytes(copiedPath));

        string entityJson = File.ReadAllText(Path.Combine(output.Path, "rp", "entity", "test_entity.json"));
        Assert.Contains("textures/entity/test_entity", entityJson);
    }

    [Fact]
    public void Compile_DefaultTexturePathCopiesNestedEntityPng()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = FixturePaths.Resolve("auto.png");
        byte[] expectedBytes = File.ReadAllBytes(sourcePath);

        PackTestBuilder.Create()
            .AddEntity<TestEntity>()
            .AddClientEntity<TestClientEntityWithNestedTexturePath>()
            .Compile(output.Path, verbose: false);

        string copiedPath = Path.Combine(output.Path, "rp", "textures", "entity", "mobs", "test_entity.png");
        Assert.True(File.Exists(copiedPath), "expected rp/textures/entity/mobs/test_entity.png for nested DefaultTexture");
        Assert.Equal(expectedBytes, File.ReadAllBytes(copiedPath));

        string entityJson = File.ReadAllText(Path.Combine(output.Path, "rp", "entity", "test_entity.json"));
        Assert.Contains("textures/entity/mobs/test_entity", entityJson);
    }

    [Fact]
    public void Compile_AddEntityTextureCopiesPng()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = FixturePaths.Resolve("manual.png");
        byte[] expectedBytes = File.ReadAllBytes(sourcePath);

        PackTestBuilder.Create()
            .AddEntityTexture("test_entity", sourcePath)
            .Compile(output.Path, verbose: false);

        string copiedPath = Path.Combine(output.Path, "rp", "textures", "entity", "test_entity.png");
        Assert.True(File.Exists(copiedPath), "expected rp/textures/entity/test_entity.png from AddEntityTexture");
        Assert.Equal(expectedBytes, File.ReadAllBytes(copiedPath));
    }

    [Fact]
    public void Compile_AddEntityTextureCopiesNestedPng()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = FixturePaths.Resolve("manual.png");
        byte[] expectedBytes = File.ReadAllBytes(sourcePath);

        PackTestBuilder.Create()
            .AddEntityTexture("mobs/test_entity", sourcePath)
            .Compile(output.Path, verbose: false);

        string copiedPath = Path.Combine(output.Path, "rp", "textures", "entity", "mobs", "test_entity.png");
        Assert.True(File.Exists(copiedPath), "expected rp/textures/entity/mobs/test_entity.png from nested AddEntityTexture key");
        Assert.Equal(expectedBytes, File.ReadAllBytes(copiedPath));
    }

    [Fact]
    public void Compile_ManualEntityTextureOverridesDefaultTexturePath()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string autoPath = FixturePaths.Resolve("auto.png");
        string manualPath = FixturePaths.Resolve("manual.png");
        byte[] expectedBytes = File.ReadAllBytes(manualPath);

        // Manual registration first; DefaultTexturePath auto-register should not overwrite.
        Pack.Create(TestUuids.Behaviour, "test pack", "entity texture override", TestUuids.Resource)
            .AddEntityTexture("test_entity", manualPath)
            .AddEntity<TestEntity>()
            .AddClientEntity<TestClientEntityWithTexturePath>()
            .Compile(output.Path, verbose: false);

        string copiedPath = Path.Combine(output.Path, "rp", "textures", "entity", "test_entity.png");
        Assert.True(File.Exists(copiedPath));
        Assert.Equal(expectedBytes, File.ReadAllBytes(copiedPath));
        Assert.NotEqual(File.ReadAllBytes(autoPath), File.ReadAllBytes(copiedPath));
    }
}
