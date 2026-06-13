using ingot.Core.TraitSystem;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;

namespace ingot.Core.Content.Block;

public abstract class BlockPermutation
{
    public abstract string Condition { get; }
    
    // component shortcuts
    public virtual string? DisplayName => null; // minecraft:display_name
    public virtual float? Friction => null; // minecraft:friction
    public virtual int? LightDampening => null; // minecraft:light_dampening
    public virtual int? LightEmission => null; // minecraft:light_emission
    public virtual bool? Replaceable => null; // minecraft:replaceable
    public virtual string? Loot => null;  // minecraft:loot
    public virtual MaterialInstances? MaterialInstances => null; // minecraft:material_instances

    public static void Compile<TBlockPermutation>(ref JsonTextWriter writer) where TBlockPermutation : BlockPermutation => Compile(typeof(TBlockPermutation), ref writer);
    public static void Compile(Type tBlockPermutation, ref JsonTextWriter writer)
    {
        BlockPermutation permutation = (Activator.CreateInstance(tBlockPermutation) as BlockPermutation)!;
        List<Trait> traits = TraitSystem.TraitSystem.GetTraits(tBlockPermutation, TraitSystem.TraitSystem.TraitType.Block);
        
        writer.WriteStartObject();
        
        Property(ref writer, "condition", permutation.Condition);
        Object(ref writer, "components", w =>
        {
            Property(ref w, "minecraft:display_name", permutation.DisplayName);
            Property(ref w, "minecraft:friction", permutation.Friction);
            Property(ref w, "minecraft:light_emission", permutation.LightEmission);
            Property(ref w, "minecraft:light_dampening", permutation.LightDampening);
            Property(ref w, "minecraft:replaceable", permutation.Replaceable);
            Property(ref w, "minecraft:loot", permutation.Loot);
            
            foreach (Trait trait in traits)
                trait.Compile(ref w);
        });
        
        writer.WriteEndObject();
    }
}