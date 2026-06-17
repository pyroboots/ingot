using ingot.Core.Behaviour;
using ingot.Tests.Content.Items;

namespace ingot.Tests.Tests.Items;

public class ItemJsonContainsMaxStackSizeTest
{
    [Fact]
    public void Compile_itemJsonContainsMaxStackSize()
    {
        string json = Item.Compile(typeof(EquipmentTestItem));
        Assert.Contains("minecraft:max_stack_size", json);
        Assert.Contains("\"value\": 1", json);
    }
}