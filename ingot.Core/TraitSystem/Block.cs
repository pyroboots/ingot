using ingot.Core.TraitSystem.Traits;
using Newtonsoft.Json;
using static ingot.Core.JsonHelper;

namespace ingot.Core.TraitSystem;

public abstract class Block
{
    public abstract string Identifier { get; }
    public virtual Version FormatVersion => new("1.20.10");

    public virtual Dictionary<string, dynamic[]> States => new();
    public virtual List<BlockPermutation> Permutations => new();

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
                    {
                        
                        Property(ref w, kvp.Key, kvp.Value);
                    }
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
                foreach (Trait t in TraitSystem.GetTraits(tBlock, TraitSystem.TraitType.Block))
                    t.Compile(ref w);
            });
            CompileTimeLogging.Pop();
        });
        
        w.WriteEndObject();

        CompileTimeLogging.Pop();

        return sw.ToString();
    }
}