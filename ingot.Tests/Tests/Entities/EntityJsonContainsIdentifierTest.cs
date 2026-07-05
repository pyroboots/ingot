using ingot.Core.Behaviour;
using ingot.Tests.Content.Entities;

namespace ingot.Tests.Entities;

public class EntityJsonContainsIdentifierTest
{
    [Fact]
    public void Compile_EntityJsonContainsFormatVersion()
    {
        string json = Entity.Compile(typeof(TestEntity));
        Assert.Contains("\"format_version\": \"1.20.10\"", json);
        Assert.Contains("minecraft:entity", json);
        Assert.Contains("\"identifier\": \"test:test_entity\"", json);
        Assert.Contains("\"is_summonable\": true", json);
    }
}