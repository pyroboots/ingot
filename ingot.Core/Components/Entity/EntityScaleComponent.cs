using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.JsonHelper;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Components.Entity;

public class EntityScaleComponent : IComponent<Content.Entity>
{
    public double Scale = 1;
    
    public void Compile(ref JsonTextWriter writer)
    {
        Object(ref writer, Identifier.ToString(), writer =>
        {
            if (Scale == 0)
                CompileTimeLogging.Warn(ref writer, "scale is 0, entity will be invisible at runtime");
            Property(ref writer, "value", Scale);
        });
    }
    
    public Identifier Identifier => new("minecraft:scale");
    public Version MinimumFormatVersion => new(0, 0, 0);
}