using ingot.Common;
using ingot.Components;
using ingot.Types;
using Newtonsoft.Json;
using static ingot.JsonHelper;
using Version = ingot.Common.Version;

namespace ingot.Content;

public class Entity : Identifiable, ICompileable
{
    public Dictionary<Identifier, IComponent<Entity>> Components { get; } = new();
    public Dictionary<Identifier, ComponentGroup> ComponentGroups { get; } = new();

    public string SpawnCategory = "creature";
    public bool IsSpawnable = true;
    public bool IsSummonable = true;
    public Version FormatVersion = new("1.26.10");
    public Entity(string identifier) : base(identifier) {}
    public Entity(Identifier identifier) : base(identifier) {}

    public void AddComponent(IComponent<Entity> component) => Components.Add(component.Identifier, component);
    public ComponentGroup AddComponentGroup(Identifier identifier)
    {
        ComponentGroup group = new(identifier);
        ComponentGroups.Add(identifier, group);
        return group;
    }
    
    public string Compile()
    {
        CompileTimeLogging.Push(Identifier.ToString());
        
        StringWriter sw = new();
        JsonTextWriter w = new(sw);
        w.Formatting = Formatting.Indented;
        w.Indentation = 4;
        
        w.WriteStartObject();
        
        Property(ref w, "format_version", FormatVersion.ToString());
        Object(ref w, "minecraft:entity", w =>
        {
            Object(ref w, "description", w =>
            {
                Property(ref w, "identifier", Identifier);
                Property(ref w, "spawn_category", SpawnCategory);
                Property(ref w, "is_spawnable", IsSpawnable);
                Property(ref w, "is_summonable", IsSummonable);
            });
            
            CompileTimeLogging.Push("component_groups");
            Object(ref w, "component_groups", w =>
            {
                foreach (var kvp in ComponentGroups)
                {
                    CompileTimeLogging.Push(kvp.Key.ToString());
                    kvp.Value.Compile(ref w);
                    CompileTimeLogging.Pop();
                }
            });
            CompileTimeLogging.Pop();
            
            CompileTimeLogging.Push("components");
            Object(ref w, "components", w =>
            {
                foreach (var kvp in Components)
                {
                    CompileTimeLogging.Push(kvp.Key.ToString());
                    kvp.Value.Compile(ref w);
                    CompileTimeLogging.Pop();
                }
            });
            CompileTimeLogging.Pop();
        });
        
        CompileTimeLogging.Pop();
        
        return sw.ToString();
    }
}