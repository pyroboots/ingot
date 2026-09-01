using ingot.Core;
using ingot.Tests.Support;

namespace ingot.Tests.Resources;

public class ExtraFileRegistrationTest
{
    [Fact]
    public void Compile_AddResourceFile_CopiesToRelativePath()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = WriteFile(output.Path, "form.json", """{ "nineslice_size": 6 }""");
        byte[] expectedBytes = File.ReadAllBytes(sourcePath);

        PackTestBuilder.Create()
            .AddResourceFile(sourcePath, "textures/qwo/background/form.json")
            .Compile(output.Path, verbose: false);

        string copied = Path.Combine(output.Path, "rp", "textures", "qwo", "background", "form.json");
        Assert.True(File.Exists(copied));
        Assert.Equal(expectedBytes, File.ReadAllBytes(copied));
    }

    [Fact]
    public void Compile_AddResourceTree_PreservesLayoutAndSkipsCache()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string tree = Path.Combine(output.Path, "qwo");
        WriteFile(tree, Path.Combine("ui", "server_form.json"), """{ "namespace": "server_form" }""");
        WriteFile(tree, Path.Combine("ui", "starlib2", "global.jsonc"), """{ "namespace": "global" }""");
        WriteFile(tree, Path.Combine("textures", "qwo", "button", "default.png"), "png");
        WriteFile(tree, Path.Combine("textures", "qwo", "button", "default.json"), """{ "nineslice_size": 6 }""");
        WriteFile(tree, Path.Combine("textures", "qwo", "button", "Gallery.cache"), "junk");
        WriteFile(tree, Path.Combine("ui", ".hidden"), "skip");

        PackTestBuilder.Create()
            .AddResourceTree(tree)
            .Compile(output.Path, verbose: false);

        string rp = Path.Combine(output.Path, "rp");
        Assert.True(File.Exists(Path.Combine(rp, "ui", "server_form.json")));
        Assert.True(File.Exists(Path.Combine(rp, "ui", "starlib2", "global.jsonc")));
        Assert.True(File.Exists(Path.Combine(rp, "textures", "qwo", "button", "default.png")));
        Assert.True(File.Exists(Path.Combine(rp, "textures", "qwo", "button", "default.json")));
        Assert.False(File.Exists(Path.Combine(rp, "textures", "qwo", "button", "Gallery.cache")));
        Assert.False(File.Exists(Path.Combine(rp, "ui", ".hidden")));
    }

    [Fact]
    public void Compile_AddResourceTree_AppliesRelativePrefix()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string tree = Path.Combine(output.Path, "screens");
        WriteFile(tree, "menu.json", """{ "namespace": "menu" }""");

        PackTestBuilder.Create()
            .AddResourceTree(tree, "ui/custom")
            .Compile(output.Path, verbose: false);

        Assert.True(File.Exists(Path.Combine(output.Path, "rp", "ui", "custom", "menu.json")));
    }

    [Fact]
    public void Compile_AddResourceTree_MissingDirectoryThrows()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string missing = Path.Combine(output.Path, "does_not_exist");

        DirectoryNotFoundException ex = Assert.Throws<DirectoryNotFoundException>(() =>
            PackTestBuilder.Create().AddResourceTree(missing));

        Assert.Contains("does_not_exist", ex.Message);
    }

    [Fact]
    public void Compile_AddScriptFile_CopiesAsIsAndIsNotImportedFromMain()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = WriteFile(output.Path, "helper.js", "export const VALUE = 1;\n");

        Pack pack = PackTestBuilder.Create();
        pack.ScriptsEnabled = true;
        pack.AddScriptFile(sourcePath)
            .AddScriptEvent("test:hello", "world.sendMessage(\"hi\");")
            .Compile(output.Path, verbose: false);

        string copied = Path.Combine(output.Path, "bp", "scripts", "helper.js");
        Assert.True(File.Exists(copied));
        Assert.Equal("export const VALUE = 1;\n", File.ReadAllText(copied));

        string mainJs = File.ReadAllText(Path.Combine(output.Path, "bp", "scripts", "main.js"));
        Assert.DoesNotContain("./helper.js", mainJs);
        Assert.Contains("./events/test_hello.js", mainJs);
    }

    [Fact]
    public void Compile_AddScriptFile_NestedRelativePath()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string sourcePath = WriteFile(output.Path, "builder.js", "export class Builder {}\n");

        Pack pack = PackTestBuilder.Create();
        pack.AddScriptFile(sourcePath, "lib/builder.js")
            .Compile(output.Path, verbose: false);

        Assert.True(File.Exists(Path.Combine(output.Path, "bp", "scripts", "lib", "builder.js")));
    }

    [Fact]
    public void AddResourceFile_RejectsParentSegments()
    {
        Pack pack = PackTestBuilder.Create();
        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            pack.AddResourceFile("file.png", "../escape.png"));
        Assert.Equal("relativePath", ex.ParamName);
    }

    [Theory]
    [InlineData("", "ui/file.json", "sourcePath")]
    [InlineData("file.json", "", "relativePath")]
    public void AddResourceFile_ThrowsWhenArgumentIsEmpty(string sourcePath, string relativePath, string paramName)
    {
        Pack pack = PackTestBuilder.Create();
        ArgumentException ex = Assert.Throws<ArgumentException>(() => pack.AddResourceFile(sourcePath, relativePath));
        Assert.Equal(paramName, ex.ParamName);
    }

    private static string WriteFile(string root, string relativePath, string contents)
    {
        string path = Path.Combine(root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, contents);
        return path;
    }
}
