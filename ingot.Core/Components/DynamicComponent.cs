using ingot.Core.Common;
using Newtonsoft.Json;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Components;

// inherit all 3 to allow use on all
public class DynamicComponent : IComponent<Content.Entity>, IComponent<Content.Block>, IComponent<Content.Item>
{
    public Identifier Identifier { get; }
    // we dont know the min fmt ver for dynamic components, so we'll assume
    // the user knows what theyre doing and allow any version
    public Version MinimumFormatVersion => new(0, 0, 0);
    public Dictionary<string, dynamic> Properties { get; }
    
    public DynamicComponent(Identifier identifier, Dictionary<string, dynamic> properties)
    {
        Identifier = identifier;
        Properties = properties;
    }
    
    public void Compile(ref JsonTextWriter writer)
    {
        writer.WritePropertyName(Identifier.ToString());
        writer.WriteStartObject();
            foreach (var prop in Properties)
            {
                writer.WritePropertyName(prop.Key);
                if (prop.Value is ICompileableFragment)
                    ((ICompileableFragment)prop.Value).Compile(ref writer);
                else
                    JsonSerializer.CreateDefault().Serialize(writer, prop.Value);
            }
        writer.WriteEndObject();
    }
}