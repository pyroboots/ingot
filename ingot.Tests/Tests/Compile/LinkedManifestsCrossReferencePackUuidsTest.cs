using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Compile;

public class LinkedManifestsCrossReferencePackUuidsTest
{
    [Fact]
    public void Compile_LinkedManifests_CrossReferencePackUuids()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            Pack pack = Pack.Create(TestUuids.Behaviour, "test pack", "manifest test", TestUuids.Resource)
                .AddBlock<TestBlock>();

            pack.Compile(output.Path, verbose: false);

            string bpManifest = File.ReadAllText(Path.Combine(output.Path, "bp", "manifest.json"));
            string rpManifest = File.ReadAllText(Path.Combine(output.Path, "rp", "manifest.json"));

            Assert.Contains(TestUuids.Resource, bpManifest);
            Assert.Contains(TestUuids.Behaviour, rpManifest);
        }
    }
}