using ingot.Core.Behaviour.Entity;
using ingot.Tests.Content.Entities;

namespace ingot.Tests.Entities;

public class EntityJsonContainsEventActionsTest
{
    [Fact]
    public void Compile_EntityJsonContainsDropItemEventAction()
    {
        string json = Entity.Compile(typeof(EventActionsTestEntity));

        Assert.Contains("\"drop_item\"", json);
        Assert.Contains("\"slot\": \"slot.weapon.mainhand\"", json);
    }

    [Fact]
    public void Compile_EntityJsonContainsQueueCommandEventAction()
    {
        string json = Entity.Compile(typeof(EventActionsTestEntity));

        Assert.Contains("\"queue_command\"", json);
        Assert.Contains("\"target\": \"other\"", json);
        Assert.Contains("\"say hello\"", json);
    }

    [Fact]
    public void Compile_EntityJsonContainsEmitVibrationEventAction()
    {
        string json = Entity.Compile(typeof(EventActionsTestEntity));

        Assert.Contains("\"emit_vibration\": \"entity_interact\"", json);
    }

    [Fact]
    public void Compile_EntityJsonContainsEmitParticleEventAction()
    {
        string json = Entity.Compile(typeof(EventActionsTestEntity));

        Assert.Contains("\"emit_particle\"", json);
        Assert.Contains("\"particle\": \"minecraft:heart_particle\"", json);
    }

    [Fact]
    public void Compile_EntityJsonContainsAddAndRemoveEventActions()
    {
        string json = Entity.Compile(typeof(EventActionsTestEntity));

        Assert.Contains("\"remove\"", json);
        Assert.Contains("\"test:baby\"", json);
        Assert.Contains("\"test:adult\"", json);
    }
}