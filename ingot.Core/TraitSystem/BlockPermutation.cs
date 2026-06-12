using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits;
using Newtonsoft.Json;
using static ingot.Core.JsonHelper;

namespace ingot.Core.TraitSystem;

public abstract class BlockPermutation
{
    public abstract string Condition { get; }

    public static void Compile<TBlockPermutation>(ref JsonTextWriter writer) where TBlockPermutation : BlockPermutation => Compile(typeof(TBlockPermutation), ref writer);
    public static void Compile(Type tBlockPermutation, ref JsonTextWriter writer)
    {
        BlockPermutation permutation = (Activator.CreateInstance(tBlockPermutation) as BlockPermutation)!;
        List<Trait> traits = TraitSystem.GetTraits(tBlockPermutation, TraitSystem.TraitType.Block);
        
        writer.WriteStartObject();
        
        Property(ref writer, "condition", permutation.Condition);
        Object(ref writer, "components", w =>
        {
            foreach (Trait trait in traits)
                trait.Compile(ref w);
        });
        
        writer.WriteEndObject();
    }
}