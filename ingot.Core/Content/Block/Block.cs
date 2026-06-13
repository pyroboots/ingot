using ingot.Core.TraitSystem;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;

namespace ingot.Core.Content.Block;

public abstract class Block
{
    public abstract string Identifier { get; }
    public virtual Version FormatVersion => new("1.20.10");

    public virtual Dictionary<string, dynamic[]> States => new();
    public virtual List<BlockPermutation> Permutations => new();
    
    // component shortcuts
    public virtual string? DisplayName => null; // minecraft:display_name
    public virtual float? Friction => null; // minecraft:friction
    public virtual int? LightDampening => null; // minecraft:light_dampening
    public virtual int? LightEmission => null; // minecraft:light_emission
    public virtual bool? Replaceable => null; // minecraft:replaceable
    public virtual string? Loot => null;  // minecraft:loot
    public abstract MaterialInstances MaterialInstances { get; } // minecraft:material_instances

    public static string Compile<TBlock>() where TBlock : Block, new() => Compile(typeof(TBlock));
    public static string Compile(Type tBlock)
    {
        Block inst = (Activator.CreateInstance(tBlock) as Block)!;
        
        CompileTimeLogging.Push(inst.Identifier);

        StringWriter sw = new();
        JsonTextWriter w = new(sw);
        w.Formatting = Formatting.Indented;
        w.Indentation = 4;

        w.WriteStartObject();

        Property(ref w, "format_version", inst.FormatVersion.ToString());
        Object(ref w, "minecraft:block", w =>
        {
            CompileTimeLogging.Push("description");
            Object(ref w, "description", w =>
            {
                Property(ref w, "identifier", inst.Identifier);
                Object(ref w, "states", w =>
                {
                    foreach (var kvp in inst.States)
                        Property(ref w, kvp.Key, kvp.Value);
                });
            });
            CompileTimeLogging.Pop();
            
            CompileTimeLogging.Push("permutations");
            Array(ref w, "permutations", w =>
            {
                foreach (BlockPermutation p in inst.Permutations)
                    BlockPermutation.Compile(p.GetType(), ref w);
            });
            CompileTimeLogging.Pop();
            
            CompileTimeLogging.Push("components");
            Object(ref w, "components", w =>
            {
                Property(ref w, "minecraft:display_name", inst.DisplayName);
                Property(ref w, "minecraft:friction", inst.Friction);
                Property(ref w, "minecraft:light_emission", inst.LightEmission);
                Property(ref w, "minecraft:light_dampening", inst.LightDampening);
                Property(ref w, "minecraft:replaceable", inst.Replaceable);
                Property(ref w, "minecraft:loot", inst.Loot);
                
                inst.MaterialInstances.Compile(ref w);
                
                foreach (Trait t in TraitSystem.TraitSystem.GetTraits(tBlock, TraitSystem.TraitSystem.TraitType.Block))
                    t.Compile(ref w);
            });
            CompileTimeLogging.Pop();
        });
        
        w.WriteEndObject();

        CompileTimeLogging.Pop();

        return sw.ToString();
    }
}