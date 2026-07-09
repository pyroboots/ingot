using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Item;
using ingot.Tests.Content;

namespace ingot.Tests.Items;

public class ItemJsonContainsIconTest
{
    [Fact]
    public void Compile_ItemJsonContainsIcon()
    {
        string json = Item.Compile(typeof(TestItem));
        Assert.Contains("minecraft:icon", json);
        // format_version 1.21+ uses textures.default rather than the legacy texture field
        Assert.Contains("\"default\": \"test_item\"", json);
    }
}