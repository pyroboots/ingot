using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Compile;

public class ScriptsEnabledWritesScriptModuleTest
{
    [Fact]
    public void Compile_scriptsEnabled_writesScriptModuleInManifest()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            Pack pack = PackTestBuilder.Create().AddBlock<TestBlock>();
            pack.ScriptsEnabled = true;
            pack.Compile(outputDir, verbose: false);

            string manifest = File.ReadAllText(Path.Combine(outputDir, "bp", "manifest.json"));
            Assert.Contains("\"type\": \"script\"", manifest);
            Assert.Contains("scripts/main.js", manifest);
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}