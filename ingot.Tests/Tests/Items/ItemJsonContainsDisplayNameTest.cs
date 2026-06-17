using ingot.Core.Behaviour;
using ingot.Tests.Content.Items;

namespace ingot.Tests.Tests.Items;

public class ItemJsonContainsDisplayNameTest
{
    [Fact]
    public void Compile_itemJsonContainsDisplayName()
    {
        string json = Item.Compile(typeof(EquipmentTestItem));
        Assert.Contains("\"value\": \"Equipment Item\"", json);
    }
}