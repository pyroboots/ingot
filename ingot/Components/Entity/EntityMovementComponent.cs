using ingot.Common;
using Newtonsoft.Json;
using static ingot.JsonHelper;
using Version = ingot.Common.Version;

namespace ingot.Components.Entity;

public class EntityMovementComponent : IComponent<Content.Entity>
{
    public required double Max;
    public required double Value;
    
    public void Compile(ref JsonTextWriter writer)
    {
        Object(ref writer, Identifier.ToString(), writer =>
        {
            Property(ref writer, "max", Max);
            Property(ref writer, "value", Value);
        });
    }
    
    public Identifier Identifier => new("minecraft:movement");
    public Version MinimumFormatVersion => new(0, 0, 0);
}