using ingot.Core.TraitSystem;

using Newtonsoft.Json;

namespace ingot.Core.Common;

/// <summary>
/// Internal use interface to handle JSON serialization of traits, singles and dynamic traits
/// </summary>
public interface ITraitable
{
    /// <summary>
    /// Array of constructed traits to facilitate traits that ingot may not implement
    /// </summary>
    public abstract Trait[] DynamicTraits { get; }

    /// <summary>
    /// Array of components that do not have object bodies: <c>"namespace:component": "string value"</c>
    /// Majority are handled via class properties but this exists for support
    /// </summary>
    public abstract Dictionary<Identifier, object> Singles { get; }

    /// <summary>
    /// Write trait JSON to <see cref="JsonTextWriter"/> <paramref name="w"/>
    /// </summary>
    /// <param name="inst">Instance to read from</param>
    /// <param name="w"><see cref="JsonTextWriter"/> to write JSON to</param>
    /// <param name="type">Expected trait type</param>
    public static void CompileTraits(ITraitable inst, ref JsonWriter w, TraitSystem.TraitSystem.TraitType type)
    {
        CompilerState.Info("compiling traits...");
        List<Trait> traits = TraitSystem.TraitSystem.GetTraits(inst, type);
        int c = 0;
        foreach (Trait t in traits)
        {
            c++;
            t.Compile(ref w);
            CompilerState.Info($"({c}/{traits.Count}) compiled trait {t.RootTrait!.Name}");
        }

        c = 0;
        foreach (Trait t in inst.DynamicTraits)
        {
            c++;
            t.Compile(ref w);
            CompilerState.Info($"({c}/{inst.DynamicTraits.Length}) compiled dynamic trait {t.Identifier}");
        }
                
        c = 0;
        foreach (var kvp in inst.Singles)
        {
            c++;
            JsonHelper.Property(ref w, kvp.Key.ToString(), kvp.Value);
            CompilerState.Info($"({c}/{inst.DynamicTraits.Length}) compiled single {kvp.Key}");
        }
    }
}