using System.ComponentModel;

using ingot.Core.Behaviour.Entity;
using ingot.Tests.Content.Entities;

namespace ingot.Tests.Entities;

public class EntityJsonContainsPropertiesTest
{
    [Fact]
    public void Compile_EntityJsonOmitsPropertiesWhenNoneDefined()
    {
        string json = Entity.Compile(typeof(TestEntity));

        // bedrock rejects description.properties: {} ("actor has no properties listed").
        Assert.DoesNotContain("\"properties\"", json);
    }

    [Fact]
    public void Compile_EntityJsonContainsBooleanProperty()
    {
        string json = Entity.Compile(typeof(PropertiesTestEntity));

        Assert.Contains("\"test:is_charged\"", json);
        Assert.Contains("\"type\": \"bool\"", json);
        Assert.Contains("\"default\": false", json);
        Assert.Contains("\"client_sync\": true", json);
    }

    [Fact]
    public void Compile_EntityJsonContainsEnumProperty()
    {
        string json = Entity.Compile(typeof(PropertiesTestEntity));

        Assert.Contains("\"test:mood\"", json);
        Assert.Contains("\"type\": \"enum\"", json);
        Assert.Contains("\"values\"", json);
        Assert.Contains("\"calm\"", json);
        Assert.Contains("\"alert\"", json);
        Assert.Contains("\"angry\"", json);
        Assert.Contains("\"default\": \"calm\"", json);
    }

    [Fact]
    public void Compile_EntityJsonContainsFloatPropertyWithRange()
    {
        string json = Entity.Compile(typeof(PropertiesTestEntity));

        Assert.Contains("\"test:power\"", json);
        Assert.Contains("\"type\": \"float\"", json);
        Assert.Contains("\"range\"", json);
        Assert.Contains("\"default\": 0.5", json);
    }

    [Fact]
    public void Compile_EntityJsonContainsIntPropertyWithClientSyncFalse()
    {
        string json = Entity.Compile(typeof(PropertiesTestEntity));

        Assert.Contains("\"test:level\"", json);
        Assert.Contains("\"type\": \"int\"", json);
        Assert.Contains("\"default\": 1", json);
        Assert.Contains("\"client_sync\": false", json);
    }

    [Fact]
    public void Compile_EnumPropertyThrowsWhenDefaultNotInValues()
    {
        Assert.Throws<InvalidEnumArgumentException>(
            () => Entity.Compile(typeof(InvalidEnumDefaultPropertyEntity)));
    }

    [Fact]
    public void Compile_FloatPropertyThrowsWhenDefaultOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Entity.Compile(typeof(OutOfRangeFloatPropertyEntity)));
    }

    [Fact]
    public void Compile_IntPropertyThrowsWhenDefaultOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Entity.Compile(typeof(OutOfRangeIntPropertyEntity)));
    }
}
