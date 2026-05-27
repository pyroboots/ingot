using Newtonsoft.Json;

namespace ingot.Core.Common;

public interface ICompileableFragment
{
    public void Compile(ref JsonTextWriter writer);
}