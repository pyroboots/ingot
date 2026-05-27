using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.JsonHelper;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Components.Entity;

public class EntityHealthComponent : IComponent<Content.Entity>
{
    public int Max = 20;
    public int Value = 20;
    
    public void Compile(ref JsonTextWriter writer)
    {
        Object(ref writer, Identifier.ToString(), writer =>
        {
            if (Max == 0)
                CompileTimeLogging.Warn(ref writer, "max health is 0");
            if (int.IsNegative(Max))
                CompileTimeLogging.Warn(ref writer, "max health is negative");
            Property(ref writer, "max", Max);
            
            if (Value == 0)
                CompileTimeLogging.Warn(ref writer, "initial health is 0");
            if (int.IsNegative(Value))
                CompileTimeLogging.Warn(ref writer, "initial health is negative");
            Property(ref writer, "value", Value);
        });
    }
    
    public Identifier Identifier => new("minecraft:health");
    public Version MinimumFormatVersion => new(0, 0, 0);
}