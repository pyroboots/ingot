using ingot.Core.Common;
using ingot.Core.TraitSystem;

using Newtonsoft.Json;

using Formatting = Newtonsoft.Json.Formatting;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Behaviour.Entity;

/// <summary>
/// Implements basic properties of an entity
/// </summary>
public abstract class Entity : IConcreteCompilable<Entity>, IIdentifiable
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
    /// List of entity <c>event</c>s
    /// </summary>
    public virtual Dictionary<Identifier, IEntityEventAction[]> Events => new();

    /// <summary>
    /// Compiles the <see cref="Entity"/> (as <paramref name="tType"/>) to JSON
    /// </summary>
    /// <param name="tType">Concrete type of <see cref="Entity"/></param>
    /// <returns>Compiled JSON</returns>
    public static string Compile(Type tType)
    {
        if (typeof(JsonEntity).IsAssignableFrom(tType))
            return JsonEntity.Compile(tType);

        Entity inst = (Activator.CreateInstance(tType) as Entity)!;

        CompilerState.Push(inst.Identifier.ToString());

        StringWriter sw = new();
        JsonTextWriter w = new(sw);
        w.Formatting = Formatting.Indented;
        w.Indentation = 4;

        JsonHelper json = new(ref w);

        w.WriteStartObject();

        json.Property("format_version", inst.FormatVersion.ToString());
        json.Object("minecraft:entity", () =>
        {
            json.Object("description", () =>
            {
                json.Property("identifier", inst.Identifier);
                json.Property("is_spawnable", inst.IsSpawnable);
                json.Property("is_summonable", inst.IsSummonable);
                json.Property("is_experimental", inst.IsExperimental);
            });
            
            CompilerState.Push("component_groups");
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
            CompilerState.Pop();
            
            CompilerState.Push("components");
            json.Object("components", () =>
            {
                CompilerState.Info("compiling traits...");
                List<Trait> traits = TraitSystem.TraitSystem.GetTraits(tType, TraitSystem.TraitSystem.TraitType.Entity);
                int c = 0;
                foreach (Trait t in traits)
                {
                    c++;
                    t.Compile(ref w);
                    CompilerState.Info($"({c}/{traits.Count}) compiled trait {t.RootTrait.Name}");
                }
            });
            CompilerState.Pop();
            
            CompilerState.Push("events");
            json.Object("events", () =>
            {
                CompilerState.Info("compiling events...");
                int c = 0;
                foreach (var kvp in inst.Events)
                {
                    CompilerState.Push(kvp.Key.ToString());
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
                    CompilerState.Pop();
                    
                    CompilerState.Info($"({c}/{inst.Events.Count}) compiled event {kvp.Key}");
                }
            });
            CompilerState.Pop();
        });

        w.WriteEndObject();

        CompilerState.Pop();

        return sw.ToString();
    }
}