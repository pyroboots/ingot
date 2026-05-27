using Newtonsoft.Json;

namespace ingot.Common;

public interface ICompileableFragment
{
    public void Compile(ref JsonTextWriter writer);
}