using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Entity;

namespace ingot.Tests.Content.Entities;

internal class TraitTestEntity : Entity, IHealth, ITypeFamily
{
    public override Identifier Identifier => new("test:trait_entity");
    int IHealth.Max => 20;
    dynamic ITypeFamily.Family => "test";
}