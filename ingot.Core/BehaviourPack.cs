using ingot.Core;
using ingot.Core.Content;
using ingot.Core.Content.Block;
using ingot.Core.TraitSystem;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;
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
    /// Adds an entity to the pack
    /// </summary>
    /// <typeparam name="TEntity">Entity class to add</typeparam>
    public BehaviourPack AddEntity<TEntity>() where TEntity : Entity, new()
    {
        Entities.Add(new TEntity());
        return this;
    }

    /// <summary>
    /// Adds a block to the pack
    /// </summary>
    /// <typeparam name="TBlock">Block class to add</typeparam>
    public BehaviourPack AddBlock<TBlock>() where TBlock : Block, new()
    {
        Blocks.Add(new TBlock());
        return this;
    }

    /// <summary>
    /// Adds an item to the pack
    /// </summary>
    /// <typeparam name="TItem">Item class to add</typeparam>
    public BehaviourPack AddItem<TItem>() where TItem : Item, new()
    {
        Items.Add(new TItem());
        return this;
    }

    /// <summary>
    /// Compiles the <see cref="BehaviourPack"/> to output <paramref name="dir"/>
    /// </summary>
    /// <param name="dir">Output directory</param>
    public void Compile(string dir)
    {
        CompileTimeLogging.Push("bp");
        
        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(Path.Combine(dir, "entities"));
        Directory.CreateDirectory(Path.Combine(dir, "blocks"));
        Directory.CreateDirectory(Path.Combine(dir, "items"));
        Directory.CreateDirectory(Path.Combine(dir, "scripts"));
        CompileTimeLogging.Info("created folder structure");
        
        CompileTimeLogging.Info("compiling entities...");
        CompileTimeLogging.Push("entities");
        int c = 0;
        foreach (Entity entity in Entities)
        {
            c++;
            string filename = entity.Identifier.Split(":")[1];
            
            string path = Path.Combine(dir, "entities", $"{filename}.json");
            string file = Entity.Compile(entity.GetType());
            File.WriteAllText(path, file);
            
            CompileTimeLogging.Info($"({c}/{Entities.Count}) compiled entity {entity.Identifier}");
        }
        CompileTimeLogging.Pop();

        CompileTimeLogging.Info("compiling blocks...");
        CompileTimeLogging.Push("blocks");
        c = 0;
        foreach (Block block in Blocks)
        {
            c++;
            string filename = block.Identifier.Split(":")[1];
            
            string path = Path.Combine(dir, "blocks", $"{filename}.json");
            string file = Block.Compile(block.GetType());
            File.WriteAllText(path, file);
            
            CompileTimeLogging.Info($"({c}/{Blocks.Count}) compiled entity {block.Identifier}");
        }
        CompileTimeLogging.Pop();
        
        CompileTimeLogging.Info("compiling blocks...");
        CompileTimeLogging.Push("items");
        c = 0;
        foreach (Item item in Items)
        {
            c++;
            string filename = item.Identifier.Split(":")[1];
            
            string path = Path.Combine(dir, "items", $"{filename}.json");
            string file = Item.Compile(item.GetType());
            File.WriteAllText(path, file);
            
            CompileTimeLogging.Info($"({c}/{Items.Count}) compiled entity {item.Identifier}");
        }
        CompileTimeLogging.Pop();
        CompileTimeLogging.Pop();
    }
}
