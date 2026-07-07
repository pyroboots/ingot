using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

namespace ingot.Tests.Content.Entities;

internal class EventSequenceTestEntity : Entity
{
    public override Identifier Identifier => new("test:sequence_entity");
    public override Dictionary<Identifier, IEntityEventAction[]> Events => new()
    {
        [new("test:sequence_event")] =
        [
            new TestSequenceEntityEventAction
            {
                EventActions =
                [
                    new TestAddEntityEventAction { ComponentGroups = [new Identifier("test:adult")] },
                    new TestDropItemEntityEventAction { Slot = Enums.InventorySlot.Head }
                ]
            }
        ]
    };
}