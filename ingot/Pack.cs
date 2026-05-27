namespace ingot;

public class Pack
{
    public string Name { get; }
    public string Description { get; }

    public BehaviourPack BehaviourPack = new();
    public ResourcePack ResourcePack = new();
    
    public Pack(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public void Compile()
    {
        BehaviourPack.Compile(Path.Combine(Directory.GetCurrentDirectory(), "bp"));
        ResourcePack.Compile(Path.Combine(Directory.GetCurrentDirectory(), "rp"));
    }
}