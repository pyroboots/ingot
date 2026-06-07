using System.Reflection;
using ingot.Core.Common;
using Newtonsoft.Json;
using Version = ingot.Core.Common.Version;
using static ingot.Core.JsonHelper;

namespace ingot.Core.TraitSystem;

public abstract class Item
{
    public abstract string Identifier { get; }

    public Version FormatVersion = new("1.20.10");

    // header props
    public enum CatalogueCategory 
    { Construction, Nature, Equipment, Items, None }
    public CatalogueCategory Category = CatalogueCategory.Items;
    public string? Group = null;
    public bool HiddenInCommands = false;

    // component shortcuts
    public virtual string Texture => "minecraft:stick"; // minecraft:icon
    public virtual int MaxStackSize => 64; // minecraft:max_stack_size
    public virtual string DisplayName => Identifier; // minecraft:display_name
    public virtual bool AllowOffhand => false; // minecraft:allow_off_hand

    public static string Compile<TItem>() where TItem : Item, new()
    {
        TItem inst = Activator.CreateInstance<TItem>();
        
        CompileTimeLogging.Push(inst.Identifier);

        StringWriter sw = new();
        JsonTextWriter w = new(sw);
        w.Formatting = Formatting.Indented;
        w.Indentation = 4;

        w.WriteStartObject();

        Property(ref w, "format_version", inst.FormatVersion.ToString());
        Object(ref w, "minecraft:item", w =>
        {
            CompileTimeLogging.Push("description");
            Object(ref w, "description", w =>
            {
                Property(ref w, "identifier", inst.Identifier);
                Object(ref w, "menu_category", w =>
                {
                    if (inst.Group?.Length > 256)
                        CompileTimeLogging.Warn(ref w, "item catalogue group exceeds 256 char limit");

                    Property(ref w, "group", inst.Group);
                    string categoryName = Enum.GetName(typeof(CatalogueCategory), inst.Category)!.ToLower();
                    Property(ref w, "category", categoryName);
                    Property(ref w, "hidden_in_commands", inst.HiddenInCommands);
                });
            });
            CompileTimeLogging.Pop();

            CompileTimeLogging.Push("components");
            Object(ref w, "components", w =>
            {
                Object(ref w, "minecraft:icon", w => Property(ref w, "texture", inst.Texture));
                Object(ref w, "minecraft:display_name", w => Property(ref w, "value", inst.DisplayName));
                Object(ref w, "minecraft:max_stack_size", w => Property(ref w, "value", inst.MaxStackSize));
                Object(ref w, "minecraft:allow_off_hand", w => Property(ref w, "value", inst.AllowOffhand));

                foreach (Trait t in TraitSystem.GetTraits<TItem>(TraitSystem.TraitType.Item))
                    t.Compile(ref w);
            });
            CompileTimeLogging.Pop();
        });

        CompileTimeLogging.Pop();

        return sw.ToString();
    }
}