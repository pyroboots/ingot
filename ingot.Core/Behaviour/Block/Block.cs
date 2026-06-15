using ingot.Core.Common;
using ingot.Core.TraitSystem;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;
using Formatting = Newtonsoft.Json.Formatting;
using Version = System.Version;

namespace ingot.Core.Behaviour.Block;

/// <summary>
/// Implements basic properties of a block
/// </summary>
public abstract class Block : IConcreteCompilable<Block>
{
    /// <summary>
    /// Block identifier used in the game
    /// </summary>
    public abstract Identifier Identifier { get; }
    /// <summary>
    /// Minimum component version
    /// </summary>
    public virtual Version FormatVersion => new("1.20.10");

    /// <summary>
    /// Dictionary of possible block states. Valid state types are: <see cref="int"/>[], <see cref="float"/>[], <see cref="bool"/>[], <see cref="string"/>[], 
    /// </summary>
    public virtual Dictionary<string, dynamic[]> States => new();
    /// <summary>
    /// List of possible block permutations
    /// </summary>
    public virtual List<BlockPermutation> Permutations => new();
    /// <summary>
    /// Array of block tags that can enable / expand vanilla functionality
    /// </summary>
    public virtual string[] Tags => [];
    
    /// <summary>
    /// Shortcut for the <c>minecraft:display_name</c> component
    /// </summary>
    public virtual string? DisplayName => null;
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
    public virtual string? Loot => null;
    /// <summary>
    /// Texture and materials for the <see cref="Block"/>. Shortcut for the <c>minecraft:material_instances</c> component
    /// </summary>
    public abstract MaterialInstances MaterialInstances { get; }

    /// <summary>
    /// Compiles the <see cref="Block"/> (as <paramref name="tType"/>) to JSON
    /// </summary>
    /// <param name="tType">Concrete type of <see cref="Block"/></param>
    /// <returns>Compiled JSON</returns>
    public static string Compile(Type tType)
    {
        Block inst = (Activator.CreateInstance(tType) as Block)!;
        
        CompilerState.Push(inst.Identifier.ToString());

        StringWriter sw = new();
        JsonTextWriter w = new(sw);
        w.Formatting = Formatting.Indented;
        w.Indentation = 4;

        w.WriteStartObject();

        Property(ref w, "format_version", inst.FormatVersion.ToString());
        Object(ref w, "minecraft:block", w =>
        {
            CompilerState.Push("description");
            Object(ref w, "description", w =>
            {
                Property(ref w, "identifier", inst.Identifier);
                Object(ref w, "states", w =>
                {
                    foreach (var kvp in inst.States)
                    {
                        int length = kvp.Value.Length;
                        if (length > 16)
                            CompilerState.Warn(ref w, $"block state {kvp.Key} has more than 16 possible permutations");
                        Property(ref w, kvp.Key, kvp.Value);
                    }
                });
            });
            CompilerState.Pop();
            
            CompilerState.Push("permutations");
            Array(ref w, "permutations", w =>
            {
                CompilerState.Info("compiling block permutations...");
                int c = 0;
                foreach (BlockPermutation p in inst.Permutations)
                {
                    c++;
                    BlockPermutation.Compile(p.GetType(), ref w);
                    CompilerState.Info($"({c}/{inst.Permutations.Count}) compiled block permutation {p.GetType().Name}");
                }
                CompilerState.Info("compiled block permutations");
            });
            CompilerState.Pop();
            
            CompilerState.Push("components");
            Object(ref w, "components", w =>
            {
                Property(ref w, "minecraft:display_name", inst.DisplayName);
                Property(ref w, "minecraft:friction", inst.Friction);
                Property(ref w, "minecraft:light_emission", inst.LightEmission);
                Property(ref w, "minecraft:light_dampening", inst.LightDampening);
                Property(ref w, "minecraft:replaceable", inst.Replaceable);
                Property(ref w, "minecraft:loot", inst.Loot);
                
                inst.MaterialInstances.Compile(ref w);

                CompilerState.Info("compiling traits...");
                List<Trait> traits = TraitSystem.TraitSystem.GetTraits(tType, TraitSystem.TraitSystem.TraitType.Block);
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
        
        w.WriteEndObject();

        CompilerState.Pop();

        return sw.ToString();
    }
}