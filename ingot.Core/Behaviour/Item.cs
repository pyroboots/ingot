using ingot.Core.Common;
using ingot.Core.TraitSystem;
using Newtonsoft.Json;
using Version = ingot.Core.Common.Version;
using static ingot.Core.Common.JsonHelper;
using Formatting = Newtonsoft.Json.Formatting;

namespace ingot.Core.Behaviour;

/// <summary>
/// Implements basic properties of an item
/// </summary>
public abstract class Item : IConcreteCompilable<Item>
{
    /// <summary>
    /// Item identifier used in the game
    /// </summary>
    public abstract Identifier Identifier { get; }
    /// <summary>
    /// Minimum component version
    /// </summary>
    public virtual Version FormatVersion => new("1.20.10");

    /// <summary>
    /// Creative inventory tabs
    /// </summary>
    public enum CatalogueCategory 
    { 
        /// <summary>
        /// Construction tab
        /// </summary>
        Construction, 
        /// <summary>
        /// Nature tabs
        /// </summary>
        Nature, 
        /// <summary>
        /// Equipment tab
        /// </summary>
        Equipment, 
        /// <summary>
        /// Items tab
        /// </summary>
        Items, 
        /// <summary>
        /// Will not appear in the creative inventory
        /// </summary>
        None 
    }
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
    public virtual string DisplayName => Identifier.ToString();
    /// <summary>
    /// Shortcut for the <c>minecraft:allow_off_hand</c> component
    /// </summary>
    public virtual bool AllowOffhand => false;
    
    /// <summary>
    /// Compiles the <see cref="Item"/> (as <paramref name="tType"/>) to JSON
    /// </summary>
    /// <param name="tType">Concrete type of <see cref="Item"/></param>
    /// <returns>Compiled JSON</returns>
    public static string Compile(Type tType)
    {
        Item inst = (Activator.CreateInstance(tType) as Item)!;
        
        CompilerState.Push(inst.Identifier.ToString());

        StringWriter sw = new();
        JsonTextWriter w = new(sw);
        w.Formatting = Formatting.Indented;
        w.Indentation = 4;

        w.WriteStartObject();

        Property(ref w, "format_version", inst.FormatVersion.ToString());
        Object(ref w, "minecraft:item", w =>
        {
            CompilerState.Push("description");
            Object(ref w, "description", w =>
            {
                Property(ref w, "identifier", inst.Identifier);
                Object(ref w, "menu_category", w =>
                {
                    if (inst.Group?.Length > 256)
                        CompilerState.Warn(ref w, "item catalogue group exceeds 256 char limit");

                    Property(ref w, "group", inst.Group);
                    string categoryName = Enum.GetName(typeof(CatalogueCategory), inst.Category)!.ToLower();
                    Property(ref w, "category", categoryName);
                    Property(ref w, "hidden_in_commands", inst.HiddenInCommands);
                });
            });
            CompilerState.Pop();

            CompilerState.Push("components");
            Object(ref w, "components", w =>
            {
                Object(ref w, "minecraft:icon", w => Property(ref w, "texture", inst.Texture));
                Object(ref w, "minecraft:display_name", w => Property(ref w, "value", inst.DisplayName));
                Object(ref w, "minecraft:max_stack_size", w => Property(ref w, "value", inst.MaxStackSize));
                Object(ref w, "minecraft:allow_off_hand", w => Property(ref w, "value", inst.AllowOffhand));

                CompilerState.Info("compiling traits...");
                List<Trait> traits = TraitSystem.TraitSystem.GetTraits(tType, TraitSystem.TraitSystem.TraitType.Item);
                int c = 0;
                foreach (Trait t in traits)
                {
                    c++;
                    t.Compile(ref w);
                    CompilerState.Info($"({c}/{traits.Count}) compiled trait {t.RootTrait.Name}");
                }
            });
            CompilerState.Pop();
        });

        CompilerState.Pop();

        return sw.ToString();
    }
}