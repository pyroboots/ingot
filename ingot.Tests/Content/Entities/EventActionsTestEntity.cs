using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

namespace ingot.Tests.Content.Entities;

internal class EventActionsTestEntity : Entity
{
    public override Identifier Identifier => new("test:event_actions_entity");
    public override Dictionary<Identifier, IEntityEventAction[]> Events => new()
    {
        [new("test:drop_item")] =
        [
            new TestDropItemEntityEventAction { Slot = Enums.InventorySlot.Mainhand }
        ],
        [new("test:queue_command")] =
        [
            new TestQueueCommandEntityEventAction
            {
                Commands = ["say hello"],
                Target = Enums.Target.Other
            }
        ],
        [new("test:emit_vibration")] =
        [
            new TestEmitVibrationEntityEventAction
            {
                Type = EmitVibrationEntityEventAction.VibrationType.EntityInteract
            }
        ],
        [new("test:emit_particle")] =
        [
            new TestEmitParticleEntityEventAction { Particle = new Identifier("minecraft:heart_particle") }
        ],
        [new("test:add_remove")] =
        [
            new TestRemoveEntityEventAction { ComponentGroups = [new Identifier("test:baby")] },
            new TestAddEntityEventAction { ComponentGroups = [new Identifier("test:adult")] }
        ]
    };
}