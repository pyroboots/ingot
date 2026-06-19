using ingot.Core.Common;
using ingot.Core.TraitSystem;

using Newtonsoft.Json;

using static ingot.Core.Common.JsonHelper;

using Formatting = Newtonsoft.Json.Formatting;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Behaviour;

/// <summary>
/// Implements basic properties of an item
/// </summary>
public abstract class Item : IConcreteCompilable<Item>, IIdentifiable
{
    /// <inheritdoc/>
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
            CompilerState.Push("description");
            json.Object("description", () =>
            {
                json.Property("identifier", inst.Identifier);
                json.Object("menu_category", () =>
                {
                    if (inst.Group?.Length > 256)
                        CompilerState.Warn(ref w, "item catalogue group exceeds 256 char limit");

                    json.Property("group", inst.Group);
                    string categoryName = Enum.GetName(typeof(CatalogueCategory), inst.Category)!.ToLower();
                    json.Property("category", categoryName);
                    json.Property("hidden_in_commands", inst.HiddenInCommands);
                });
            });
            CompilerState.Pop();

            CompilerState.Push("components");
            json.Object("components", () =>
            {
                TextureAutoRegistration.RegisterItemTexture(inst.Texture, inst.TexturePath, ref w);

                json.Object("minecraft:icon", () => json.Property("texture", inst.Texture));
                json.Object("minecraft:display_name", () => json.Property("value", inst.DisplayName));
                json.Object("minecraft:max_stack_size", () => json.Property("value", inst.MaxStackSize));
                json.Object("minecraft:allow_off_hand", () => json.Property("value", inst.AllowOffhand));

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