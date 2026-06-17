using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Compile;

public class LinkedManifestsCrossReferencePackUuidsTest
{
    [Fact]
    public void Compile_linkedManifests_crossReferencePackUuids()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            Pack pack = Pack.Create(TestUuids.Behaviour, "test pack", "manifest test", TestUuids.Resource)
                .AddBlock<TestBlock>();

            pack.Compile(outputDir, verbose: false);

            string bpManifest = File.ReadAllText(Path.Combine(outputDir, "bp", "manifest.json"));
            string rpManifest = File.ReadAllText(Path.Combine(outputDir, "rp", "manifest.json"));

            Assert.Contains(TestUuids.Resource, bpManifest);
            Assert.Contains(TestUuids.Behaviour, rpManifest);
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}