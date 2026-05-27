using ingot.Common;
using Newtonsoft.Json;
using Version = ingot.Common.Version;
using static ingot.JsonHelper;

namespace ingot.Components.Item;

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