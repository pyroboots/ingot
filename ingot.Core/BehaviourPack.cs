using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Loot;
using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;
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
    public List<Entity> Entities = new();
    /// <summary>
    /// List of <see cref="Block"/> types added to the pack
    /// </summary>
    public List<Block> Blocks = new();
    /// <summary>
    /// List of <see cref="Item"/> types added to the pack
    /// </summary>
    public List<Item> Items = new();
    /// <summary>
    /// List of <see cref="IRecipe"/> types added to the pack
    /// </summary>
    public List<IRecipe> Recipes = new();
    /// <summary>
    /// List of <see cref="LootTable"/> types added to the pack
    /// </summary>
    public List<LootTable> LootTables = new();

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
        
        LootTables.Add(inst);
        return this;
    }

    /// <summary>
    /// Compiles the <see cref="BehaviourPack"/> to output <paramref name="dir"/>
    /// </summary>
    /// <param name="dir">Output directory</param>
    public void Compile(string dir)
    {
        CompilerState.Push("bp");
        
        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(Path.Combine(dir, "entities"));
        Directory.CreateDirectory(Path.Combine(dir, "blocks"));
        Directory.CreateDirectory(Path.Combine(dir, "items"));
        Directory.CreateDirectory(Path.Combine(dir, "scripts"));
        Directory.CreateDirectory(Path.Combine(dir, "recipes"));
        Directory.CreateDirectory(Path.Combine(dir, "loot_tables"));
        CompilerState.Info("created folder structure");
        
        CompilerState.Info("compiling entities...");
        CompilerState.Push("entities");
        int c = 0;
        foreach (Entity entity in Entities)
        {
            c++;
            string filename = entity.Identifier.Name;
            
            string path = Path.Combine(dir, "entities", $"{filename}.json");
            string file = Entity.Compile(entity.GetType());
            File.WriteAllText(path, file);
            
            CompilerState.Info($"({c}/{Entities.Count}) compiled entity {entity.Identifier}");
        }
        CompilerState.Pop();

        CompilerState.Info("compiling blocks...");
        CompilerState.Push("blocks");
        c = 0;
        foreach (Block block in Blocks)
        {
            c++;
            string filename = block.Identifier.Name;
            
            string path = Path.Combine(dir, "blocks", $"{filename}.json");
            string file = Block.Compile(block.GetType());
            File.WriteAllText(path, file);
            
            CompilerState.Info($"({c}/{Blocks.Count}) compiled block {block.Identifier}");
        }
        CompilerState.Pop();
        
        CompilerState.Info("compiling items...");
        CompilerState.Push("items");
        c = 0;
        foreach (Item item in Items)
        {
            c++;
            string filename = item.Identifier.Name;
            
            string path = Path.Combine(dir, "items", $"{filename}.json");
            string file = Item.Compile(item.GetType());
            File.WriteAllText(path, file);
            
            CompilerState.Info($"({c}/{Items.Count}) compiled item {item.Identifier}");
        }
        CompilerState.Pop();
        
        CompilerState.Info("compiling recipes...");
        CompilerState.Push("recipes");
        c = 0;
        foreach (IRecipe recipe in Recipes)
        {
            c++;
            string filename = recipe.Identifier.Name;
            
            string path = Path.Combine(dir, "recipes", $"{filename}.json");
            string file;
            
            if (recipe is ShapedRecipe)
                file = ShapedRecipe.Compile(recipe.GetType());
            else if (recipe is ShapelessRecipe)
                file = ShapelessRecipe.Compile(recipe.GetType());
            else if (recipe is FurnaceRecipe)
                file = FurnaceRecipe.Compile(recipe.GetType());
            else if (recipe is BrewingMixRecipe)
                file = BrewingMixRecipe.Compile(recipe.GetType());
            else throw new InvalidCastException("expected a recipe type");
            
            File.WriteAllText(path, file);
            
            CompilerState.Info($"({c}/{Recipes.Count}) compiled recipe {recipe.Identifier}");
        }
        CompilerState.Pop();

        CompilerState.Info("compiling loot tables...");
        CompilerState.Push("loot_tables");
        c = 0;
        foreach (LootTable lootTable in LootTables)
        {
            c++;
            string path = Path.Combine(dir, lootTable.Reference);
            Directory.CreateDirectory(path);
            
            string file = LootTable.Compile(lootTable.GetType());
            File.WriteAllText(Path.Combine(path, $"{lootTable.Identifier.Name}.json"), file);

            CompilerState.Info($"({c}/{LootTables.Count}) compiled loot table {lootTable.Identifier} -> {Path.Combine(path, $"{lootTable.Identifier.Name}.json")}");
        }
        CompilerState.Pop();
        
        CompilerState.Pop();
    }
}
