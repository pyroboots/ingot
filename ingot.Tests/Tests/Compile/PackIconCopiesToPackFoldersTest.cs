using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Compile;

public class PackIconCopiesToPackFoldersTest
{
    [Fact]
    public void Compile_PackIconCopiesToPackFolders()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string iconPath = FixturePaths.Resolve("manual.png");
        byte[] expectedBytes = File.ReadAllBytes(iconPath);

        Pack pack = Pack.Create(TestUuids.Behaviour, "test pack", "icon test", TestUuids.Resource)
            .AddBlock<TestBlock>();
        pack.PackIcon = iconPath;
        pack.Compile(output.Path, verbose: false);

        // Minecraft requires the pack icon filename to be pack_icon.png
        string behaviourIcon = Path.Combine(output.Path, "bp", "pack_icon.png");
        string resourceIcon = Path.Combine(output.Path, "rp", "pack_icon.png");

        Assert.True(File.Exists(behaviourIcon));
        Assert.True(File.Exists(resourceIcon));
        Assert.Equal(expectedBytes, File.ReadAllBytes(behaviourIcon));
        Assert.Equal(expectedBytes, File.ReadAllBytes(resourceIcon));
    }
}