namespace ingot.Core;

public class ResourcePack
{
    public void Compile(string dir)
    {
        Directory.CreateDirectory(dir);
    }
}