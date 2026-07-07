using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

namespace ingot.Example.Entities;

public class LasagnaSpiritCalmGroup : EntityComponentGroup
{
    public override Identifier Identifier => new("test:lasagna_spirit_calm");
}

public class LasagnaSpiritEnragedGroup : EntityComponentGroup
{
    public override Identifier Identifier => new("test:lasagna_spirit_enraged");
}

public class LasagnaSpiritEntity : Entity
{
    public override Identifier Identifier => new("test:lasagna_spirit");
    public override bool IsSummonable => true;

    public override EntityComponentGroup[] ComponentGroups =>
    [
        new LasagnaSpiritCalmGroup(),
        new LasagnaSpiritEnragedGroup()
    ];

    public override Dictionary<Identifier, IEntityEventAction[]> Events => new()
    {
        [new("minecraft:entity_spawned")] =
        [
            new RandomizeEntityEventAction
            {
                EventActions =
                [
                    new(90, [
                        new ComponentGroupAddEntityEventAction
                        {
                            ComponentGroups = [new Identifier("test:lasagna_spirit_calm")]
                        }
                    ]),
                    new(10, [
                        new ComponentGroupAddEntityEventAction
                        {
                            ComponentGroups = [new Identifier("test:lasagna_spirit_enraged")]
                        }
                    ])
                ]
            }
        ],
        [new("test:enrage")] =
        [
            new ComponentGroupRemoveEntityEventAction
            {
                ComponentGroups = [new Identifier("test:lasagna_spirit_calm")]
            },
            new ComponentGroupAddEntityEventAction
            {
                ComponentGroups = [new Identifier("test:lasagna_spirit_enraged")]
            },
            new EmitVibrationEntityEventAction
            {
                Type = EmitVibrationEntityEventAction.VibrationType.EntityAct
            }
        ],
        [new("test:calm_down")] =
        [
            new SequenceEntityEventAction
            {
                EventActions =
                [
                    new ComponentGroupRemoveEntityEventAction
                    {
                        ComponentGroups = [new Identifier("test:lasagna_spirit_enraged")]
                    },
                    new ComponentGroupAddEntityEventAction
                    {
                        ComponentGroups = [new Identifier("test:lasagna_spirit_calm")]
                    },
                    new EmitParticleEntityEventAction
                    {
                        Particle = new Identifier("minecraft:heart_particle")
                    }
                ]
            }
        ]
    };
}