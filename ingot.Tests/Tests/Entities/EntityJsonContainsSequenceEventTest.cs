using ingot.Core.Behaviour.Entity;
using ingot.Tests.Content.Entities;

namespace ingot.Tests.Entities;

public class EntityJsonContainsSequenceEventTest
{
    [Fact]
    public void Compile_EntityJsonContainsSequenceEvent()
    {
        string json = Entity.Compile(typeof(EventSequenceTestEntity));

        Assert.Contains("\"sequence\"", json);
        Assert.Contains("\"add\"", json);
        Assert.Contains("\"test:adult\"", json);
        Assert.Contains("\"drop_item\"", json);
        Assert.Contains("\"slot\": \"slot.armor.head\"", json);
    }
}