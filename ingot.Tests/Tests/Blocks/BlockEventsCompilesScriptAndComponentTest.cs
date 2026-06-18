using ingot.Core;
using ingot.Core.Behaviour;
using ingot.Tests.Content.Blocks;
using ingot.Tests.Support;

namespace ingot.Tests.Tests.Blocks;

public class BlockEventsCompilesScriptAndComponentTest
{
    [Fact]
    public void Compile_blockEvents_writesScriptAndAddsComponent()
    {
        string outputDir = CompileTestHelper.CreateOutputDirectory();
        try
        {
            Pack pack = PackTestBuilder.Create().AddBlock<BlockEventsTestBlock>();
            pack.ScriptsEnabled = true;
            pack.Compile(outputDir, verbose: false);

            string blockJson = File.ReadAllText(Path.Combine(outputDir, "bp", "blocks", "events_block.json"));
            Assert.Contains("test:test_events_block_block_events_component", blockJson);

            string scriptPath = Path.Combine(outputDir, "bp", "scripts", "blocks", "test_events_block_events.js");
            Assert.True(File.Exists(scriptPath));
            string script = File.ReadAllText(scriptPath);
            Assert.Contains("registerCustomComponent", script);
            Assert.Contains("onPlace", script);
            Assert.Contains("event.block.setType('minecraft:stone');", script);

            string mainJs = File.ReadAllText(Path.Combine(outputDir, "bp", "scripts", "main.js"));
            Assert.Contains("./blocks/test_events_block_events.js", mainJs);
        }
        finally
        {
            CompileTestHelper.DeleteOutputDirectory(outputDir);
        }
    }
}