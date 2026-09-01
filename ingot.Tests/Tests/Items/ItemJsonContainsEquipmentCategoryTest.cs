using ingot.Tests.Content.Items;
using ingot.Tests.Support;

namespace ingot.Tests.Items;

public class ItemJsonContainsEquipmentCategoryTest
{
    [Fact]
    public void Compile_ItemJsonContainsEquipmentCategory()
    {
        string json = CompileTestHelper.CompileItemJson<EquipmentTestItem>();
        Assert.Contains("\"category\": \"equipment\"", json);
        Assert.Contains("itemGroup.name.sword", json);
    }
}