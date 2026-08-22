using ingot.Tests.Content.Items;
using ingot.Tests.Support;

namespace ingot.Tests.Items;

public class ItemJsonContainsDisplayNameTest
{
    [Fact]
    public void Compile_ItemJsonContainsDisplayName()
    {
        string json = CompileTestHelper.CompileItemJson<EquipmentTestItem>();
        Assert.Contains("\"value\": \"Equipment Item\"", json);
    }
}