using ingot.Core;
using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Block;
using ingot.Tests.Content.Blocks;
using ingot.Tests.Support;

using Version = ingot.Core.Common.Version;

namespace ingot.Tests.Blocks;

public class BlockEventsCompilesScriptAndComponentTest
{
    [Fact]
    public void Compile_BlockEvents_WritesScriptAndAddsComponent()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            Pack pack = PackTestBuilder.Create().AddBlock<BlockEventsTestBlock>();
            pack.ScriptsEnabled = true;
            pack.Compile(output.Path, verbose: false);

            string blockJson = File.ReadAllText(Path.Combine(output.Path, "bp", "blocks", "events_block.json"));
            Assert.Contains("test:test_events_block_block_events_component", blockJson);
            // Custom Components V2 requires format_version 1.21.90+ for direct component entries.
            Assert.Contains("\"format_version\": \"1.21.90\"", blockJson);

            string scriptPath = Path.Combine(output.Path, "bp", "scripts", "blocks", "test_events_block_events.js");
            Assert.True(File.Exists(scriptPath));
            string script = File.ReadAllText(scriptPath);
            Assert.Contains("registerCustomComponent", script);
            Assert.Contains("onPlace", script);
            Assert.Contains("event.block.setType('minecraft:stone');", script);

            string mainJs = File.ReadAllText(Path.Combine(output.Path, "bp", "scripts", "main.js"));
            Assert.Contains("./blocks/test_events_block_events.js", mainJs);
        }
    }

    [Fact]
    public void Compile_ScriptApiModulesWithoutServer_StillWritesServerDependency()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            Pack pack = PackTestBuilder.Create().AddBlock<BlockEventsTestBlock>();
            pack.ScriptsEnabled = true;
            // Author mistakenly replaced the dictionary, dropping @minecraft/server.
            pack.ScriptApiModules = new() { ["@minecraft/server-ui"] = new Version(2, 0, 0) };
            pack.Compile(output.Path, verbose: false);

            string manifest = File.ReadAllText(Path.Combine(output.Path, "bp", "manifest.json"));
            Assert.Contains("@minecraft/server", manifest);
            Assert.Contains("@minecraft/server-ui", manifest);

            string script = File.ReadAllText(Path.Combine(output.Path, "bp", "scripts", "blocks", "test_events_block_events.js"));
            Assert.Contains("import * as serverUi from \"@minecraft/server-ui\";", script);
        }
    }

    [Fact]
    public void Default_BlockFormatVersion_IsCustomComponentsV2()
    {
        string json = Block.Compile(typeof(BlockEventsTestBlock));
        Assert.Contains("\"format_version\": \"1.21.90\"", json);
    }
}