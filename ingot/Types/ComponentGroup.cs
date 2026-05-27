using ingot.Common;
using ingot.Components;
using Newtonsoft.Json;

namespace ingot.Types;

public class ComponentGroup : Identifiable, ICompileableFragment
{
    public ComponentGroup(string identifier) : base(identifier) {}
    public ComponentGroup(Identifier identifier) : base(identifier) {}

    public Dictionary<string, IComponent<Content.Entity>> Components { get; } = new();

    public void AddComponent(IComponent<Content.Entity> component) 
        => Components.Add(component.Identifier.ToString(), component);
    
    public void Compile(ref JsonTextWriter writer)
    {
        writer.WritePropertyName(Identifier.ToString());
        writer.WriteStartObject();
        foreach (var component in Components)
        {
            CompileTimeLogging.Push(component.Key);
            component.Value.Compile(ref writer);
            CompileTimeLogging.Pop();
        }
        writer.WriteEndObject();
    }
}