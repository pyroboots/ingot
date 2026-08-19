using ingot.Core.Common;
using ingot.Core.TraitSystem;

using Newtonsoft.Json;

using Formatting = Newtonsoft.Json.Formatting;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Behaviour.Entity;

/// <summary>
/// Implements basic properties of an entity
/// </summary>
public abstract class Entity : IConcreteCompilable<Entity>, IIdentifiable, ITraitable
{
    /// <inheritdoc/>
    public abstract Identifier Identifier { get; }
    /// <summary>
    /// Minimum component version written to <c>format_version</c> in the generated entity JSON.
    /// </summary>
    public virtual Version FormatVersion => new("1.20.10");
    /// <summary>
    /// Whether the entity can spawn naturally in the world.
    /// </summary>
    public virtual bool IsSpawnable => false;
    /// <summary>
    /// Whether the entity can be summoned with commands.
    /// </summary>
    public virtual bool IsSummonable => true;
    /// <summary>
    /// Whether the entity requires experimental gameplay.
    /// </summary>
    public virtual bool IsExperimental => false;

    /// <summary>
    /// List of entity <c>component_group</c>s
    /// </summary>
    public virtual EntityComponentGroup[] ComponentGroups => [];

    /// <summary>
    /// Optional parameter that is used to imitate a vanilla entity's hard-coded elements
    /// </summary>
    public virtual Identifier? RuntimeIdentifier => null;
    
    /// <summary>
    /// List of entity <c>event</c>s
    /// </summary>
    public virtual Dictionary<Identifier, IEntityEventAction[]> Events => new();

    /// <summary>
    /// List of entity properties
    /// </summary>
    public virtual Dictionary<Identifier, IEntityProperty> Properties => new();

    /// <summary>
    /// Optional explicit client-entity type for resource-pack visuals.
    /// When null, <see cref="Pack.AddEntity{TEntity}"/> may discover a
    /// <c>ClientEntity&lt;TEntity&gt;</c> (or nested <c>Client</c> type) in the same assembly.
    /// </summary>
    public virtual Type? ClientEntityType => null;
    
    /// <inheritdoc/>
    public virtual Trait[] DynamicTraits => [];
    
    /// <inheritdoc/>
    public virtual Dictionary<Identifier, object> Singles => new();

    /// <inheritdoc/>
    public static string Compile(Type tType)
    {
        if (typeof(JsonEntity).IsAssignableFrom(tType))
            return JsonEntity.Compile(tType);

        Entity inst = (Activator.CreateInstance(tType) as Entity)!;
        return CompileFromInstance(inst);
    }

    /// <inheritdoc/>
    public static string Compile<TConcreteType>() where TConcreteType : Entity, new() => Compile(typeof(TConcreteType));

    /// <summary>
    /// Compiles a pre-constructed instance of <see cref="Entity"/> to JSON.
    /// Useful for runtime configuration and deriving multiple objects from a single parent concrete type
    /// (e.g. having a <c>MasterStone</c> type and changing values at runtime to emit variants).
    /// </summary>
    /// <param name="inst">Instance to compile</param>
    /// <returns>Compiled JSON</returns>
    public static string CompileFromInstance(Entity inst)
    {
        if (inst is JsonEntity jsonEntity)
            return JsonEntity.CompileFromInstance(jsonEntity);

        Type tType = inst.GetType();
        
        CompilerState.Push(inst.Identifier.ToString());

        StringWriter sw = new();
        JsonWriter w = new JsonTextWriter(sw)
        {
            Formatting = Formatting.Indented, 
            Indentation = 4,
        };
        

        JsonHelper json = new(ref w);

        w.WriteStartObject();

        json.Property("format_version", inst.FormatVersion.ToString());
        json.Object("minecraft:entity", () =>
        {
            json.Object("description", () =>
            {
                json.Property("identifier", inst.Identifier);
                // empty "properties": {} is rejected by ("actor has no properties listed").
                // only emit the key when at least one entity property is defined.
                if (inst.Properties.Count > 0)
                {
                    json.Object("properties", () =>
                    {
                        foreach (var kvp in inst.Properties)
                            json.Property(kvp.Key.ToString(), kvp.Value);
                    });
                }
                json.Property("is_spawnable", inst.IsSpawnable);
                json.Property("is_summonable", inst.IsSummonable);
                json.Property("is_experimental", inst.IsExperimental);
                json.Property("runtime_identifier", inst.RuntimeIdentifier);
            });

            json.Object("component_groups", () =>
            {
                int c = 0;
                foreach (EntityComponentGroup ecg in inst.ComponentGroups)
                {
                    c++;
                    w.WritePropertyName(ecg.Identifier.ToString());
                    ecg.Compile(ref w);

                    CompilerState.Info($"({c}/{inst.ComponentGroups.Length}) compiled component group {ecg.Identifier}");
                }
            });

            json.Object("components", () =>
            {
                ITraitable.CompileTraits(inst, ref w, TraitSystem.TraitSystem.TraitType.Entity);
            });

            json.Object("events", () =>
            {
                CompilerState.Info("compiling events...");
                int c = 0;
                foreach (var kvp in inst.Events)
                {
                    c++;
                    json.Object(kvp.Key.ToString(), () =>
                    {
                        int i = 0;
                        foreach (IEntityEventAction e in kvp.Value)
                        {
                            i++;
                            e.Compile(ref w);

                            CompilerState.Info($"({i}/{kvp.Value.Length}) compiled event action {e.Name}");
                        }
                    });

                    CompilerState.Info($"({c}/{inst.Events.Count}) compiled event {kvp.Key}");
                }
            });
        });

        w.WriteEndObject();

        CompilerState.Pop();

        return sw.ToString();
    }
}