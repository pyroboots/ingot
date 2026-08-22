using ingot.Tests.Content.Items;
using ingot.Tests.Support;

namespace ingot.Tests.Items;

public class ItemJsonContainsMaxStackSizeTest
{
    [Fact]
    public void Compile_ItemJsonContainsMaxStackSize()
    {
        string json = CompileTestHelper.CompileItemJson<EquipmentTestItem>();
        Assert.Contains("minecraft:max_stack_size", json);
        Assert.Contains("\"value\": 1", json);
    }
}