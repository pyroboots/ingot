using ingot.Core.Behaviour;
using ingot.Tests.Content;

namespace ingot.Tests.Tests.Items;

public class ItemJsonContainsIconTest
{
    [Fact]
    public void Compile_itemJsonContainsIcon()
    {
        string json = Item.Compile(typeof(TestItem));
        Assert.Contains("minecraft:icon", json);
        Assert.Contains("\"texture\": \"test_item\"", json);
    }
}