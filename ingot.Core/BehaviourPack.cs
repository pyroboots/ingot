using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Entity;
using ingot.Core.Behaviour.Item;
using ingot.Core.Behaviour.Loot;
using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;

using Newtonsoft.Json;

using Version = ingot.Core.Common.Version;

namespace ingot.Core;

/// <summary>
/// C# representation of a Minecraft behaviour pack
/// </summary>
public class BehaviourPack
{
    /// <summary>
    /// Minecraft UUID to be used at runtime
    /// </summary>
    public string Uuid;

    /// <summary>
    /// Version of the <see cref="BehaviourPack"/>. When <see cref="ResourcePack"/> is linked, it will require at least this version.
    /// </summary>
    public Version BehaviourPackVersion;

    /// <summary>
    /// Creates a <see cref="BehaviourPack"/> with the given runtime UUID and optional version.
    /// </summary>
    /// <param name="uuid">Minecraft UUID to be used at runtime</param>
    /// <param name="version">Version of the <see cref="BehaviourPack"/>. When <see cref="ResourcePack"/> is linked, it will require at least this version.</param>
    public BehaviourPack(string uuid, Version? version = null)
    {
        Uuid = uuid;
        BehaviourPackVersion = version ?? new Version(1, 0, 0);
    }
    /// <summary>
    /// Helper factory method to initiate API-style syntax
    /// </summary>
    /// <param name="uuid">Minecraft UUID to be used at runtime</param>
    /// <param name="version">Version of the <see cref="BehaviourPack"/>. When <see cref="ResourcePack"/> is linked, it will require at least this version.</param>
    public static BehaviourPack Create(string uuid, Version? version = null) => new(uuid, version);

    /// <summary>
    /// List of <see cref="Entity"/> types added to the pack
    /// </summary>
    public readonly List<Entity> Entities = new();
    /// <summary>
    /// List of <see cref="Block"/> types added to the pack
    /// </summary>
    public readonly List<Block> Blocks = new();
    /// <summary>
    /// List of <see cref="Item"/> types added to the pack
    /// </summary>
    public readonly List<Item> Items = new();
    /// <summary>
    /// List of <see cref="IRecipe"/> types added to the pack
    /// </summary>
    public readonly List<IRecipe> Recipes = new();
    /// <summary>
    /// List of <see cref="LootTable"/> types added to the pack
    /// </summary>
    public readonly List<LootTable> LootTables = new();
    /// <summary>
    /// Function paths executed every tick via <c>functions/tick.json</c>
    /// </summary>
    public List<string> TickFunctions = new();
    /// <summary>
    /// Dictionary of <c>.mcfunction</c>s by identifier and source file
    /// </summary>
    public Dictionary<string, string> Functions = new();
    
    /// <summary>
    /// Adds an entity to the pack
    /// </summary>
    /// <typeparam name="TEntity">Entity class to add</typeparam>
    public BehaviourPack AddEntity<TEntity>() where TEntity : Entity, new() =>
        AddEntity(typeof(TEntity));
    /// <summary>
    /// Adds an entity to the pack
    /// </summary>
    /// <param name="tEntity">Entity class to add</param>
    public BehaviourPack AddEntity(Type tEntity)
    {
        Entity inst = (Activator.CreateInstance(tEntity) as Entity)!;
        return AddEntityFromInstance(inst);
    }
    /// <summary>
    /// Adds an entity to the pack. Accepts both trait-based entities and <see cref="JsonEntity"/> instances.
    /// </summary>
    /// <param name="inst">Entity instance to add</param>
    public BehaviourPack AddEntityFromInstance(Entity inst)
    {
        Entities.Add(inst);
        return this;
    }

    /// <summary>
    /// Adds a block to the pack
    /// </summary>
    /// <typeparam name="TBlock">Block class to add</typeparam>
    public BehaviourPack AddBlock<TBlock>() where TBlock : Block, new() =>
        AddBlock(typeof(TBlock));
    /// <summary>
    /// Adds a block to the pack
    /// </summary>
    /// <param name="tBlock">Block class to add</param>
    public BehaviourPack AddBlock(Type tBlock)
    {
        Block inst = (Activator.CreateInstance(tBlock) as Block)!;
        return AddBlockFromInstance(inst);
    }
    /// <summary>
    /// Adds a block to the pack
    /// </summary>
    /// <param name="inst">Block instance to add</param>
    public BehaviourPack AddBlockFromInstance(Block inst)
    {
        Blocks.Add(inst);
        return this;
    }

    /// <summary>
    /// Adds an item to the pack
    /// </summary>
    /// <typeparam name="TItem">Item class to add</typeparam>
    public BehaviourPack AddItem<TItem>() where TItem : Item, new() =>
        AddItem(typeof(TItem));
    /// <summary>
    /// Adds an item to the pack
    /// </summary>
    /// <param name="tItem">Item class to add</param>
    public BehaviourPack AddItem(Type tItem)
    {
        Item inst = (Activator.CreateInstance(tItem) as Item)!;
        return AddItemFromInstance(inst);
    }
    /// <summary>
    /// Adds an item to the pack
    /// </summary>
    /// <param name="inst">Item instance to add</param>
    public BehaviourPack AddItemFromInstance(Item inst)
    {
        Items.Add(inst);
        return this;
    }

    /// <summary>
    /// Adds a recipe to the pack
    /// </summary>
    /// <typeparam name="TRecipe">Recipe class to add</typeparam>
    public BehaviourPack AddRecipe<TRecipe>() where TRecipe : IRecipe, new() =>
        AddRecipe(typeof(TRecipe));
    /// <summary>
    /// Adds a recipe to the pack
    /// </summary>
    /// <param name="tRecipe">Recipe class to add</param>
    public BehaviourPack AddRecipe(Type tRecipe)
    {
        IRecipe inst = (Activator.CreateInstance(tRecipe) as IRecipe)!;
        return AddRecipeFromInstance(inst);
    }
    /// <summary>
    /// Adds a recipe to the pack
    /// </summary>
    /// <param name="inst">Recipe instance to add</param>
    public BehaviourPack AddRecipeFromInstance(IRecipe inst)
    {
        Recipes.Add(inst);
        return this;
    }

    /// <summary>
    /// Adds a loot table to the pack
    /// </summary>
    /// <typeparam name="TLootTable">Loot table class to add</typeparam>
    public BehaviourPack AddLootTable<TLootTable>() where TLootTable : LootTable, new() =>
        AddLootTable(typeof(TLootTable));
    /// <summary>
    /// Adds a loot table to the pack
    /// </summary>
    /// <param name="tLootTable">Loot table class to add</param>
    public BehaviourPack AddLootTable(Type tLootTable)
    {
        if (LootTables.Any(t => t.GetType() == tLootTable))
            return this;

        LootTable inst = (Activator.CreateInstance(tLootTable) as LootTable)!;
        return AddLootTableFromInstance(inst);
    }
    /// <summary>
    /// Adds a loot table to the pack
    /// </summary>
    /// <param name="inst">Loot table instance to add</param>
    public BehaviourPack AddLootTableFromInstance(LootTable inst)
    {
        LootTables.Add(inst);
        return this;
    }
    
    /// <summary>
    /// Adds a function to the <see cref="BehaviourPack"/>
    /// </summary>
    /// <param name="identifier">The name of the function in game</param>
    /// <param name="sourceFile">The function source file</param>
    /// <param name="service">Whether to run this function every tick</param>
    public BehaviourPack AddFunction(string identifier, string sourceFile, bool service)
    {
        Functions.Add(identifier, sourceFile);
        if (service) TickFunctions.Add(identifier);
        return this;
    }

    /// <summary>
    /// Compiles the <see cref="BehaviourPack"/> to output <paramref name="dir"/>
    /// </summary>
    /// <param name="dir">Output directory</param>
    public void Compile(string dir)
    {
        CompilerState.Push("bp");

        #region dirs
        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(Path.Combine(dir, "entities"));
        Directory.CreateDirectory(Path.Combine(dir, "blocks"));
        Directory.CreateDirectory(Path.Combine(dir, "items"));
        Directory.CreateDirectory(Path.Combine(dir, "scripts"));
        Directory.CreateDirectory(Path.Combine(dir, "recipes"));
        Directory.CreateDirectory(Path.Combine(dir, "loot_tables"));
        Directory.CreateDirectory(Path.Combine(dir, "functions"));
        CompilerState.Info("created folder structure");
        #endregion

        #region identifiable compilation
        CompileIdentifiableCollection(Entities, dir, "entities", "entity", Entity.CompileFromInstance);
        CompileIdentifiableCollection(Blocks, dir, "blocks", "block", Block.CompileFromInstance);
        CompileIdentifiableCollection(Items, dir, "items", "item", Item.CompileFromInstance);
        CompileIdentifiableCollection(Recipes, dir, "recipes", "recipe", r => r.Compile());
        #endregion
        
        #region loot tables
        CompilerState.Info("compiling loot tables...");
        CompilerState.Push("loot_tables");
        int c = 0;
        foreach (LootTable lootTable in LootTables)
        {
            c++;
            string path = Path.Combine(dir, lootTable.Reference);
            Directory.CreateDirectory(path);

            string file = LootTable.CompileFromInstance(lootTable);
            File.WriteAllText(Path.Combine(path, $"{lootTable.Identifier.Name}.json"), file);

            CompilerState.Info($"({c}/{LootTables.Count}) compiled loot table {lootTable.Identifier} -> {Path.Combine(path, $"{lootTable.Identifier.Name}.json")}");
        }
        CompilerState.Pop();
        #endregion
        
        #region funcs
        CompilerState.Info("compiling functions...");
        CompilerState.Push("functions");
        c = 0;
        foreach (var kvp in Functions)
        {
            c++;
            File.Copy(kvp.Value, Path.Combine(dir, "functions",  $"{kvp.Key}.mcfunction"), true);

            CompilerState.Info($"({c}/{Functions.Count}) compiled function {kvp.Key}");
        }
        
        CompilerState.Push("tick.json");
        string json = JsonConvert.SerializeObject(new { values = TickFunctions }, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(Path.Combine(dir, "functions", "tick.json"), json + Environment.NewLine);
        CompilerState.Info($"wrote tick.json with {TickFunctions.Count} functions");
        CompilerState.Pop();
        CompilerState.Pop();
        #endregion

        CompilerState.Pop();
    }

    private static void CompileIdentifiableCollection<T>(
        IReadOnlyList<T> items,
        string dir,
        string subfolder,
        string itemLabel,
        Func<T, string> compile) where T : IIdentifiable
    {
        CompilerState.Info($"compiling {subfolder}...");
        CompilerState.Push(subfolder);
        int c = 0;
        foreach (T item in items)
        {
            c++;
            string path = Path.Combine(dir, subfolder, $"{item.Identifier.Name}.json");
            File.WriteAllText(path, compile(item));
            CompilerState.Info($"({c}/{items.Count}) compiled {itemLabel} {item.Identifier}");
        }

        CompilerState.Pop();
    }
}