using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

namespace ingot.Tests.Content.Entities;

internal class ExperimentalTestEntity : Entity
{
    public override Identifier Identifier => new("test:experimental_entity");
    public override bool IsExperimental => true;
}