using ingot.Core.Behaviour;
using ingot.Tests.Content.Entities;

namespace ingot.Tests.Tests.Entities;

public class EntityJsonContainsIdentifierTest
{
    [Fact]
    public void Compile_entityJsonContainsFormatVersion()
    {
        string json = Entity.Compile(typeof(TestEntity));
        Assert.Contains("\"format_version\": \"1.20.10\"", json);
        Assert.Contains("minecraft:entity", json);
    }
}