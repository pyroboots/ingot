using ingot.Core.TraitSystem;
using Newtonsoft.Json;
using Version = ingot.Core.Common.Version;
using static ingot.Core.Common.JsonHelper;

namespace ingot.Core.Behaviour;

/// <summary>
/// Implements basic properties of an item
/// </summary>
public abstract class Item
{
    /// <summary>
    /// Item identifier used in the game
    /// </summary>
    public abstract string Identifier { get; }
    /// <summary>
    /// Minimum component version
    /// </summary>
    public virtual Version FormatVersion => new("1.20.10");

    // header props
    public enum CatalogueCategory 
    { Construction, Nature, Equipment, Items, None }
    /// <summary>
    /// Which section of the creative inventory the item appears in
    /// </summary>
    public virtual CatalogueCategory Category => CatalogueCategory.Items;
    /// <summary>
    /// Which item group of <see cref="CatalogueCategory"/> the item appears in
    /// </summary>
    public virtual string? Group => null;
    /// <summary>
    /// Whether the item is visible by command item arguments
    /// </summary>
    public virtual bool HiddenInCommands => false;
    
    /// <summary>
    /// Shortcut for the <c>minecraft:icon</c> component
    /// </summary>
    public abstract string Texture { get; }
    /// <summary>
    /// Shortcut for the <c>minecraft:max_stack_size</c> component
    /// </summary>
    public virtual int MaxStackSize => 64;
    /// <summary>
    /// Shortcut for the <c>minecraft:display_name</c> component
    /// </summary>
    public virtual string DisplayName => Identifier;
    /// <summary>
    /// Shortcut for the <c>minecraft:allow_off_hand</c> component
    /// </summary>
    public virtual bool AllowOffhand => false;

    /// <summary>
    /// Compiles the <typeparamref name="TItem"/> to JSON
    /// </summary>
    /// <typeparam name="TItem">The type class to compile</typeparam>
    public static string Compile<TItem>() where TItem : Item, new() => Compile(typeof(TItem));
    public static string Compile(Type tItem)
    {
        Item inst = (Activator.CreateInstance(tItem) as Item)!;
        
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

                CompileTimeLogging.Info("compiling traits...");
                List<Trait> traits = TraitSystem.TraitSystem.GetTraits(tItem, TraitSystem.TraitSystem.TraitType.Item);
                int c = 0;
                foreach (Trait t in traits)
                {
                    c++;
                    t.Compile(ref w);
                    CompileTimeLogging.Info($"({c}/{traits.Count}) compiled trait {t.RootTrait.Name}");
                }
            });
            CompileTimeLogging.Pop();
        });

        CompileTimeLogging.Pop();

        return sw.ToString();
    }
}