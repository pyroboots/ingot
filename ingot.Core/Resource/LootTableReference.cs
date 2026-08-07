using ingot.Core.Behaviour.Loot;

namespace ingot.Core.Resource;

/// <summary>
/// Represents a loot table
/// </summary>
/// <typeparam name="TLootTable">Loot table to reference</typeparam>
public class LootTableReference<TLootTable> where TLootTable : LootTable, new()
{
    private readonly string _id;
    
    /// <summary>
    /// Implicitly registers and references a loot table
    /// </summary>
    /// <exception cref="InvalidOperationException">loot table registration only valid during pack compilation</exception>
    public LootTableReference()
    {
        Pack pack = CompilerState.CurrentPack 
                    ?? throw new InvalidOperationException("loot table registration only valid during pack compilation");
        
        _id = new TLootTable().Reference;
        if (pack.BehaviourPack.LootTables.All((loot) => loot.Reference != _id))
        {
            CompilerState.Info($"implicitly registered loot table {_id}");
            pack.BehaviourPack.AddLootTable<TLootTable>();
        }
    }

    /// <summary/>
    public static implicit operator string(LootTableReference<TLootTable> loot) => loot._id;
    /// <summary/>
    public static implicit operator LootTableReference(LootTableReference<TLootTable> loot) => new(typeof(TLootTable), loot._id);
}

/// <summary/>
public class LootTableReference(Type parent, string reference)
{
    /// <summary>
    /// Underlying type of the reference
    /// </summary>
    public Type Parent = parent;
    /// <summary>
    /// Implicit reference string of the asset
    /// </summary>
    public string Reference = reference;
}