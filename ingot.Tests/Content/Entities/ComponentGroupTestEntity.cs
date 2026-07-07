using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

namespace ingot.Tests.Content.Entities;

internal class ComponentGroupTestEntity : Entity
{
    public override Identifier Identifier => new("test:component_group_entity");
    public override EntityComponentGroup[] ComponentGroups => [new AdultComponentGroup()];
}