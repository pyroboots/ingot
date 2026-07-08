using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Item;
using ingot.Tests.Content.Items;

namespace ingot.Tests.Items;

public class ItemJsonContainsMaxStackSizeTest
{
    [Fact]
    public void Compile_ItemJsonContainsMaxStackSize()
    {
        string json = Item.Compile(typeof(EquipmentTestItem));
        Assert.Contains("minecraft:max_stack_size", json);
        Assert.Contains("\"value\": 1", json);
    }
}