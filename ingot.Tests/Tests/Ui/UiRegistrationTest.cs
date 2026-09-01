using System.Text.Json;

using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Ui;

public class UiRegistrationTest
{
    [Fact]
    public void Compile_AddUiCopiesJsonToUiFolder()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = WriteUiJson(output.Path, "menu.json", """{ "namespace": "test_menu" }""");
        byte[] expectedBytes = File.ReadAllBytes(sourcePath);

        PackTestBuilder.Create()
            .AddUi(sourcePath)
            .Compile(output.Path, verbose: false);

        string copiedPath = Path.Combine(output.Path, "rp", "ui", "menu.json");
        Assert.True(File.Exists(copiedPath));
        Assert.Equal(expectedBytes, File.ReadAllBytes(copiedPath));
    }

    [Fact]
    public void Compile_AddUiUsesCustomRpName()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = WriteUiJson(output.Path, "menu.json", """{ "namespace": "test_menu" }""");

        PackTestBuilder.Create()
            .AddUi(sourcePath, rpName: "custom_menu")
            .Compile(output.Path, verbose: false);

        Assert.True(File.Exists(Path.Combine(output.Path, "rp", "ui", "custom_menu.json")));
        Assert.False(File.Exists(Path.Combine(output.Path, "rp", "ui", "menu.json")));
    }

    [Fact]
    public void Compile_AddUiSupportsNestedRpName()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = WriteUiJson(output.Path, "menu.json", """{ "namespace": "test_menu" }""");

        PackTestBuilder.Create()
            .AddUi(sourcePath, rpName: "custom/menu")
            .Compile(output.Path, verbose: false);

        Assert.True(File.Exists(Path.Combine(output.Path, "rp", "ui", "custom", "menu.json")));
    }

    [Fact]
    public void Compile_AddUiPreservesJsoncExtensionAndListsItInUiDefs()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = WriteUiJson(output.Path, "global.jsonc", """{ "namespace": "global" }""");

        PackTestBuilder.Create()
            .AddUi(sourcePath, rpName: "starlib2/global")
            .Compile(output.Path, verbose: false);

        string copied = Path.Combine(output.Path, "rp", "ui", "starlib2", "global.jsonc");
        Assert.True(File.Exists(copied));
        Assert.False(File.Exists(Path.Combine(output.Path, "rp", "ui", "starlib2", "global.json")));

        using JsonDocument doc = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(output.Path, "rp", "ui", "_ui_defs.json")));
        Assert.Equal("ui/starlib2/global.jsonc", doc.RootElement.GetProperty("ui_defs")[0].GetString());
    }

    [Fact]
    public void Compile_AddUiWritesUiDefsForCustomNamespaceFiles()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = WriteUiJson(output.Path, "menu.json", """{ "namespace": "test_menu" }""");

        PackTestBuilder.Create()
            .AddUi(sourcePath, rpName: "custom/menu")
            .Compile(output.Path, verbose: false);

        string defsPath = Path.Combine(output.Path, "rp", "ui", "_ui_defs.json");
        Assert.True(File.Exists(defsPath));

        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(defsPath));
        JsonElement defs = doc.RootElement.GetProperty("ui_defs");
        Assert.Equal(1, defs.GetArrayLength());
        Assert.Equal("ui/custom/menu.json", defs[0].GetString());
    }

    [Fact]
    public void Compile_AddUiSkipsUiDefsForVanillaScreenOverlay()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = WriteUiJson(output.Path, "hud_screen.json", """{ "namespace": "hud" }""");

        PackTestBuilder.Create()
            .AddUi(sourcePath, rpName: "hud_screen", includeInUiDefs: false)
            .Compile(output.Path, verbose: false);

        Assert.True(File.Exists(Path.Combine(output.Path, "rp", "ui", "hud_screen.json")));
        Assert.False(File.Exists(Path.Combine(output.Path, "rp", "ui", "_ui_defs.json")));
    }

    [Fact]
    public void Compile_AddUiSkipsUiDefsForSystemFiles()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = WriteUiJson(output.Path, "_global_variables.json", """{ "$test": 1 }""");

        PackTestBuilder.Create()
            .AddUi(sourcePath, rpName: "_global_variables")
            .Compile(output.Path, verbose: false);

        Assert.True(File.Exists(Path.Combine(output.Path, "rp", "ui", "_global_variables.json")));
        Assert.False(File.Exists(Path.Combine(output.Path, "rp", "ui", "_ui_defs.json")));
    }

    [Fact]
    public void Compile_ProvidedUiDefsFileIsCopiedAndNotRegenerated()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string menuPath = WriteUiJson(output.Path, "menu.json", """{ "namespace": "test_menu" }""");
        string defsPath = WriteUiJson(output.Path, "_ui_defs.json", """{ "ui_defs": [ "ui/handwritten.json" ] }""");

        PackTestBuilder.Create()
            .AddUi(menuPath)
            .AddUi(defsPath, rpName: "_ui_defs")
            .Compile(output.Path, verbose: false);

        string compiledDefs = File.ReadAllText(Path.Combine(output.Path, "rp", "ui", "_ui_defs.json"));
        using JsonDocument doc = JsonDocument.Parse(compiledDefs);
        Assert.Equal("ui/handwritten.json", doc.RootElement.GetProperty("ui_defs")[0].GetString());
        Assert.Equal(1, doc.RootElement.GetProperty("ui_defs").GetArrayLength());
    }

    [Fact]
    public void Compile_AddUiTextureCopiesPng()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string texturePath = FixturePaths.Resolve("auto.png");
        byte[] expectedBytes = File.ReadAllBytes(texturePath);

        PackTestBuilder.Create()
            .AddUiTexture("button", texturePath)
            .Compile(output.Path, verbose: false);

        string copiedPath = Path.Combine(output.Path, "rp", "textures", "ui", "button.png");
        Assert.True(File.Exists(copiedPath));
        Assert.Equal(expectedBytes, File.ReadAllBytes(copiedPath));
    }

    [Fact]
    public void Compile_AddUiTextureSupportsNestedKey()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string texturePath = FixturePaths.Resolve("auto.png");

        PackTestBuilder.Create()
            .AddUiTexture("buttons/play", texturePath)
            .Compile(output.Path, verbose: false);

        Assert.True(File.Exists(Path.Combine(output.Path, "rp", "textures", "ui", "buttons", "play.png")));
    }

    [Fact]
    public void Compile_AddUiMissingSourceThrows()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string missingPath = Path.Combine(output.Path, "does_not_exist.json");

        FileNotFoundException ex = Assert.Throws<FileNotFoundException>(() =>
            PackTestBuilder.Create()
                .AddUi(missingPath, rpName: "missing")
                .Compile(output.Path, verbose: false));

        Assert.Contains("missing", ex.Message);
        Assert.False(File.Exists(Path.Combine(output.Path, "rp", "ui", "missing.json")));
    }

    [Fact]
    public void Compile_AddUiTextureMissingSourceThrows()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string missingPath = Path.Combine(output.Path, "does_not_exist.png");

        FileNotFoundException ex = Assert.Throws<FileNotFoundException>(() =>
            PackTestBuilder.Create()
                .AddUiTexture("missing", missingPath)
                .Compile(output.Path, verbose: false));

        Assert.Contains("missing", ex.Message);
    }

    [Fact]
    public void AddUi_ReturnsPackForFluentChaining()
    {
        Pack pack = PackTestBuilder.Create();
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = WriteUiJson(output.Path, "menu.json", """{ "namespace": "test_menu" }""");

        Pack result = pack
            .AddUi(sourcePath)
            .AddUiTexture("button", FixturePaths.Resolve("auto.png"));

        Assert.Same(pack, result);
        Assert.Contains("menu", pack.ResourcePack.UiIds);
        Assert.Contains("button", pack.ResourcePack.UiTextureKeys);
    }

    [Theory]
    [InlineData("", "menu", "sourceJsonPath")]
    public void AddUi_ThrowsWhenArgumentIsEmpty(string sourcePath, string rpName, string paramName)
    {
        Pack pack = PackTestBuilder.Create();

        ArgumentException ex = Assert.Throws<ArgumentException>(() => pack.AddUi(sourcePath, rpName));
        Assert.Equal(paramName, ex.ParamName);
    }

    [Theory]
    [InlineData("", "path.png", "key")]
    [InlineData("button", "", "sourcePngPath")]
    public void AddUiTexture_ThrowsWhenArgumentIsEmpty(string key, string sourcePath, string paramName)
    {
        Pack pack = PackTestBuilder.Create();

        ArgumentException ex = Assert.Throws<ArgumentException>(() => pack.AddUiTexture(key, sourcePath));
        Assert.Equal(paramName, ex.ParamName);
    }

    [Fact]
    public void Compile_CreatesUiFolderStructure()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();

        PackTestBuilder.Create()
            .Compile(output.Path, verbose: false);

        Assert.True(Directory.Exists(Path.Combine(output.Path, "rp", "ui")));
        Assert.True(Directory.Exists(Path.Combine(output.Path, "rp", "textures", "ui")));
    }

    private static string WriteUiJson(string dir, string fileName, string contents)
    {
        string path = Path.Combine(dir, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, contents);
        return path;
    }
}
