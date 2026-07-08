using ingot.Core.Behaviour;
using ingot.Tests.Content.Items;

namespace ingot.Tests.Items;

public class ItemJsonContainsFoodTraitTest
{
    [Fact]
    public void Compile_ItemJsonContainsFoodTrait()
    {
        string json = Item.Compile(typeof(FoodTestItem));
        Assert.Contains("minecraft:food", json);
        Assert.Contains("\"nutrition\": 4", json);
        Assert.Contains("\"using_converts_to\": \"minecraft:bowl\"", json);
    }

    [Fact]
    public void Compile_FoodItemJsonContainsUseDuration()
    {
        string json = Item.Compile(typeof(FoodTestItem));
        Assert.Contains("minecraft:use_modifiers", json);
        Assert.Contains("\"use_duration\": 1.6", json);
        Assert.Contains("minecraft:use_animation", json);
        Assert.Contains("\"value\": \"eat\"", json);
    }
}