using ingot.Core.Behaviour.Loot;
using ingot.Core.Common;
using ingot.Core.TraitSystem;

using Newtonsoft.Json;

using static ingot.Core.Common.JsonHelper;

namespace ingot.Core.Behaviour.Block;

/// <summary>
/// C# representation of a block permutation
/// </summary>
public abstract class BlockPermutation : ITraitable
{
    /// <summary>
    /// Molang condition that determines when this permutation is active
    /// </summary>
    public abstract Molang Condition { get; }
    /// <summary>
    /// Parent <see cref="Block"/> of this <see cref="BlockPermutation"/>
    /// </summary>
    public abstract Block Parent { get; }

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
    /// Texture and materials for the <see cref="BlockPermutation"/>. Shortcut for the <c>minecraft:material_instances</c> component
    /// </summary>
    public virtual MaterialInstances? MaterialInstances => null;

    /// <summary>
    /// Array of block tags that can enable / expand vanilla functionality
    /// </summary>
    public virtual string[] Tags => [];
    
    /// <inheritdoc/>
    public virtual Trait[] DynamicTraits => [];

    /// <inheritdoc/>
    public virtual Dictionary<Identifier, object> Singles => new();

    /// <summary>
    /// Compiles the <typeparamref name="TBlockPermutation"/> to JSON
    /// </summary>
    /// <typeparam name="TBlockPermutation">The type class block permutation to compile</typeparam>
    public static void Compile<TBlockPermutation>(ref JsonWriter writer) where TBlockPermutation : BlockPermutation => Compile(typeof(TBlockPermutation), ref writer);

    /// <summary>
    /// Compiles the <see cref="BlockPermutation"/> (as <paramref name="tBlockPermutation"/>) to JSON
    /// </summary>
    /// <param name="tBlockPermutation">Concrete type of <see cref="BlockPermutation"/></param>
    /// <param name="writer">JSON source stream to write to</param>
    public static void Compile(Type tBlockPermutation, ref JsonWriter writer)
    {
        BlockPermutation permutation = (BlockPermutation)Activator.CreateInstance(tBlockPermutation)!;
        CompileFromInstance(permutation, ref writer);
    }

    /// <summary>
    /// Compiles a pre-constructed instance of a <see cref="BlockPermutation"/> to JSON.
    /// Useful for runtime configuration and deriving multiple objects from a single parent concrete type (e.g. having a <c>MasterStone</c> type and changing the explosion resistance at runtime to create a new block)
    /// </summary>
    /// <param name="permutation">Instance to compile</param>
    /// <param name="writer">JSON source stream to write to</param>
    /// <returns>Compiled JSON</returns>
    public static void CompileFromInstance(BlockPermutation permutation, ref JsonWriter writer)
    {
        if (permutation.MaterialInstances is not null)
        {
            JsonWriter? warnWriter = null;
            TextureAutoRegistration.RegisterMaterialInstances(permutation.MaterialInstances.Value, ref warnWriter);
        }

        JsonHelper json = new(ref writer);

        writer.WriteStartObject();

        json.Property("condition", permutation.Condition.ToString());
        json.Object("components", () =>
        {
            json.Property("minecraft:tags", permutation.Tags);

            json.Property("minecraft:display_name", permutation.DisplayName);
            json.Property("minecraft:friction", permutation.Friction);
            json.Property("minecraft:light_emission", permutation.LightEmission);
            json.Property("minecraft:light_dampening", permutation.LightDampening);
            json.Property("minecraft:replaceable", permutation.Replaceable);

            if (permutation.Loot is not null)
            {
                if (CompilerState.CurrentPack is not null
                    && CompilerState.CurrentPack.BehaviourPack.LootTables.All(t => t.GetType() != permutation.Loot.GetType()))
                    // with loot because its not a component, and instead a reference to a compiled file, we
                    // just add it to the compilation list if its not already there
                    CompilerState.CurrentPack.BehaviourPack.AddLootTable(permutation.Loot.GetType());

                json.Property("minecraft:loot", permutation.Loot.RelativePath);
            }

            if (permutation.MaterialInstances is not null)
                permutation.MaterialInstances.Value.Compile(ref json.Writer);

            ITraitable.CompileTraits(permutation, ref json.Writer, TraitSystem.TraitSystem.TraitType.Block);
        });

        writer.WriteEndObject();
    }
}