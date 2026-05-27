using ingot.Core.Common;
using static ingot.Core.JsonHelper;
using Newtonsoft.Json;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Components.Entity;

public class EntityIsBabyComponent : IComponent<Content.Entity>
{
    public void Compile(ref JsonTextWriter writer) 
        => Object(ref writer, Identifier.ToString(), writer => { });
    
    public Identifier Identifier => new("minecraft:is_baby");
    public Version MinimumFormatVersion => new(0, 0, 0);
}