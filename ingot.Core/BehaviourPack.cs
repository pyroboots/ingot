using ingot.Core;
using ingot.Core.TraitSystem;

namespace ingot.Core;

public class BehaviourPack
{
    public Dictionary<string, Entity> Entities = new();
    public Dictionary<string, Block> Blocks = new();
    public Dictionary<string, Item> Items = new();

    public void AddEntity<TEntity>() where TEntity : Entity, new()
    {
        Entity inst = new TEntity();
        Entities.Add(inst.Identifier, inst);
    }
    
    public void AddBlock<TBlock>() where  TBlock : Block, new()
    {
        Block inst = new TBlock();
        Blocks.Add(inst.Identifier, inst);
    }
    
    public void AddItem<TItem>() where  TItem : Item, new()
    {
        Item inst = new TItem();
        Items.Add(inst.Identifier, inst);
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
            string file = Entity.Compile(entity.GetType());
            File.WriteAllText(path, file);
        }
    }
}
