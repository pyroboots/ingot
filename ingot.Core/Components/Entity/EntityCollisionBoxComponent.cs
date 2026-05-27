using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.JsonHelper;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Components.Entity;

public class EntityCollisionBoxComponent : IComponent<Content.Entity>
{
    public required double Width;
    public required double Height;
    
    public void Compile(ref JsonTextWriter writer)
    {
        Object(ref writer, Identifier.ToString(), writer =>
        {
            if (double.IsNegative(Width))
                CompileTimeLogging.Warn(ref writer, "width is negative, will be assumed 0 at runtime");
            Property(ref writer, "width", Width);
            if (double.IsNegative(Height))
                CompileTimeLogging.Warn(ref writer, "height is negative, will be assumed 0 at runtime");
            Property(ref writer, "height", Height);
        });
    }
    
    public Identifier Identifier => new("minecraft:collision_box");
    public Version MinimumFormatVersion => new(0, 0, 0);
}