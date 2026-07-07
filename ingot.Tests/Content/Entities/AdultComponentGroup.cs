using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

namespace ingot.Tests.Content.Entities;

internal class AdultComponentGroup : EntityComponentGroup
{
    public override Identifier Identifier => new("test:adult");
}