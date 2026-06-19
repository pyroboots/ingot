using ingot.Core.Behaviour;
using ingot.Tests.Content;

namespace ingot.Tests.Items;

public class ItemJsonContainsIconTest
{
    [Fact]
    public void Compile_ItemJsonContainsIcon()
    {
        string json = Item.Compile(typeof(TestItem));
        Assert.Contains("minecraft:icon", json);
        Assert.Contains("\"texture\": \"test_item\"", json);
    }
}