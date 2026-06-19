using ingot.Core.Behaviour.Loot;
using ingot.Tests.Content.Loot;

namespace ingot.Tests.Loot;

public class LootTableJsonContainsPoolsTest
{
    [Fact]
    public void Compile_LootTableJsonContainsPools()
    {
        string json = LootTable.Compile(typeof(TestBlockLootTable));
        Assert.Contains("\"pools\"", json);
        Assert.Contains("test:test_item", json);
        Assert.Contains("\"function\": \"set_count\"", json);
    }
}