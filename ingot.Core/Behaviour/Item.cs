using System.Text;

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

                if (inst.ItemEvents is { HasEvents: true } itemEvents)
                {
                    if (CompilerState.CurrentPack is null)
                        CompilerState.Warn(ref w, "item events require pack compilation to generate scripts");
                    else if (!CompilerState.CurrentPack.ScriptsEnabled)
                        CompilerState.Warn(ref w, "item events require ScriptsEnabled on the pack");
                    else
                    {
                        (string jsonComponentName, string code) = itemEvents.Compile(inst.Identifier);
                        CompilerState.CurrentPack.RegisterGeneratedScript(itemEvents.GetScriptPath(inst.Identifier), code);
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
            CompilerState.Pop();
        });

        CompilerState.Pop();

        return sw.ToString();
    }
}

/// <summary>
/// Autogenerates Script API bindings for item events
/// </summary>
public struct ItemEvents : IScriptEvents
{
    /// <summary>Called when an item containing this component is hitting an entity and about to take durability damage</summary>
    public string? BeforeDurabilityDamageEvent; // onBeforeDurabilityDamage
    /// <summary>Called when the use duration of the item containing this component is completed</summary>
    /// <remarks>The complete use event requires the <c>minecraft:use_modifiers</c> component to be active on your item to trigger</remarks>
    public string? CompleteUseEvent; // onCompleteUse
    /// <summary>Called when an item containing this component is eaten by an entity</summary>
    /// <remarks>The complete use event requires the <c>minecraft:use_modifiers</c> and the <c>minecraft:food</c> component to be active on your item to trigger</remarks>
    public string? ConsumeEvent; // onConsume
    /// <summary>Called when an item containing this component is used to hit another entity</summary>
    public string? HitEntityEvent; // onHitEntity
    /// <summary>Called when an item containing this component is used to mine a block</summary>
    public string? MineBlockEvent; // onMineBlock
    /// <summary>Called when an item containing this component is used by a player</summary>
    public string? UseEvent; // onUse
    /// <summary>Called when an item containing this component is used on a block</summary>
    public string? UseOnEvent; // onUseOn

    /// <inheritdoc/>
    public object?[] Events => 
    [
        BeforeDurabilityDamageEvent,
        CompleteUseEvent,
        ConsumeEvent,
        HitEntityEvent,
        MineBlockEvent,
        UseEvent,
        UseOnEvent,
    ];
    /// <inheritdoc/>
    public bool HasEvents => Events.Any(e => e is not null);

    /// <inheritdoc/>
    public string GetScriptPath(Identifier id) => $"scripts/items/{id.Namespace}_{id.Name}_events.js";

    /// <inheritdoc/>
    public (string jsonComponentName, string code) Compile(Identifier id)
    {
        StringBuilder sb = new();
        sb.AppendLine("// autogenerated by ingot");

        sb.AppendLine("import { system } from \"@minecraft/server\";");
        sb.AppendLine();

        string codeComponentName = ingot.Core.Common.Formatting.SnakeToPascalCase(string.Join('_', [
            id.Namespace,
            id.Name,
            "item_events_component"
        ]));
        string jsonComponentName = $"{id.Namespace}:" + ingot.Core.Common.Formatting.PascalToSnakeCase(codeComponentName);

        sb.AppendLine($"const {codeComponentName} = {{");

        if (BeforeDurabilityDamageEvent is not null)
            sb.AppendLine(ComponentEvent("onBeforeDurabilityDamage", BeforeDurabilityDamageEvent));
        if (CompleteUseEvent is not null)
            sb.AppendLine(ComponentEvent("onCompleteUse", CompleteUseEvent));
        if (ConsumeEvent is not null)
            sb.AppendLine(ComponentEvent("onConsume", ConsumeEvent));
        if (HitEntityEvent is not null)
            sb.AppendLine(ComponentEvent("onHitEntity", HitEntityEvent));
        if (MineBlockEvent is not null)
            sb.AppendLine(ComponentEvent("onMineBlock", MineBlockEvent));
        if (UseEvent is not null)
            sb.AppendLine(ComponentEvent("onUse", UseEvent));
        if (UseOnEvent is not null)
            sb.AppendLine(ComponentEvent("onUseOn", UseOnEvent));

        sb.AppendLine("};");
        sb.AppendLine();

        sb.AppendLine("system.beforeEvents.startup.subscribe(({ itemComponentRegistry }) => {");
        sb.AppendLine($"    itemComponentRegistry.registerCustomComponent(");
        sb.AppendLine($"        \"{jsonComponentName}\",");
        sb.AppendLine($"        {codeComponentName}");
        sb.AppendLine($"    );");
        sb.AppendLine("});");

        return (jsonComponentName, sb.ToString());
    }
    
    private string ComponentEvent(string name, string content)
    {
        StringBuilder sb = new();

        /*
         * onEntityFallOn(event) {
         *      ...
         * },
         */

        sb.Append(name);
        sb.AppendLine("(event) {");
        sb.AppendLine(content);
        sb.AppendLine("},");

        return sb.ToString();
    }
}