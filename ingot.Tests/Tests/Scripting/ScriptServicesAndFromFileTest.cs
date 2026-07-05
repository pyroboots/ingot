using ingot.Core;
using ingot.Tests.Content;
using ingot.Tests.Content.Blocks;
using ingot.Tests.Support;

namespace ingot.Tests.Scripting;

public class ScriptServicesAndFromFileTest
{
    [Fact]
    public void Compile_Service_WritesServiceScriptAndImportsInMainJs()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            Pack pack = PackTestBuilder.Create();
            pack.ScriptsEnabled = true;
            pack.AddService(FixturePaths.Resolve("scripts/tick_service.js"));
            pack.Compile(output.Path, verbose: false);

            string servicePath = Path.Combine(output.Path, "bp", "scripts", "services", "tick_service.js");
            Assert.True(File.Exists(servicePath));
            Assert.Contains("runInterval", File.ReadAllText(servicePath));

            string mainJs = File.ReadAllText(Path.Combine(output.Path, "bp", "scripts", "main.js"));
            Assert.Contains("./services/tick_service.js", mainJs);
        }
    }

    [Fact]
    public void Compile_BlockEventsFromFile_WritesHandlerBodyFromFile()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            Pack pack = PackTestBuilder.Create().AddBlock<BlockEventsFromFileTestBlock>();
            pack.ScriptsEnabled = true;
            pack.Compile(output.Path, verbose: false);

            string script = File.ReadAllText(Path.Combine(output.Path, "bp", "scripts", "blocks", "test_events_from_file_block_events.js"));
            Assert.Contains("event.block.setType(\"minecraft:stone\");", script);
        }
    }

    [Fact]
    public void Compile_BlockEventsMissingTrait_WritesWarningToLog()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            Pack pack = PackTestBuilder.Create().AddBlock<BlockEventsMissingTraitTestBlock>();
            pack.ScriptsEnabled = true;
            pack.Compile(output.Path, verbose: true);

            string log = File.ReadAllText(Path.Combine(output.Path, "ingot.log"));
            Assert.Contains("requires trait ITick", log);
        }
    }
}