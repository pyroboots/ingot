using ingot.Core.Behaviour;
using ingot.Tests.Content.Items;

namespace ingot.Tests.Tests.Items;

public class ItemJsonContainsFoodTraitTest
{
    [Fact]
    public void Compile_itemJsonContainsFoodTrait()
    {
        string json = Item.Compile(typeof(FoodTestItem));
        Assert.Contains("minecraft:food", json);
        Assert.Contains("\"nutrition\": 4", json);
        Assert.Contains("\"using_converts_to\": \"minecraft:bowl\"", json);
    }
}