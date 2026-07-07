using ingot.Core.Behaviour.Entity;
using ingot.Tests.Content.Entities;

namespace ingot.Tests.Entities;

public class EntityJsonContainsExperimentalTest
{
    [Fact]
    public void Compile_EntityJsonContainsExperimentalWhenEnabled()
    {
        string json = Entity.Compile(typeof(ExperimentalTestEntity));
        Assert.Contains("\"is_experimental\": true", json);
    }

    [Fact]
    public void Compile_EntityJsonContainsExperimentalWhenDisabled()
    {
        string json = Entity.Compile(typeof(TestEntity));
        Assert.Contains("\"is_experimental\": false", json);
    }
}