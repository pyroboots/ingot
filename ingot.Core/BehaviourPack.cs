using ingot.Core;
using ingot.Core.Content;
using ingot.Core.Content.Block;
using ingot.Core.TraitSystem;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;
using Version = ingot.Core.Common.Version;

namespace ingot.Core;

public class BehaviourPack
{
    public string Uuid;
    public Version BehaviourPackVersion;
    public BehaviourPack(string uuid, Version? version = null)
    {
        Uuid = uuid;
        BehaviourPackVersion = version ?? new Version(1, 0, 0);
    }
    public static BehaviourPack Create(string uuid, Version? version = null) => new(uuid, version);
    
    public List<Entity> Entities = new();
    public List<Block> Blocks = new();
    public List<Item> Items = new();

    public BehaviourPack AddEntity<TEntity>() where TEntity : Entity, new()
    {
        Entities.Add(new TEntity());
        return this;
    }

    public BehaviourPack AddBlock<TBlock>() where TBlock : Block, new()
    {
        Blocks.Add(new TBlock());
        return this;
    }

    public BehaviourPack AddItem<TItem>() where TItem : Item, new()
    {
        Items.Add(new TItem());
        return this;
    }

    public void Compile(string dir)
    {
        CompileTimeLogging.Push("bp");
        
        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(Path.Combine(dir, "entities"));
        Directory.CreateDirectory(Path.Combine(dir, "blocks"));
        Directory.CreateDirectory(Path.Combine(dir, "items"));
        Directory.CreateDirectory(Path.Combine(dir, "scripts"));
        CompileTimeLogging.Log("created folder structure");
        
        CompileTimeLogging.Log("compiling entities...");
        CompileTimeLogging.Push("entities");
        int c = 0;
        foreach (Entity entity in Entities)
        {
            c++;
            string filename = entity.Identifier.Split(":")[1];
            
            string path = Path.Combine(dir, "entities", $"{filename}.json");
            string file = Entity.Compile(entity.GetType());
            File.WriteAllText(path, file);
            
            CompileTimeLogging.Log($"({c}/{Entities.Count}) compiled entity {entity.Identifier}");
        }
        CompileTimeLogging.Pop();

        CompileTimeLogging.Log("compiling blocks...");
        CompileTimeLogging.Push("blocks");
        c = 0;
        foreach (Block block in Blocks)
        {
            c++;
            string filename = block.Identifier.Split(":")[1];
            
            string path = Path.Combine(dir, "blocks", $"{filename}.json");
            string file = Block.Compile(block.GetType());
            File.WriteAllText(path, file);
            
            CompileTimeLogging.Log($"({c}/{Blocks.Count}) compiled entity {block.Identifier}");
        }
        CompileTimeLogging.Pop();
        
        CompileTimeLogging.Log("compiling blocks...");
        CompileTimeLogging.Push("items");
        c = 0;
        foreach (Item item in Items)
        {
            c++;
            string filename = item.Identifier.Split(":")[1];
            
            string path = Path.Combine(dir, "items", $"{filename}.json");
            string file = Item.Compile(item.GetType());
            File.WriteAllText(path, file);
            
            CompileTimeLogging.Log($"({c}/{Items.Count}) compiled entity {item.Identifier}");
        }
        CompileTimeLogging.Pop();
        CompileTimeLogging.Pop();
    }
}
