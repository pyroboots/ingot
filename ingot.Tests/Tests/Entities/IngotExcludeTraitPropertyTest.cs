using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;
using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits.Entity;

namespace ingot.Tests.Entities;

public class IngotExcludeTraitPropertyTest
{
    [Fact]
    public void Compile_IngotExcludeOnInterfaceOmitsProperty()
    {
        string json = Entity.Compile(typeof(LookAtPlayerTestEntity));

        Assert.Contains("minecraft:behavior.look_at_player", json);
        Assert.Contains("\"look_distance\": 6.0", json);
        Assert.DoesNotContain("max_look_time", json);
        Assert.DoesNotContain("min_look_time", json);
        Assert.DoesNotContain("target_distance", json);
    }

    [Fact]
    public void Compile_IngotExcludeOnImplementationOmitsProperty()
    {
        string json = Entity.Compile(typeof(ImplExcludeTestEntity));

        Assert.Contains("minecraft:health", json);
        Assert.Contains("\"max\": 10", json);
        // Value is excluded on the implementation; only Max should remain (or both if Value not excluded)
        Assert.DoesNotContain("\"value\": 10", json);
    }

    private sealed class LookAtPlayerTestEntity : Entity, IBehaviorLookAtPlayer, IHealth
    {
        public override Identifier Identifier => new("test", "look_at_player_entity");
        int IHealth.Max => 10;
        int IBehaviorLookAtPlayer.Priority => 7;
        float IBehaviorLookAtPlayer.LookDistance => 6f;
    }

    private sealed class ImplExcludeTestEntity : Entity, IHealth
    {
        public override Identifier Identifier => new("test", "impl_exclude_entity");

        int IHealth.Max => 10;

        [IngotExclude]
        int IHealth.Value => 10;
    }
}
