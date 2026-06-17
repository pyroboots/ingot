using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;

namespace ingot.Core.Behaviour.Loot;

/// <summary>
/// A loot table function that modifies a dropped item
/// </summary>
public abstract class LootFunction : ICompileableFragment
{
    /// <summary>
    /// Bedrock function name (e.g. <c>set_count</c>)
    /// </summary>
    public abstract string FunctionName { get; }

    /// <summary>
    /// Writes function-specific parameters
    /// </summary>
    protected abstract void CompileParameters(ref JsonTextWriter writer);

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);

        writer.WriteStartObject();
        json.Property("function", FunctionName);
        CompileParameters(ref writer);
        writer.WriteEndObject();
    }
}

/// <summary>
/// Sets the quantity of items returned
/// </summary>
public class SetCount : LootFunction
{
    /// <summary>
    /// Item count to return
    /// </summary>
    public required IntRange Count { get; init; }

    /// <inheritdoc/>
    public override string FunctionName => "set_count";

    /// <inheritdoc/>
    protected override void CompileParameters(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Property("count", Count);
    }
}

/// <summary>
/// Transforms a normal map into a treasure map that marks the location of hidden treasure
/// </summary>
public class ExplorationMap : LootFunction
{
    /// <summary>
    /// Enumeration of possible exploration map destinations
    /// </summary>
    public enum ExplorationMapDestination
    {
        /// <summary>Buried City</summary>
        BuriedCity,
        /// <summary>End City</summary>
        EndCity,
        /// <summary>Fortress</summary>
        Fortress,
        /// <summary>Mansion</summary>
        Mansion,
        /// <summary>Mineshaft</summary>
        Mineshaft,
        /// <summary>Monument</summary>
        Monument,
        /// <summary>PillagerOutpost</summary>
        PillagerOutpost,
        /// <summary>Ruins</summary>
        Ruins,
        /// <summary>Shipwreck</summary>
        Shipwreck,
        /// <summary>Stronghold</summary>
        Stronghold,
        /// <summary>Temple</summary>
        Temple,
        /// <summary>Village</summary>
        Village,
    }
    
    /// <summary>
    /// Destination to mark on the map
    /// </summary>
    public required ExplorationMapDestination Destination { get; init; }
    
    /// <inheritdoc/>
    public override string FunctionName => "exploration_map";

    /// <inheritdoc/>
    protected override void CompileParameters(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Property("destination", Enum.GetName(Destination)!.ToLower());
    }
}

/// <summary>
///  Modifies the count of how many items are returned when an entity is killed by an item with the looting enchantment
/// </summary>
public class LootingEnchant : LootFunction
{
    /// <summary>
    /// Bonus quantity to get upon death
    /// </summary>
    public required IntRange Count { get; init; }
    
    /// <inheritdoc/>
    public override string FunctionName => "looting_enchant";

    /// <inheritdoc/>
    protected override void CompileParameters(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Property("count", Count);
    }
}

/// <summary>
///  Lets you pick a random auxiliary value for an item
/// </summary>
public class RandomAuxiliaryValue : LootFunction
{
    /// <summary>
    /// Range of auxiliary values to pick
    /// </summary>
    public required IntRange Values { get; init; }
    
    /// <inheritdoc/>
    public override string FunctionName => "random_aux_value";

    /// <inheritdoc/>
    protected override void CompileParameters(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Property("values", Values);
    }
}

/// <summary>
///  Allows you to randomize the block state of the resulting item
/// </summary>
public class RandomBlockState : LootFunction
{
    /// <summary>
    /// Range of states to pick
    /// </summary>
    public required IntRange Values { get; init; }
    /// <summary>
    /// Block state to randomize
    /// </summary>
    public required string BlockState { get; init; }
    
    /// <inheritdoc/>
    public override string FunctionName => "random_block_state";

    /// <inheritdoc/>
    protected override void CompileParameters(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Property("values", Values);
        json.Property("blocK_state", BlockState);
    }
}

/// <summary>
///  Affects the colors of the random leather items supplied by a leather worker
/// </summary>
public class RandomDye : LootFunction
{
    /// <inheritdoc/>
    public override string FunctionName => "random_dye";

    // has no parameters, its a tag
    /// <inheritdoc/>
    protected override void CompileParameters(ref JsonTextWriter writer) { }
}

/// <summary>
///  Only works with a spawn egg and is used to set the entity ID of that spawn egg
/// </summary>
public class SetActorId : LootFunction
{
    /// <summary>
    /// <see cref="Identifier"/> of the entity to put in the spawn egg
    /// </summary>
    public required Identifier Identifier { get; init; }
    
    /// <inheritdoc/>
    public override string FunctionName => "set_actor_id";

    /// <inheritdoc/>
    protected override void CompileParameters(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Property("id", Identifier);
    }
}

/// <summary>
///  Sets the contents of a book
/// </summary>
public class SetBookContents : LootFunction
{
    /// <summary>
    /// Author of the book
    /// </summary>
    public required string Author { get; init; }
    
    /// <summary>
    /// Title of the book
    /// </summary>
    public required string Title { get; init; }
    
    /// <summary>
    /// String array representing the pages of the book
    /// </summary>
    public required string[] Pages { get; init; }
    
    /// <inheritdoc/>
    public override string FunctionName => "set_book_contents";

    /// <inheritdoc/>
    protected override void CompileParameters(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Property("author", Author);
        json.Property("title", Title);
        json.Property("pages", Pages);
    }
}

/// <summary>
///  Sets the percentage of durability remaining for items that have durability
/// </summary>
public class SetDamage : LootFunction
{
    /// <summary>
    /// Range of percentage damage to the durability
    /// </summary>
    public required IntRange Damage { get; init; }
    
    /// <inheritdoc/>
    public override string FunctionName => "set_damage";

    /// <inheritdoc/>
    protected override void CompileParameters(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Property("damage", Damage);
    }
}

/// <summary>
///  Defines the lore of an item
/// </summary>
public class SetLore : LootFunction
{
    /// <summary>
    /// Lines of lore on the item
    /// </summary>
    public required string[] Lore { get; init; }
    
    /// <inheritdoc/>
    public override string FunctionName => "set_lore";

    /// <inheritdoc/>
    protected override void CompileParameters(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Property("lore", Lore);
    }
}

/// <summary>
///  Sets the name of an item
/// </summary>
public class SetName : LootFunction
{
    /// <summary>
    /// Display name of the item
    /// </summary>
    public required string Name { get; init; }
    
    /// <inheritdoc/>
    public override string FunctionName => "set_name";

    /// <inheritdoc/>
    protected override void CompileParameters(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Property("name", Name);
    }
}