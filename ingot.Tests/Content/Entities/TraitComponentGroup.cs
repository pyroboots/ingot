using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Entity;

namespace ingot.Tests.Content.Entities;

internal class TraitComponentGroup : EntityComponentGroup, IHealth
{
    public override Identifier Identifier => new("test:trait_group");
    public override Entity Parent => new TraitComponentGroupTestEntity();
    int IHealth.Max => 10;
}

internal class TraitComponentGroupTestEntity : Entity
{
    public override Identifier Identifier => new("test:trait_group_entity");
    public override EntityComponentGroup[] ComponentGroups => [new TraitComponentGroup()];
}