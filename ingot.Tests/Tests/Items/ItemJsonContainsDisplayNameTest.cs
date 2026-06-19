using ingot.Core.Behaviour;
using ingot.Tests.Content.Items;

namespace ingot.Tests.Items;

public class ItemJsonContainsDisplayNameTest
{
    [Fact]
    public void Compile_ItemJsonContainsDisplayName()
    {
        string json = Item.Compile(typeof(EquipmentTestItem));
        Assert.Contains("\"value\": \"Equipment Item\"", json);
    }
}