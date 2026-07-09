using ingot.Core.Common;
using ingot.Core.TraitSystem;

using Newtonsoft.Json;

using Formatting = Newtonsoft.Json.Formatting;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Behaviour.Item;

/// <summary>
/// Implements basic properties of an item
/// </summary>
public abstract class Item : IConcreteCompilable<Item>, IIdentifiable
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
    /// Optional path to the source PNG for <see cref="Texture"/>. When set, ingot auto-registers the
    /// item icon in the resource pack during compilation unless it was already added manually.
    /// </summary>
    public virtual string? TexturePath => null;
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
    /// Script API event bindings
    /// </summary>
    public virtual ItemEvents? ItemEvents => null;

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
                TextureAutoRegistration.RegisterItemTexture(inst.Texture, inst.TexturePath, ref w);

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
        });

        w.WriteEndObject();

        CompilerState.Pop();

        return sw.ToString();
    }
}