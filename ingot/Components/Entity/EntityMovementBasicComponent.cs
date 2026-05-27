using ingot.Common;
using Newtonsoft.Json;
using static ingot.JsonHelper;
using Version = ingot.Common.Version;

namespace ingot.Components.Entity;

public class EntityMovementBasicComponent : IComponent<Content.Entity>
{
    public double MaxTurn = 30;
    
    public void Compile(ref JsonTextWriter writer) 
        => Object(ref writer, Identifier.ToString(), writer => Property(ref writer, "max_turn", MaxTurn));
    
    public Identifier Identifier => new("minecraft:movement.basic");
    public Version MinimumFormatVersion => new(0, 0, 0);
}