using ingot.Core.Common;
using Newtonsoft.Json;
using Version = ingot.Core.Common.Version;
using static ingot.Core.JsonHelper;

namespace ingot.Core.Components.Item;

public class ItemDamageComponent : IComponent<Content.Item>
{
    public required int Damage;    
    
    public void Compile(ref JsonTextWriter writer)
    {
        Object(ref writer, Identifier.ToString(), w =>
        {
            Property(ref w, "value", Damage);
        });
    }

    public Identifier Identifier => new("minecraft:damage");
    public Version MinimumFormatVersion => new(0, 0, 0);
}