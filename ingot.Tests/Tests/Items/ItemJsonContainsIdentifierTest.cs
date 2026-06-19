using ingot.Core.Behaviour;
using ingot.Tests.Content;

namespace ingot.Tests.Items;

public class ItemJsonContainsIdentifierTest
{
    [Fact]
    public void Compile_ItemJsonContainsIdentifier()
    {
        string json = Item.Compile(typeof(TestItem));
        Assert.Contains("\"identifier\": \"test:test_item\"", json);
    }
}