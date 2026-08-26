using ingot.Core;
using ingot.Core.Behaviour.Block;
using ingot.Core.Common;
using ingot.Core.Scripting;
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
            string service = File.ReadAllText(servicePath);
            Assert.Contains("system.runInterval(() => {", service);
            Assert.Contains("}, 1);", service);
            Assert.Contains("service tick body marker", service);
        }
    }

    [Fact]
    public void Compile_Service_WithCustomInterval_UsesIntervalInGeneratedScript()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            Pack pack = PackTestBuilder.Create();
            pack.ScriptsEnabled = true;
            pack.AddService(FixturePaths.Resolve("scripts/tick_service.js"), intervalTicks: 20);
            pack.Compile(output.Path, verbose: false);

            string service = File.ReadAllText(Path.Combine(output.Path, "bp", "scripts", "services", "tick_service.js"));
            Assert.Contains("}, 20);", service);
            Assert.DoesNotContain("}, 1);", service);

            string mainJs = File.ReadAllText(Path.Combine(output.Path, "bp", "scripts", "main.js"));
            Assert.Contains("./services/tick_service.js", mainJs);
        }
    }

    [Fact]
    public void Compile_ScriptEvent_WritesSubscriptionAndImportsInMainJs()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            Pack pack = PackTestBuilder.Create();
            pack.ScriptsEnabled = true;
            pack.AddScriptEvent("test:hello", """
                world.sendMessage(event.message);
                """);
            pack.Compile(output.Path, verbose: false);

            string eventPath = Path.Combine(output.Path, "bp", "scripts", "events", "test_hello.js");
            Assert.True(File.Exists(eventPath));
            string script = File.ReadAllText(eventPath);
            Assert.Contains("system.afterEvents.scriptEventReceive.subscribe((event) => {", script);
            Assert.Contains("if (event.id !== \"test:hello\") return;", script);
            Assert.Contains("world.sendMessage(event.message);", script);

            string mainJs = File.ReadAllText(Path.Combine(output.Path, "bp", "scripts", "main.js"));
            Assert.Contains("./events/test_hello.js", mainJs);
        }
    }

    [Fact]
    public void Compile_ScriptEventFromFile_WritesHandlerBodyFromFile()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            Pack pack = PackTestBuilder.Create();
            pack.ScriptsEnabled = true;
            pack.AddScriptEvent(
                "test:hello_file",
                ScriptHandler.FromFile(FixturePaths.Resolve("scripts/hello_event.js")));
            pack.Compile(output.Path, verbose: false);

            string script = File.ReadAllText(Path.Combine(output.Path, "bp", "scripts", "events", "hello_event.js"));
            Assert.Contains("if (event.id !== \"test:hello_file\") return;", script);
            Assert.Contains("script event body marker", script);
            Assert.Contains("world.sendMessage(event.message);", script);
        }
    }

    [Fact]
    public void Compile_ScriptEventWithoutScriptsEnabled_WritesWarningToLog()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            Pack pack = PackTestBuilder.Create();
            pack.ScriptsEnabled = false;
            pack.AddScriptEvent("test:ignored", "world.sendMessage(\"nope\");");
            pack.Compile(output.Path, verbose: true);

            string log = File.ReadAllText(Path.Combine(output.Path, "ingot.log"));
            Assert.Contains("script events are registered but ScriptsEnabled is false", log);
            Assert.False(Directory.Exists(Path.Combine(output.Path, "bp", "scripts", "events")));
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
    public void Compile_BlockEventsFromFile_HoistsLeadingImports()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        string helperPath = Path.Combine(output.Path, "helper.js");
        File.WriteAllText(helperPath, "export class Helper {}\n");
        string handlerPath = Path.Combine(output.Path, "interact.js");
        File.WriteAllText(handlerPath, """
            import { Helper } from "../helper.js";

            const helper = new Helper();
            event.player.sendMessage("ok");
            """);

        Pack pack = PackTestBuilder.Create();
        pack.BehaviourPack.AddBlockFromInstance(new ImportHoistTestBlock(handlerPath));
        pack.ScriptsEnabled = true;
        pack.AddScriptFile(helperPath);
        pack.Compile(output.Path, verbose: false);

        string script = File.ReadAllText(Path.Combine(output.Path, "bp", "scripts", "blocks", "test_import_hoist_block_events.js"));
        int importIndex = script.IndexOf("import { Helper } from \"../helper.js\";", StringComparison.Ordinal);
        int componentIndex = script.IndexOf("const TestImportHoistBlockBlockEventsComponent", StringComparison.Ordinal);
        Assert.True(importIndex >= 0, "expected hoisted helper import");
        Assert.True(componentIndex >= 0, "expected generated component");
        Assert.True(importIndex < componentIndex, "helper import must sit at file scope");
        Assert.Contains("const helper = new Helper();", script);
        Assert.True(File.Exists(Path.Combine(output.Path, "bp", "scripts", "helper.js")));
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

internal sealed class ImportHoistTestBlock(string handlerPath) : Block
{
    public override Identifier Identifier => new("test:import_hoist_block");

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance(
            "import_hoist_block",
            MaterialInstance.RenderMethods.Opaque,
            FixturePaths.Resolve("test_block.png"))
    };

    public override BlockEvents? BlockEvents => new()
    {
        PlayerInteractEvent = ScriptHandler.FromFile(handlerPath)
    };
}