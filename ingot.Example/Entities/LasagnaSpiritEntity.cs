using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

namespace ingot.Example.Entities;

public class LasagnaSpiritEntity : Entity
{
    public override Identifier Identifier => new("test:lasagna_spirit");
    public override bool IsSummonable => true;
}