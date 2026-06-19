using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Compile;

public class ScriptsEnabledWritesScriptModuleTest
{
    [Fact]
    public void Compile_ScriptsEnabled_WritesScriptModuleInManifest()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            Pack pack = PackTestBuilder.Create().AddBlock<TestBlock>();
            pack.ScriptsEnabled = true;
            pack.Compile(output.Path, verbose: false);

            string manifest = File.ReadAllText(Path.Combine(output.Path, "bp", "manifest.json"));
            Assert.Contains("\"type\": \"script\"", manifest);
            Assert.Contains("scripts/main.js", manifest);
        }
    }
}