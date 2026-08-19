using ingot.Core.Common;

using Newtonsoft.Json;

namespace ingot.Core.Behaviour.Entity;

/// <summary>
/// C# representation of an action in an entity event
/// </summary>
public interface IEntityEventAction : ICompilableFragment
{
    /// <summary>
    /// Name of the entity event action
    /// </summary>
    public string Name { get; }
}

/// <summary>
/// Entity event action to remove an entity component group
/// </summary>
public class ComponentGroupRemoveEntityEventAction : IEntityEventAction
{
    /// <inheritdoc/>
    public string Name => "remove";

    /// <summary>
    /// Array of <see cref="EntityComponentGroup"/>s to remove.
    /// When empty or null, emits an empty <c>remove</c> object.
    /// </summary>
    public Identifier[] ComponentGroups = [];
    
    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        JsonHelper json = new(ref writer);
        
        json.Object(Name, () =>
        {
            if (ComponentGroups is { Length: > 0 })
                json.Property("component_groups", ComponentGroups.Select(i => i.ToString()).ToArray());
        });
    }
}

/// <summary>
/// Entity event action to add an entity component group
/// </summary>
public class ComponentGroupAddEntityEventAction : IEntityEventAction
{
    /// <inheritdoc/>
    public string Name => "add";

    /// <summary>
    /// Array of <see cref="EntityComponentGroup"/>s to add
    /// </summary>
    public required Identifier[] ComponentGroups;
    
    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        JsonHelper json = new(ref writer);
        
        json.Object(Name, () =>
        {
            json.Property("component_groups", ComponentGroups.Select(i => i.ToString()).ToArray());
        });
    }
}

/// <summary>
/// Entity event action to drop an item from an inventory slot
/// </summary>
public class DropItemEntityEventAction : IEntityEventAction
{
    /// <inheritdoc/>
    public string Name => "drop_item";
    
    /// <summary>
    /// Inventory slot to drop an item from
    /// </summary>
    public required Enums.InventorySlot Slot; 
    
    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        JsonHelper json = new(ref writer);
        
        json.Object(Name, () =>
        {
            json.Property("slot", Enums.InventorySlot_AsString(Slot));
        });
    }
}

/// <summary>
/// Entity event action to emit a particle
/// </summary>
public class EmitParticleEntityEventAction : IEntityEventAction
{
    /// <inheritdoc/>
    public string Name => "emit_particle";

    /// <summary>
    /// Particle ID to emit
    /// </summary>
    public required Identifier Particle;
    
    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        JsonHelper json = new(ref writer);
        
        json.Object(Name, () =>
        {
            json.Property("particle", Particle.ToString());
        });
    }
}

/// <summary>
/// Entity event action to emit a vibration
/// </summary>
public class EmitVibrationEntityEventAction : IEntityEventAction
{
    /// <inheritdoc/>
    public string Name => "emit_vibration";

    /// <summary>
    /// Enumeration of emittable vibration types
    /// </summary>
    public enum VibrationType
    {
        /// <summary>Equates to <c>shear</c></summary>
        Shear,
        /// <summary>Equates to <c>entity_act</c></summary>
        EntityAct,
        /// <summary>Equates to <c>entity_interact</c></summary>
        EntityInteract,
    }

    /// <summary>
    /// Type of vibration to emit
    /// </summary>
    public required VibrationType Type; 
    
    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        JsonHelper json = new(ref writer);
        
        json.Property("emit_vibration", Enums.AsString(Type));
    }
}

/// <summary>
/// Chooses a list of entity event actions to execute from a weighted pool
/// </summary>
public class RandomizeEntityEventAction : IEntityEventAction
{
    /// <inheritdoc/>
    public string Name => "randomize";

    /// <summary>
    /// Weighted pool of entity event actions
    /// </summary>
    /// <param name="Weight">Odds of choosing this array of <see cref="IEntityEventAction"/>s</param>
    /// <param name="Actions">Array of actions to execute</param>
    public record EventActionPool(float Weight, IEntityEventAction[] Actions);
    
    /// <summary>
    /// Array of weighted eventActions
    /// </summary>
    public required EventActionPool[] EventActions;
    
    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        JsonHelper json = new(ref writer);
        
        json.Array(Name, () =>
        {
            foreach (EventActionPool p in EventActions) json.Object("", () =>
            {
                json.Property("weight", p.Weight);
                foreach (IEntityEventAction a in p.Actions)
                    a.Compile(ref json.Writer);
            });
        });
    }
}

/// <summary>
/// Ordered execution sequence of entity event actions
/// </summary>
public class SequenceEntityEventAction : IEntityEventAction
{
    /// <inheritdoc/>
    public string Name => "sequence";
    
    /// <summary>
    /// Sequence to execute
    /// </summary>
    public required IEntityEventAction[] EventActions;
    
    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        JsonHelper json = new(ref writer);
        
        json.Array(Name, () =>
        {
            foreach (IEntityEventAction a in EventActions) 
                json.Object("", () => a.Compile(ref json.Writer));
        });
    }
}

/// <summary>
/// Triggers another entity event (string form or object with event/target).
/// </summary>
public class TriggerEntityEventAction : IEntityEventAction
{
    /// <inheritdoc/>
    public string Name => "trigger";

    /// <summary>
    /// Event identifier to trigger (e.g. <c>minecraft:spawn_adult</c>).
    /// </summary>
    public required string Event;

    /// <summary>
    /// Optional target. When set, emits <c>{ "event": "...", "target": "..." }</c> instead of a bare string.
    /// </summary>
    public Enums.Target? Target;

    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        JsonHelper json = new(ref writer);

        if (Target is null)
        {
            json.Property(Name, Event);
            return;
        }

        json.Object(Name, () =>
        {
            json.Property("event", Event);
            json.Property("target", Enums.Target_AsString(Target.Value));
        });
    }
}

/// <summary>
/// Runs a command
/// </summary>
public class QueueCommandEntityEventAction : IEntityEventAction
{
    /// <inheritdoc/>
    public string Name => "queue_command";
    
    /// <summary>
    /// Commands to execute - excluding the <c>/</c> prefix
    /// </summary>
    public required string[] Commands;

    /// <summary>
    /// Who the command's instigator is
    /// </summary>
    public Enums.Target Target = Enums.Target.Self;
    
    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        JsonHelper json = new(ref writer);
        
        json.Object(Name, () =>
        {
            json.Property("target", Enums.Target_AsString(Target));
            json.Property("command", Commands);
        });
    }
}