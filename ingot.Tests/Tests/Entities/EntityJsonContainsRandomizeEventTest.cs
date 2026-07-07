using ingot.Core.Behaviour.Entity;
using ingot.Tests.Content.Entities;

namespace ingot.Tests.Entities;

public class EntityJsonContainsRandomizeEventTest
{
    [Fact]
    public void Compile_EntityJsonContainsRandomizeEvent()
    {
        string json = Entity.Compile(typeof(EventRandomizeTestEntity));

        Assert.Contains("\"randomize\"", json);
        Assert.Contains("\"weight\": 95", json);
        Assert.Contains("\"weight\": 5", json);
        Assert.Contains("\"test:adult\"", json);
        Assert.Contains("\"test:baby\"", json);
    }
}