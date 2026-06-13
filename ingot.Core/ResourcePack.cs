namespace ingot.Core;
using Version = Common.Version;

/// <summary>
/// C# representation of a Minecraft resource pack
/// </summary>
public class ResourcePack
{
    /// <summary>
    /// Minecraft UUID to be used at runtime
    /// </summary>
    public string Uuid;
    /// <summary>
    /// Version of the <see cref="ResourcePack"/>. When <see cref="BehaviourPack"/> is linked, it will require at least this version.
    /// </summary>
    public Version ResourcePackVersion;
    /// <summary>
    /// Helper factory method to initiate API-style syntax
    /// </summary>
    /// <param name="uuid">Minecraft UUID to be used at runtime</param>
    /// <param name="version">Version of the <see cref="BehaviourPack"/>. When <see cref="ResourcePack"/> is linked, it will require at least this version.</param>
    public static ResourcePack Create(string uuid, Version? version = null) => new(uuid, version);
    public ResourcePack(string uuid, Version? version = null)
    {
        Uuid = uuid;
        ResourcePackVersion = version ?? new Version(1, 0, 0);
    }
    
    /// <summary>
    /// Compiles the <see cref="ResourcePack"/> to output <paramref name="dir"/>
    /// </summary>
    /// <param name="dir">Output directory</param>
    public void Compile(string dir)
    {
        Directory.CreateDirectory(dir);
    }
}