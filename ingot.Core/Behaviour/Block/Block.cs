using ingot.Core.Behaviour.Block.BlockTraits;
using ingot.Core.Behaviour.Loot;
using ingot.Core.Common;
using ingot.Core.Resource;
using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits;

using Newtonsoft.Json;

using static ingot.Core.Common.JsonHelper;

using Formatting = Newtonsoft.Json.Formatting;
using Version = ingot.Core.Common.Version;


namespace ingot.Core.Behaviour.Block;

/// <summary>
/// Implements basic properties of a block
/// </summary>
public abstract class Block : IConcreteCompilable<Block>, IIdentifiable, ITraitable
{
    /// <summary>
    /// Block identifier used in the game
    /// </summary>
    public abstract Identifier Identifier { get; }
    /// <summary>
    /// Block JSON format version. Defaults to <c>1.21.90</c> so custom components
    /// can be declared as direct entries under <c>components</c> (Custom Components V2).
    /// </summary>
    public virtual Version FormatVersion => new("1.21.90");

    /// <summary>
    /// Dictionary of possible block states. Valid state types are: <see cref="int"/>[], <see cref="float"/>[], <see cref="bool"/>[], <see cref="string"/>[], 
    /// </summary>
    public virtual Dictionary<string, object[]> States => new();
    /// <summary>
    /// List of possible block permutations
    /// </summary>
    public virtual BlockPermutation[] Permutations => [];
    /// <summary>
    /// Array of block tags that can enable / expand vanilla functionality
    /// </summary>
    public virtual string[] Tags => [];

    /// <summary>
    /// Which section of the creative inventory the block appears in
    /// </summary>
    public virtual Enums.CatalogueCategory Category => Enums.CatalogueCategory.Items;
    /// <summary>
    /// Which item group of <see cref="Category"/> the block appears in
    /// </summary>
    public virtual string? Group => null;

    /// <summary>
    /// Shortcut for the <c>minecraft:display_name</c> component
    /// </summary>
    public virtual string? DisplayName => null;
    /// <summary>
    /// Localized name written to <c>texts/en_US.lang</c>. Defaults to <see cref="DisplayName"/>.
    /// </summary>
    public virtual string? LangName => DisplayName;
    /// <summary>
    /// Shortcut for the <c>minecraft:geometry</c> component
    /// </summary>
    public virtual string? Geometry => "minecraft:geometry.full_block";
    /// <summary>
    /// Texture key written to <c>blocks.json</c> in the resource pack
    /// </summary>
    public virtual string? ResourceTexture => null;
    /// <summary>
    /// Sound identifier written to <c>blocks.json</c> in the resource pack
    /// </summary>
    public virtual string? Sound => null;
    /// <summary>
    /// Shortcut for the <c>minecraft:friction</c> component
    /// </summary>
    public virtual float? Friction => null;
    /// <summary>
    /// Shortcut for the <c>minecraft:light_dampening</c> component
    /// </summary>
    public virtual int? LightDampening => null;
    /// <summary>
    /// Shortcut for the <c>minecraft:light_emission</c> component
    /// </summary>
    public virtual int? LightEmission => null;
    /// <summary>
    /// Shortcut for the <c>minecraft:replaceable</c> component
    /// </summary>
    public virtual bool? Replaceable => null;
    /// <summary>
    /// Shortcut for the <c>minecraft:loot</c> component
    /// </summary>
    public virtual LootTable? Loot => null;
    /// <summary>
    /// Texture and materials for the <see cref="Block"/>. Shortcut for the <c>minecraft:material_instances</c> component
    /// </summary>
    public abstract MaterialInstances MaterialInstances { get; }

    /// <summary>
    /// Script API event bindings
    /// </summary>
    public virtual BlockEvents? BlockEvents => null;

    /// <inheritdoc/>
    public virtual Trait[] DynamicTraits => [];
    
    /// <inheritdoc/>
    public virtual Dictionary<Identifier, object> Singles => new();

    /// <summary>
    /// Vanilla description traits under <c>minecraft:block/description/traits</c>
    /// (placement direction/position, connection, multi-block, etc.).
    /// Not to be confused with component traits (<see cref="IBlockTrait"/>).
    /// </summary>
    public virtual IVanillaBlockTrait[] BlockTraits => [];
    
    /// <summary>
    /// Recipe to craft this block
    /// </summary>
    public virtual RecipeReference? Recipe => null;
    
    /// <inheritdoc/>
    public static string Compile(Type tType)
    {
        Block inst = (Activator.CreateInstance(tType) as Block)!;
        return CompileFromInstance(inst);
    }

    /// <inheritdoc/>
    public static string Compile<TConcreteType>() where TConcreteType : Block, new() => Compile(typeof(TConcreteType));

    /// <inheritdoc/>
    public static string CompileFromInstance(Block inst)
    {
        Type tType = inst.GetType();
        
        CompilerState.Push(inst.Identifier.ToString());

        StringWriter sw = new();
        JsonTextWriter w = new(sw);
        w.Formatting = Formatting.Indented;
        w.Indentation = 4;
        JsonHelper json = new(ref w);

        w.WriteStartObject();

        json.Property("format_version", inst.FormatVersion.ToString());
        json.Object("minecraft:block", () =>
        {
            json.Object("description", () =>
            {
                json.Property("identifier", inst.Identifier);

                if (inst.Category != Enums.CatalogueCategory.None)
                {
                    json.Object("menu_category", () =>
                    {
                        if (inst.Group?.Length > 256)
                            throw new ArgumentException($"block catalogue group ({inst.Group}) exceeds 256 char limit");

                        json.Property("group", inst.Group);
                        string categoryName = Enum.GetName(typeof(Enums.CatalogueCategory), inst.Category)!.ToLower();
                        json.Property("category", categoryName);
                    });
                }

                if (inst.BlockTraits.Length > 0)
                {
                    json.Object("traits", () =>
                    {
                        foreach (IVanillaBlockTrait trait in inst.BlockTraits)
                        {
                            if (inst.FormatVersion < trait.MinimumFormatVersion)
                            {
                                throw new ArgumentException(
                                    $"{trait.Identifier} requires minimum format version {trait.MinimumFormatVersion}, " +
                                    $"but {tType.Name} has {inst.FormatVersion}");
                            }

                            trait.Compile(ref w);
                        }
                    });
                }

                if (inst.States.Count > 0)
                {
                    json.Object("states", () =>
                    {
                        foreach (var kvp in inst.States)
                        {
                            int length = kvp.Value.Length;
                            if (length > 16)
                                throw new ArgumentException(
                                    $"block state {kvp.Key} has more than 16 possible permutations");
                            json.Property(kvp.Key, kvp.Value);
                        }
                    });
                }
            });

            if (inst.Permutations.Length > 0)
            {
                json.Array("permutations", () =>
                {
                    CompilerState.Info("compiling block permutations...");
                    int c = 0;
                    foreach (BlockPermutation p in inst.Permutations)
                    {
                        c++;
                        BlockPermutation.CompileFromInstance(p, ref w);
                        CompilerState.Info($"({c}/{inst.Permutations.Length}) compiled block permutation {p.GetType().Name}");
                    }
                    CompilerState.Info("compiled block permutations");
                });
            }

            json.Object("components", () =>
            {
                json.Property("minecraft:tags", inst.Tags);

                json.Property("minecraft:display_name", inst.DisplayName);
                json.Property("minecraft:friction", inst.Friction);
                json.Property("minecraft:light_emission", inst.LightEmission);
                json.Property("minecraft:light_dampening", inst.LightDampening);
                json.Property("minecraft:replaceable", inst.Replaceable);

                if (inst.Loot is not null)
                {
                    if (CompilerState.CurrentPack is not null
                        && CompilerState.CurrentPack.BehaviourPack.LootTables.All(t => t.GetType() != inst.Loot.GetType()))
                        // with loot because its not a component, and instead a reference to a compiled file, we
                        // just add it to the compilation list if its not already there
                        CompilerState.CurrentPack.BehaviourPack.AddLootTable(inst.Loot.GetType());

                    json.Property("minecraft:loot", inst.Loot.RelativePath);
                }

                json.Property("minecraft:geometry", inst.Geometry);

                inst.MaterialInstances.Compile(ref w);
                TextureAutoRegistration.RegisterMaterialInstances(inst.MaterialInstances, ref w);

                if (inst.BlockEvents is { HasEvents: true } blockEvents)
                {
                    if (CompilerState.CurrentPack is null)
                        CompilerState.Warn(ref w, "block events require pack compilation to generate scripts");
                    else
                    {
                        string jsonComponentName = blockEvents.GetJsonComponentName(inst.Identifier);
                        json.Object(jsonComponentName, () => { });
                        CompilerState.Info($"block event component {jsonComponentName}");
                    }
                }

                ITraitable.CompileTraits(inst, ref w, TraitSystem.TraitSystem.TraitType.Block);
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