using ingot.Core.Behaviour;
using ingot.Tests.Content.Items;

namespace ingot.Tests.Items;

public class ItemJsonContainsEquipmentCategoryTest
{
    [Fact]
    public void Compile_ItemJsonContainsEquipmentCategory()
    {
        string json = Item.Compile(typeof(EquipmentTestItem));
        Assert.Contains("\"category\": \"equipment\"", json);
        Assert.Contains("itemGroup.name.sword", json);
    }
}