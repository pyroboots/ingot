using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Entity;

namespace ingot.Tests.Content.Entities;

internal class PresetTestEntity : Entity, IBasicEntity
{
    public override Identifier Identifier => new("test:preset_entity");
    dynamic ITypeFamily.Family => "mob";
    int IHealth.Max => 10;
    dynamic IDespawn.DespawnFromDistance => null!;
    EntityFilter IDespawn.Filters => null!;
}