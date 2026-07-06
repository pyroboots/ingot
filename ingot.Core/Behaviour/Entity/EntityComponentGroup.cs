using ingot.Core.Common;
using ingot.Core.TraitSystem;

using Newtonsoft.Json;

namespace ingot.Core.Behaviour.Entity;

/// <summary>
/// C# representation of an entity permutation (<c>component_group</c>)
/// </summary>
public abstract class EntityComponentGroup : ICompilableFragment, IIdentifiable
{
    /// <inheritdoc/>
    public abstract Identifier Identifier { get; }
    
    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        CompilerState.Push(Identifier.ToString());
        
        List<Trait> traits = TraitSystem.TraitSystem.GetTraits(GetType(), TraitSystem.TraitSystem.TraitType.Entity);
        
        writer.WriteStartObject();
        int c = 0;
        foreach (Trait t in traits)
        {
            c++;
            t.Compile(ref writer);
            CompilerState.Info($"({c}/{traits.Count}) compiled trait {t.RootTrait.Name}");
        }
        
        writer.WriteEndObject();
        
        CompilerState.Pop();
    }
}