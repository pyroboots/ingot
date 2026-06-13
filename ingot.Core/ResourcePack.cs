namespace ingot.Core;
using Version = Common.Version;

public class ResourcePack
{
    public string Uuid;
    public Version ResourcePackVersion;
    public static ResourcePack Create(string uuid, Version? version = null) => new(uuid, version);
    public ResourcePack(string uuid, Version? version = null)
    {
        Uuid = uuid;
        ResourcePackVersion = version ?? new Version(1, 0, 0);
    }
    
    public void Compile(string dir)
    {
        Directory.CreateDirectory(dir);
    }
}