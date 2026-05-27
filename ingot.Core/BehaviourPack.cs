using ingot.Core.Content;

namespace ingot.Core;

public class BehaviourPack
{
    public Dictionary<string, Entity> Entities = new();
    public Dictionary<string, Block> Blocks = new();
    public Dictionary<string, Item> Items = new();

    public Entity AddEntity(string identifier)
    {
        Entity entity = new(identifier);
        Entities.Add(identifier, entity);
        
        return entity;
    }
    
    public Block AddBlock(string identifier)
    {
        Block block = new(identifier);
        Blocks.Add(identifier, block);
        
        return block;
    }
    
    public Item AddItem(string identifier)
    {
        Item item = new(identifier);
        Items.Add(identifier, item);
        
        return item;
    }

    public void Compile(string dir)
    {
        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(Path.Combine(dir, "entities"));
        Directory.CreateDirectory(Path.Combine(dir, "blocks"));
        Directory.CreateDirectory(Path.Combine(dir, "items"));
        
        foreach (var kvp in Entities)
        {
            string filename = kvp.Key.Split(':')[1];
            Entity entity = kvp.Value;

            string path = Path.Combine(dir, "entities", $"{filename}.json");
            string file = entity.Compile();
            File.WriteAllText(path, file);
        }
    }
}