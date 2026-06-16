using ingot.Core.Behaviour.Loot;
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
    public virtual LootTable? Loot => null;
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
        JsonHelper json = new(ref w);

        w.WriteStartObject();

        json.Property("format_version", inst.FormatVersion.ToString());
        json.Object("minecraft:block", () =>
        {
            CompilerState.Push("description");
            json.Object("description", () =>
            {
                json.Property("identifier", inst.Identifier);
                json.Object("states", () =>
                {
                    foreach (var kvp in inst.States)
                    {
                        int length = kvp.Value.Length;
                        if (length > 16)
                            CompilerState.Warn(ref w, $"block state {kvp.Key} has more than 16 possible permutations");
                        json.Property(kvp.Key, kvp.Value);
                    }
                });
            });
            CompilerState.Pop();
            
            CompilerState.Push("permutations");
            json.Array("permutations", () =>
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
            json.Object("components", () =>
            {
                foreach (string t in inst.Tags) 
                    json.Object($"tag:{t}", () => {});
                
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