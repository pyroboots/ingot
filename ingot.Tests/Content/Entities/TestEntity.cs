using ingot.Core.Behaviour;
using ingot.Core.Common;

namespace ingot.Tests.Content.Entities;

internal class TestEntity : Entity
{
    public override Identifier Identifier => new("test:test_entity");
}