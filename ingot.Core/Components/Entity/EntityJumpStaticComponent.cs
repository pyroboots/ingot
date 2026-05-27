using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.JsonHelper;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Components.Entity;

public class EntityJumpStaticComponent : IComponent<Content.Entity>
{
    public double JumpPower = 0.42;
    
    public void Compile(ref JsonTextWriter writer) 
        => Object(ref writer, Identifier.ToString(), writer => Property(ref writer, "jump_power", JumpPower));
    
    public Identifier Identifier => new("minecraft:jump.static");
    public Version MinimumFormatVersion => new(0, 0, 0);
}