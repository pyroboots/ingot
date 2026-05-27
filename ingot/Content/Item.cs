using ingot.Common;
using ingot.Components;
using Newtonsoft.Json;
using static ingot.JsonHelper;
using Version = ingot.Common.Version;

namespace ingot.Content;

public class Item : Identifiable, ICompileable
{
    public Version FormatVersion = new("1.20.10");
    public Dictionary<Identifier, IComponent<Item>> Components { get; } = new();

    // header props
    public enum CatalogueCategory 
    { Construction, Nature, Equipment, Items, None }
    public CatalogueCategory Category = CatalogueCategory.Items;
    public string? Group = null;
    public bool HiddenInCommands = false;

    // component shortcuts
    public string? Texture = null; // minecraft:icon
    public int MaxStackSize = 64; // minecraft:max_stack_size
    public string? DisplayName = null; // minecraft:display_name
    public bool AllowOffhand = false; // minecraft:allow_off_hand
    
    public Item(string identifier) : base(identifier) {}
    public Item(Identifier identifier) : base(identifier) {}
    
    public void AddComponent(IComponent<Item> component) => Components.Add(component.Identifier, component);
    
    public string Compile()
    {
        CompileTimeLogging.Push(Identifier.ToString());
        
        StringWriter sw = new();
        JsonTextWriter w = new(sw);
        w.Formatting = Formatting.Indented;
        w.Indentation = 4;
        
        w.WriteStartObject();
        
        Property(ref w, "format_version", FormatVersion.ToString());
        Object(ref w, "minecraft:item", w =>
        {
            CompileTimeLogging.Push("description");
            Object(ref w, "description", w =>
            {
                Property(ref w, "identifier", Identifier);
                Object(ref w, "menu_category", w =>
                {
                    if (Group?.Length > 256)
                        CompileTimeLogging.Warn(ref w, "item catalogue group exceeds 256 char limit");
                    
                    Property(ref w, "group", Group);
                    string categoryName = Enum.GetName(typeof(CatalogueCategory), Category)!.ToLower();
                    Property(ref w, "category", categoryName);
                    Property(ref w, "hidden_in_commands", HiddenInCommands);
                });
            });
            CompileTimeLogging.Pop();
            
            CompileTimeLogging.Push("components");
            Object(ref w, "components", w =>
            {
                if (Texture is not null)
                {
                    if (Components.ContainsKey(new("minecraft:icon")))
                    {
                        CompileTimeLogging.Warn(ref w, "item texture is defined in components and properties, component definition will take priority");
                        return;
                    }
                    
                    Object(ref w, "minecraft:icon", w => Property(ref w, "texture", Texture));
                }
                
                if (DisplayName is not null)
                {
                    if (Components.ContainsKey(new("minecraft:display_name")))
                    {
                        CompileTimeLogging.Warn(ref w, "item display name is defined in components and properties, component definition will take priority");
                        return;
                    }
                    
                    Object(ref w, "minecraft:display_name", w => Property(ref w, "value", DisplayName));
                }
                
                if (Components.ContainsKey(new("minecraft:display_name")) == false)
                    Object(ref w, "minecraft:max_stack_size", w => Property(ref w, "value", MaxStackSize));
                else
                    CompileTimeLogging.Warn(ref w, "item max stack size is defined in components and properties, component definition will take priority");
                
                if (Components.ContainsKey(new("minecraft:allow_off_hand")) == false)
                    Object(ref w, "minecraft:allow_off_hand", w => Property(ref w, "value", AllowOffhand));
                else
                    CompileTimeLogging.Warn(ref w, "item allow off hand is defined in components and properties, component definition will take priority");
                
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