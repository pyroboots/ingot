using ingot.Core;
using ingot.Core.Behaviour;
using ingot.Tests.Content.Items;
using ingot.Tests.Support;

namespace ingot.Tests.Items;

public class ItemEventsCompilesScriptAndComponentTest
{
    [Fact]
    public void Compile_ItemEvents_WritesScriptAndAddsComponent()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();
        {
            Pack pack = PackTestBuilder.Create().AddItem<ItemEventsTestItem>();
            pack.ScriptsEnabled = true;
            pack.Compile(output.Path, verbose: false);

            string itemJson = File.ReadAllText(Path.Combine(output.Path, "bp", "items", "events_item.json"));
            Assert.Contains("test:test_events_item_item_events_component", itemJson);
            Assert.Contains("\"components\"", itemJson);
            Assert.DoesNotContain("\"test:test_events_item_item_events_component\": {}", itemJson.Split("\"components\"")[0]);

            string scriptPath = Path.Combine(output.Path, "bp", "scripts", "items", "test_events_item_events.js");
            Assert.True(File.Exists(scriptPath));
            string script = File.ReadAllText(scriptPath);
            Assert.Contains("registerCustomComponent", script);
            Assert.Contains("itemComponentRegistry", script);
            Assert.Contains("onUse", script);
            Assert.Contains("event.source.sendMessage('used item');", script);

            string mainJs = File.ReadAllText(Path.Combine(output.Path, "bp", "scripts", "main.js"));
            Assert.Contains("./items/test_events_item_events.js", mainJs);
        }
    }
}