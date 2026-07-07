using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

namespace ingot.Tests.Content.Entities;

internal class EventRandomizeTestEntity : Entity
{
    public override Identifier Identifier => new("test:randomize_entity");
    public override Dictionary<Identifier, IEntityEventAction[]> Events => new()
    {
        [new("test:randomize_event")] =
        [
            new TestRandomizeEntityEventAction
            {
                EventActions =
                [
                    new RandomizeEntityEventAction.EventActionPool(95, [
                        new TestAddEntityEventAction { ComponentGroups = [new Identifier("test:adult")] }
                    ]),
                    new RandomizeEntityEventAction.EventActionPool(5, [
                        new TestAddEntityEventAction { ComponentGroups = [new Identifier("test:baby")] }
                    ])
                ]
            }
        ]
    };
}