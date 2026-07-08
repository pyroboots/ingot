using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;
using ingot.Tests.Content.Entities;

namespace ingot.Tests.Entities;

public class EntityEventsDslTest
{
    [Fact]
    public void Map_SpawnedAdultOrBaby_EmitsSequenceAndRandomize()
    {
        Identifier baby = new("test", "custom_cow_baby");
        var events = EntityEvents.Map(
            (Identifier.Vanilla("entity_spawned"),
                EntityEvents.SpawnedAdultOrBaby(95f, 5f, "test:spawn_adult", baby)));

        // Compile via a tiny entity that only carries these events
        string json = Entity.Compile(typeof(DslEventsEntity));

        Assert.Contains("minecraft:entity_spawned", json);
        Assert.Contains("\"sequence\"", json);
        Assert.Contains("\"randomize\"", json);
        Assert.Contains("\"weight\": 95.0", json);
        Assert.Contains("\"trigger\": \"test:spawn_adult\"", json);
        Assert.Contains("test:custom_cow_baby", json);
    }

    [Fact]
    public void GrowUp_EmitsRemoveAndAdd()
    {
        string json = Entity.Compile(typeof(DslGrowUpEntity));
        Assert.Contains("minecraft:ageable_grow_up", json);
        Assert.Contains("\"remove\"", json);
        Assert.Contains("\"add\"", json);
        Assert.Contains("test:baby_group", json);
        Assert.Contains("test:adult_group", json);
    }

    private sealed class DslEventsEntity : Entity
    {
        public override Identifier Identifier => new("test", "dsl_events");
        public override Dictionary<Identifier, IEntityEventAction[]> Events => EntityEvents.Map(
            (Identifier.Vanilla("entity_spawned"),
                EntityEvents.SpawnedAdultOrBaby(95f, 5f, "test:spawn_adult", new Identifier("test", "custom_cow_baby"))));
    }

    private sealed class DslGrowUpEntity : Entity
    {
        public override Identifier Identifier => new("test", "dsl_grow_up");
        public override Dictionary<Identifier, IEntityEventAction[]> Events => EntityEvents.Map(
            (Identifier.Vanilla("ageable_grow_up"),
                EntityEvents.GrowUp(new Identifier("test", "baby_group"), new Identifier("test", "adult_group"))));
    }
}
