using ingot.Core.Common;

namespace ingot.Tests.Common;

public class IdentifierNormalizesInputTest
{
    [Fact]
    public void Identifier_NormalizesNamespaceAndName()
    {
        Identifier identifier = new("TEST:My_Block");

        Assert.Equal("test", identifier.Namespace);
        Assert.Equal("my_block", identifier.Name);
        Assert.Equal("test:my_block", identifier.ToString());
    }

    [Fact]
    public void Identifier_EmptyStringFallsBackToUnknown()
    {
        Identifier identifier = new("");

        Assert.Equal("minecraft", identifier.Namespace);
        Assert.Equal("unknown", identifier.Name);
    }
}