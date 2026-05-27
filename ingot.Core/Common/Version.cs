using Newtonsoft.Json;

namespace ingot.Core.Common;

public class Version : ICompileableFragment
{
    public int Major;
    public int Minor;
    public int Patch;

    public Version(string version)
    {
        string[] parts = version.Split('.');
        Major = parts.Length > 0 ? int.Parse(parts[0]) : 0;
        Minor = parts.Length > 1 ? int.Parse(parts[1]) : 0;
        Patch = parts.Length > 2 ? int.Parse(parts[2]) : 0;
    }

    public Version(int major, int minor, int patch)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
    }
    public override string ToString() => $"{Major}.{Minor}.{Patch}";
    
    public void Compile(ref JsonTextWriter writer)
    {
        writer.WriteStartArray();
        writer.WriteValue(Major);
        writer.WriteValue(Minor);
        writer.WriteValue(Patch);
        writer.WriteEndArray();
    }
}