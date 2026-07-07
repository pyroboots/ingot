using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Entity;

using Newtonsoft.Json;

using Formatting = ingot.Core.Common.Formatting;

namespace ingot.Core.Behaviour.Entity;

/// <summary>
/// Definition of an entity that can ride another entity with the <see cref="IAddrider"/> trait
/// </summary>
public class EntityRider : ICompilableFragment
{
    /// <summary>
    /// The entity type that will be riding
    /// </summary>
    public required Identifier EntityType;
    /// <summary>
    /// The spawn event that will be used when the riding entity is created
    /// </summary>
    public required Identifier SpawnEvent;

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Property(Formatting.PascalToSnakeCase(nameof(EntityType)), EntityType.ToString());
            json.Property(Formatting.PascalToSnakeCase(nameof(SpawnEvent)), SpawnEvent.ToString());
        });
    }
}

/// <summary>
/// Definition of an item that can be fed to an entity with the <see cref="IAgeable"/> trait
/// </summary>
public class EntityFeedItem : ICompilableFragment
{
    /// <summary>
    /// The item identifier
    /// </summary>
    public required Identifier Item;
    /// <summary>
    /// How much the entity ages when fed this item
    /// </summary>
    public required float Growth;
    
    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Property(Formatting.PascalToSnakeCase(nameof(Item)), Item.ToString());
            json.Property(Formatting.PascalToSnakeCase(nameof(Growth)), Growth);
        });
    }
}

/// <summary>
/// Sound event definition for the <see cref="IAmbientSoundInterval"/> trait
/// </summary>
public class EntitySoundEventName : ICompilableFragment
{
    /// <summary>
    /// The condition that must be satisfied to select the given ambient sound
    /// </summary>
    public required string Condition;
    /// <summary>
    /// Level sound event to be played as the ambient sound
    /// </summary>
    public required string EventName;
    
    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Property(Formatting.PascalToSnakeCase(nameof(Condition)), Condition);
            json.Property(Formatting.PascalToSnakeCase(nameof(EventName)), EventName);
        });
    }
}

/// <summary>
/// Conditionally triggers an entity event
/// </summary>
public class EntityEventTrigger : ICompilableFragment
{
    /// <summary>
    /// The condition that must be satisfied to select the given ambient sound
    /// </summary>
    public required EntityFilter? Filters;
    /// <summary>
    /// Level sound event to be played as the ambient sound
    /// </summary>
    public required string Event;
    
    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Property(Formatting.PascalToSnakeCase(nameof(Filters)), Filters);
            json.Property(Formatting.PascalToSnakeCase(nameof(Event)), Event);
        });
    }
}

/// <summary>
/// Represents an entry for blocks being broken for the <see cref="IBlockSensor"/> trait
/// </summary>
public class EntityBlockBreakEntry : ICompilableFragment
{
    /// <summary>
    /// List of blocks that trigger the on_block_broken event
    /// </summary>
    public required Identifier[] BlockList;
    /// <summary>
    /// Event to call when a block in block_list is broken
    /// </summary>
    public required Identifier OnBlockBroken;
    
    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Array(Formatting.PascalToSnakeCase(nameof(BlockList)), () =>
            {
                foreach (Identifier id in BlockList)
                    json.Writer.WriteValue(id.ToString());
            });
            json.Property(Formatting.PascalToSnakeCase(nameof(OnBlockBroken)), OnBlockBroken);
        });
    }
}