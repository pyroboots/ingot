using ingot.Core.TraitSystem;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;

namespace ingot.Core.Behaviour.Block;

/// <summary>
/// C# representation of a block permutation
/// </summary>
public abstract class BlockPermutation
{
    /// <summary>
    /// Molang condition that determines when this permutation is active
    /// </summary>
    public abstract string Condition { get; }
    
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
    /// Texture and materials for the <see cref="BlockPermutation"/>. Shortcut for the <c>minecraft:material_instances</c> component
    /// </summary>
    public virtual MaterialInstances? MaterialInstances => null;

    /// <summary>
    /// Compiles the <typeparamref name="TBlockPermutation"/> to JSON
    /// </summary>
    /// <typeparam name="TBlockPermutation">The type class block permutation to compile</typeparam>
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