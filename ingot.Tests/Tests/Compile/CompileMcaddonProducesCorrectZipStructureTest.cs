using System.IO.Compression;

using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Compile;

public class CompileMcaddonProducesCorrectZipStructureTest
{
    [Fact]
    public void CompileMcaddon_ProducesCorrectZipStructureAndRemovesTempOutput()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string mcaddonPath = Path.Combine(output.Path, "test.mcaddon");

        Pack pack = Pack.Create(TestUuids.Behaviour, "test pack", "compile test", TestUuids.Resource)
            .AddBlock<TestBlock>();

        pack.CompileMcaddon(mcaddonPath, verbose: false);

        Assert.True(File.Exists(mcaddonPath));
        Assert.False(Directory.Exists(Path.Combine(output.Path, "bp")));
        Assert.False(Directory.Exists(Path.Combine(output.Path, "rp")));

        using ZipArchive zip = ZipFile.OpenRead(mcaddonPath);
        string[] entries = zip.Entries.Select(e => e.FullName).ToArray();

        Assert.Contains("test pack BP/manifest.json", entries);
        Assert.Contains("test pack RP/manifest.json", entries);
        Assert.Contains("test pack BP/blocks/test_block.json", entries);
        Assert.DoesNotContain(entries, e => e.StartsWith("bp/", StringComparison.Ordinal));
        Assert.DoesNotContain(entries, e => e.StartsWith("rp/", StringComparison.Ordinal));
        Assert.DoesNotContain(entries, e => e.StartsWith("pack/", StringComparison.Ordinal));
        Assert.DoesNotContain(entries, e => e.Contains("ingot.log", StringComparison.Ordinal));
        Assert.DoesNotContain(entries, e => e.Contains(".ingot", StringComparison.Ordinal));
    }
}