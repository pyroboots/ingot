using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.JsonHelper;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Components.Entity;

public class EntityMovementBasicComponent : IComponent<Content.Entity>
{
    public double MaxTurn = 30;
    
    public void Compile(ref JsonTextWriter writer) 
        => Object(ref writer, Identifier.ToString(), writer => Property(ref writer, "max_turn", MaxTurn));
    
    public Identifier Identifier => new("minecraft:movement.basic");
    public Version MinimumFormatVersion => new(0, 0, 0);
}