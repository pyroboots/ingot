using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Entity;

namespace ingot.Example.Entities;

public class LasagnaSpiritEntity : Entity, IEntityBehaviourPresetFlying
{
    public override Identifier Identifier => new("test", "lasagna_spirit");
    public dynamic Family => "lasagna";
    int IHealth.Max => 20;
    
    
    dynamic IDespawn.DespawnFromDistance => null;
    EntityFilter IDespawn.Filters => null;
    
    float IMovement.Max => 6;
    float IMovement.Value => 3;

    string[] INavigationFly.BlocksToAvoid => [];
    float IBehaviorFloatWander.FloatDuration => 6f;

    public override EntityComponentGroup[] ComponentGroups => [new LasagnaSpiritEntityAngry()];

    public override Dictionary<Identifier, IEntityEventAction[]> Events => new()
    {
        [new Identifier("minecraft:entity_spawned")] = new []
        {
            new RandomizeEntityEventAction()
            {
                EventActions = new []
                {
                    new RandomizeEntityEventAction.EventActionPool(0.5f, new []
                    {
                        new ComponentGroupAddEntityEventAction()
                        {
                            ComponentGroups = new []
                            {
                                new Identifier("test", "lasagna_spirit_angry"),
                            }
                        }
                    })
                }
            }
        }
    };
}

public class LasagnaSpiritEntityAngry : EntityComponentGroup, IEntityBehaviourPresetFlyingHostile
{
    public override Identifier Identifier => new("test", "lasagna_spirit_angry");
    public override Entity Parent => new LasagnaSpiritEntity();
    public dynamic Family { get; }
    public int Max { get; }
    public int Value { get; }

    float IMovement.Value => Value;

    public dynamic DespawnFromDistance { get; }
    public EntityFilter Filters { get; }
    float IMovement.Max => Max;

    public string[] BlocksToAvoid { get; }
    public float FloatDuration { get; }
    public FloatRange Damage { get; }
    public string EffectName { get; }
    public int AttackInterval { get; }
    public string AttackTypes { get; }
}