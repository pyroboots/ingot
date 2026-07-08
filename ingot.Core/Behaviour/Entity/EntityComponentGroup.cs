using ingot.Core.Common;
using ingot.Core.TraitSystem;

using Newtonsoft.Json;

namespace ingot.Core.Behaviour.Entity;

/// <summary>
/// C# representation of an entity permutation (<c>component_group</c>)
/// </summary>
public abstract class EntityComponentGroup : ICompilableFragment, IIdentifiable
{
    /// <summary>
    /// Identifier of the component group
    /// </summary>
    public abstract Identifier Identifier { get; }

    /// <summary>
    /// Parent <see cref="Entity"/> of this <see cref="EntityComponentGroup"/>
    /// </summary>
    public abstract Entity Parent { get; }
    
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

/// <summary>
/// Component group whose parent entity type is <typeparamref name="TParent"/>, so
/// <see cref="Parent"/> is inferred without re-stating it on every group.
/// </summary>
/// <typeparam name="TParent">Behaviour entity that owns this group.</typeparam>
public abstract class EntityComponentGroup<TParent> : EntityComponentGroup
    where TParent : Entity, new()
{
    /// <inheritdoc/>
    public override Entity Parent => new TParent();

    /// <summary>
    /// Builds a group id in the parent entity's namespace with the given name segment
    /// (e.g. parent <c>test:custom_cow</c> + <c>custom_cow_baby</c>).
    /// </summary>
    protected Identifier GroupId(string name) =>
        new(Parent.Identifier.Namespace, name);

    /// <summary>
    /// Builds a group id by appending a suffix to the parent entity name
    /// (e.g. parent <c>test:custom_cow</c> + <c>_baby</c> → <c>test:custom_cow_baby</c>).
    /// </summary>
    protected Identifier GroupIdFromParent(string suffix) =>
        Parent.Identifier.WithNameSuffix(suffix);
}