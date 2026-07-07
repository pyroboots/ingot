using ingot.Core.Behaviour.Entity;
using ingot.Tests.Content.Entities;

namespace ingot.Tests.Entities;

public class EntityJsonContainsComponentGroupTest
{
    [Fact]
    public void Compile_EntityJsonContainsComponentGroup()
    {
        string json = Entity.Compile(typeof(ComponentGroupTestEntity));

        Assert.Contains("\"component_groups\"", json);
        Assert.Contains("\"test:adult\"", json);
    }
}