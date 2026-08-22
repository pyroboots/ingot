using ingot.Tests.Content;
using ingot.Tests.Support;

namespace ingot.Tests.Items;

public class ItemJsonContainsIdentifierTest
{
    [Fact]
    public void Compile_ItemJsonContainsIdentifier()
    {
        string json = CompileTestHelper.CompileItemJson<TestItem>();
        Assert.Contains("\"identifier\": \"test:test_item\"", json);
    }
}