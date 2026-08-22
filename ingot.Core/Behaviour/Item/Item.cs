using System.Runtime.CompilerServices;

using ingot.Core.Common;
using ingot.Core.Resource;
using ingot.Core.TraitSystem;

using Newtonsoft.Json;

using Formatting = Newtonsoft.Json.Formatting;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Behaviour.Item;

/// <summary>
/// Implements basic properties of an item
/// </summary>
public abstract class Item : IConcreteCompilable<Item>, IIdentifiable, ITraitable
{
    /// <inheritdoc/>
    public abstract Identifier Identifier { get; }
    /// <summary>
    /// Item JSON format version. Defaults to <c>1.21.90</c> so custom components
    /// can be declared as direct entries under <c>components</c> (Custom Components V2).
    /// </summary>
    public virtual Version FormatVersion => new("1.21.90");
    
    /// <summary>
    /// Which section of the creative inventory the item appears in
    /// </summary>
    public virtual Enums.CatalogueCategory Category => Enums.CatalogueCategory.Items;
    /// <summary>
    /// Which item group of <see cref="Enums.CatalogueCategory"/> the item appears in
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
    /// Recipe to craft this item
    /// </summary>
    public virtual RecipeReference? Recipe => null;

    /// <summary>
    /// Script API event bindings
    /// </summary>
    public virtual ItemEvents? ItemEvents => null;
    
    /// <inheritdoc/>
    public virtual Trait[] DynamicTraits => [];
    
    /// <inheritdoc/>
    public virtual Dictionary<Identifier, object> Singles => new();
    
    /// <inheritdoc/>
    public static string Compile(Type tType)
    {
        Item inst = (Activator.CreateInstance(tType) as Item)!;
        return CompileFromInstance(inst);
    }

    /// <inheritdoc/>
    public static string Compile<TConcreteType>() where TConcreteType : Item, new() => Compile(typeof(TConcreteType));

    /// <inheritdoc/>
    public static string CompileFromInstance(Item inst)
    {
        Type tType = inst.GetType();
        
        CompilerState.Push(inst.Identifier.ToString());

        StringWriter sw = new();
        JsonWriter w = new JsonTextWriter(sw)
        {
            Formatting = Formatting.Indented,
            Indentation = 4,
        };
        

        JsonHelper json = new(ref w);

        w.WriteStartObject();

        json.Property("format_version", inst.FormatVersion.ToString());
        json.Object("minecraft:item", () =>
        {
            json.Object("description", () =>
            {
                json.Property("identifier", inst.Identifier);
                json.Object("menu_category", () =>
                {
                    if (inst.Group?.Length > 256)
                        throw new ArgumentException("item catalogue group exceeds 256 char limit");

                    json.Property("group", inst.Group);
                    string categoryName = Enum.GetName(typeof(Enums.CatalogueCategory), inst.Category)!.ToLower();
                    json.Property("category", categoryName);
                    json.Property("hidden_in_commands", inst.HiddenInCommands);
                });
            });

            json.Object("components", () =>
            {
                json.Object("minecraft:icon", () =>
                {
                    if (inst.FormatVersion.Major > 1
                        || (inst.FormatVersion.Major == 1 && inst.FormatVersion.Minor >= 21))
                    {
                        json.Object("textures", () => json.Property("default", inst.Texture));
                    }
                    else
                    {
                        json.Property("texture", inst.Texture);
                    }
                });
                json.Object("minecraft:display_name", () => json.Property("value", inst.DisplayName));
                json.Object("minecraft:max_stack_size", () => json.Property("value", inst.MaxStackSize));
                json.Object("minecraft:allow_off_hand", () => json.Property("value", inst.AllowOffhand));

                if (inst.ItemEvents is { HasEvents: true } itemEvents)
                {
                    if (CompilerState.CurrentPack is null)
                        CompilerState.Warn(ref w, "item events require pack compilation to generate scripts");
                    else
                    {
                        string jsonComponentName = itemEvents.GetJsonComponentName(inst.Identifier);
                        json.Object(jsonComponentName, () => { });
                        CompilerState.Info($"item event component {jsonComponentName}");
                    }
                }

                ITraitable.CompileTraits(inst, ref w, TraitSystem.TraitSystem.TraitType.Item);
            });

            // c# doesnt actually run ctors until accessed because its lazy, so 
            // we have to touch it in some way to get it to. we can just pipe the
            // value into discard
            _ = inst.Recipe;
        });

        w.WriteEndObject();

        CompilerState.Pop();

        return sw.ToString();
    }
}