using System.Text.Json;

using ingot.Core;
using ingot.Core.Resource;
using ingot.Tests.Support;

using Sound = ingot.Core.Resource.SoundDefinitions.SoundDefinition.Sound;
using SoundCategory = ingot.Core.Resource.SoundDefinitions.SoundDefinition.SoundDefinitionCategory;

namespace ingot.Tests.Sounds;

public class SoundDefinitionRegistrationTest
{
    [Fact]
    public void Compile_RegisterSoundDefinitionWritesSoundDefinitionsJson()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();

        PackTestBuilder.Create()
            .RegisterSoundDefinition(
                "ambient.basalt_deltas.additions",
                [
                    Sound.Reference(
                        "sounds/ambient/nether/basalt_deltas/basaltground1",
                        volume: 0.55f,
                        weight: 10,
                        is3D: false),
                    Sound.Reference(
                        "sounds/ambient/nether/basalt_deltas/click1",
                        volume: 0.19f,
                        weight: 20,
                        is3D: false),
                ],
                category: SoundCategory.Ambient)
            .RegisterSoundDefinition(
                "ambient.basalt_deltas.loop",
                [
                    Sound.Reference(
                        "sounds/ambient/nether/basalt_deltas/ambience",
                        volume: 4.0f,
                        is3D: false,
                        stream: true),
                ],
                category: SoundCategory.Ambient,
                maxDistance: 64f,
                minDistance: 8f)
            .Compile(output.Path, verbose: false);

        string path = Path.Combine(output.Path, "rp", "sounds", "sound_definitions.json");
        Assert.True(File.Exists(path));

        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(path));
        JsonElement root = doc.RootElement;

        Assert.Equal("1.20.20", root.GetProperty("format_version").GetString());

        JsonElement defs = root.GetProperty("sound_definitions");
        Assert.True(defs.TryGetProperty("ambient.basalt_deltas.additions", out JsonElement additions));
        Assert.Equal("ambient", additions.GetProperty("category").GetString());
        Assert.Equal(JsonValueKind.Null, additions.GetProperty("max_distance").ValueKind);
        Assert.Equal(JsonValueKind.Null, additions.GetProperty("min_distance").ValueKind);

        JsonElement additionSounds = additions.GetProperty("sounds");
        Assert.Equal(2, additionSounds.GetArrayLength());
        Assert.Equal(
            "sounds/ambient/nether/basalt_deltas/basaltground1",
            additionSounds[0].GetProperty("name").GetString());
        Assert.False(additionSounds[0].GetProperty("is3D").GetBoolean());
        Assert.Equal(0.55, additionSounds[0].GetProperty("volume").GetDouble(), precision: 3);
        Assert.Equal(10, additionSounds[0].GetProperty("weight").GetInt32());

        Assert.True(defs.TryGetProperty("ambient.basalt_deltas.loop", out JsonElement loop));
        Assert.Equal(64f, loop.GetProperty("max_distance").GetSingle());
        Assert.Equal(8f, loop.GetProperty("min_distance").GetSingle());
        Assert.True(loop.GetProperty("sounds")[0].GetProperty("stream").GetBoolean());
    }

    [Fact]
    public void Compile_RegisterSoundDefinitionCopiesSourceFilesIntoNestedSoundsDirs()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string assets = Path.Combine(output.Path, "assets");
        Directory.CreateDirectory(assets);

        string groundSource = Path.Combine(assets, "basaltground1.ogg");
        string clickSource = Path.Combine(assets, "click1.fsb");
        string ambienceSource = Path.Combine(assets, "ambience.ogg");
        File.WriteAllBytes(groundSource, [0x4F, 0x67, 0x67, 0x53]); // "OggS" header stub
        File.WriteAllBytes(clickSource, [0x01, 0x02, 0x03]);
        File.WriteAllBytes(ambienceSource, [0x4F, 0x67, 0x67, 0x53, 0x00]);

        PackTestBuilder.Create()
            .RegisterSoundDefinition(
                "ambient.basalt_deltas.additions",
                [
                    Sound.Create(
                        groundSource,
                        "sounds/ambient/nether/basalt_deltas/basaltground1",
                        volume: 0.55f,
                        weight: 10,
                        is3D: false),
                    Sound.Create(
                        clickSource,
                        "sounds/ambient/nether/basalt_deltas/click1",
                        volume: 0.19f,
                        weight: 20,
                        is3D: false),
                ],
                category: SoundCategory.Ambient)
            .RegisterSoundDefinition(
                "ambient.basalt_deltas.loop",
                [
                    Sound.Create(
                        ambienceSource,
                        "sounds/ambient/nether/basalt_deltas/ambience",
                        volume: 4.0f,
                        is3D: false,
                        stream: true),
                ],
                category: SoundCategory.Ambient)
            .Compile(output.Path, verbose: false);

        string groundTarget = Path.Combine(
            output.Path, "rp", "sounds", "ambient", "nether", "basalt_deltas", "basaltground1.ogg");
        string clickTarget = Path.Combine(
            output.Path, "rp", "sounds", "ambient", "nether", "basalt_deltas", "click1.fsb");
        string ambienceTarget = Path.Combine(
            output.Path, "rp", "sounds", "ambient", "nether", "basalt_deltas", "ambience.ogg");

        Assert.True(File.Exists(groundTarget), $"expected {groundTarget}");
        Assert.True(File.Exists(clickTarget), $"expected {clickTarget}");
        Assert.True(File.Exists(ambienceTarget), $"expected {ambienceTarget}");
        Assert.Equal(File.ReadAllBytes(groundSource), File.ReadAllBytes(groundTarget));
        Assert.Equal(File.ReadAllBytes(clickSource), File.ReadAllBytes(clickTarget));
        Assert.Equal(File.ReadAllBytes(ambienceSource), File.ReadAllBytes(ambienceTarget));

        // JSON name stays extensionless even when the copied file has an extension
        using JsonDocument doc = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(output.Path, "rp", "sounds", "sound_definitions.json")));
        Assert.Equal(
            "sounds/ambient/nether/basalt_deltas/basaltground1",
            doc.RootElement
                .GetProperty("sound_definitions")
                .GetProperty("ambient.basalt_deltas.additions")
                .GetProperty("sounds")[0]
                .GetProperty("name")
                .GetString());
    }

    [Fact]
    public void Compile_PathWithoutNameAutoResolvesPackPathFromCategory()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string source = Path.Combine(output.Path, "toot.ogg");
        File.WriteAllBytes(source, [0x4F, 0x67, 0x67, 0x53]);

        PackTestBuilder.Create()
            .RegisterSoundDefinition(
                "example.toot",
                [Sound.Create(source)],
                category: SoundCategory.Block)
            .Compile(output.Path, verbose: false);

        string target = Path.Combine(output.Path, "rp", "sounds", "block", "toot.ogg");
        Assert.True(File.Exists(target), $"expected {target}");

        using JsonDocument doc = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(output.Path, "rp", "sounds", "sound_definitions.json")));
        Assert.Equal(
            "sounds/block/toot",
            doc.RootElement
                .GetProperty("sound_definitions")
                .GetProperty("example.toot")
                .GetProperty("sounds")[0]
                .GetProperty("name")
                .GetString());
    }

    [Fact]
    public void Compile_SameSoundSourceSharedAcrossDefinitionsCopiedOnce()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string source = Path.Combine(output.Path, "shared.ogg");
        File.WriteAllBytes(source, [0xAA, 0xBB]);

        PackTestBuilder.Create()
            .RegisterSoundDefinition(
                "example.a",
                [Sound.Create(source, "sounds/custom/shared")],
                category: SoundCategory.Neutral)
            .RegisterSoundDefinition(
                "example.b",
                [Sound.Create(source, "sounds/custom/shared")],
                category: SoundCategory.Neutral)
            .Compile(output.Path, verbose: false);

        string target = Path.Combine(output.Path, "rp", "sounds", "custom", "shared.ogg");
        Assert.True(File.Exists(target));
        Assert.Equal(File.ReadAllBytes(source), File.ReadAllBytes(target));
    }

    [Fact]
    public void Compile_MissingSoundSourceThrows()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string missing = Path.Combine(output.Path, "does_not_exist.ogg");

        FileNotFoundException ex = Assert.Throws<FileNotFoundException>(() =>
            PackTestBuilder.Create()
                .RegisterSoundDefinition(
                    "example.toot",
                    [Sound.Create(missing, "sounds/trumpet")],
                    category: SoundCategory.Neutral)
                .Compile(output.Path, verbose: false));

        Assert.Contains("trumpet", ex.Message);
        Assert.False(File.Exists(Path.Combine(output.Path, "rp", "sounds", "trumpet.ogg")));
    }

    [Fact]
    public void Compile_ConflictingSourcesForSameRpPathThrows()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string a = Path.Combine(output.Path, "a.ogg");
        string b = Path.Combine(output.Path, "b.ogg");
        File.WriteAllBytes(a, [0x01]);
        File.WriteAllBytes(b, [0x02]);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            PackTestBuilder.Create()
                .RegisterSoundDefinition(
                    "example.a",
                    [Sound.Create(a, "sounds/custom/note")])
                .RegisterSoundDefinition(
                    "example.b",
                    [Sound.Create(b, "sounds/custom/note")])
                .Compile(output.Path, verbose: false));

        Assert.Contains("sounds/custom/note.ogg", ex.Message);
    }

    [Fact]
    public void Compile_WithoutSoundDefinitionsWritesEmptySoundDefinitionsJson()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();

        PackTestBuilder.Create().Compile(output.Path, verbose: false);

        string path = Path.Combine(output.Path, "rp", "sounds", "sound_definitions.json");
        Assert.True(File.Exists(path));

        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(path));
        JsonElement root = doc.RootElement;
        Assert.Equal("1.20.20", root.GetProperty("format_version").GetString());
        Assert.Empty(root.GetProperty("sound_definitions").EnumerateObject());
    }

    [Fact]
    public void RegisterSoundDefinition_ReturnsPackForFluentChaining()
    {
        Pack pack = PackTestBuilder.Create();

        Pack result = pack.RegisterSoundDefinition(
            "example.toot",
            [Sound.Reference("sounds/trumpet")],
            category: SoundCategory.Neutral);

        Assert.Same(pack, result);
        Assert.Contains("example.toot", pack.ResourcePack.SoundDefinitionIds);
    }

    [Fact]
    public void RegisterSoundDefinition_ThrowsWhenSoundIdIsEmpty()
    {
        Pack pack = PackTestBuilder.Create();

        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            pack.RegisterSoundDefinition("", [Sound.Reference("sounds/trumpet")]));
        Assert.Equal("soundId", ex.ParamName);
    }

    [Fact]
    public void RegisterSoundDefinition_ThrowsWhenSoundsEmpty()
    {
        Pack pack = PackTestBuilder.Create();

        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            pack.RegisterSoundDefinition("example.toot", []));
        Assert.Equal("sounds", ex.ParamName);
    }

    [Fact]
    public void RegisterSoundDefinition_ThrowsWhenPathAndNameEmpty()
    {
        Pack pack = PackTestBuilder.Create();

        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            pack.RegisterSoundDefinition("example.toot", [new Sound("  ", "  ")]));
        Assert.Equal("sounds", ex.ParamName);
    }

    [Fact]
    public void RegisterSoundDefinition_LaterRegistrationOverwritesSameId()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();

        PackTestBuilder.Create()
            .RegisterSoundDefinition(
                "example.toot",
                [Sound.Reference("sounds/old")],
                category: SoundCategory.Block)
            .RegisterSoundDefinition(
                "example.toot",
                [Sound.Reference("sounds/new")],
                category: SoundCategory.Music)
            .Compile(output.Path, verbose: false);

        string path = Path.Combine(output.Path, "rp", "sounds", "sound_definitions.json");
        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(path));
        JsonElement def = doc.RootElement.GetProperty("sound_definitions").GetProperty("example.toot");

        Assert.Equal("music", def.GetProperty("category").GetString());
        Assert.Equal("sounds/new", def.GetProperty("sounds")[0].GetProperty("name").GetString());
    }
}
