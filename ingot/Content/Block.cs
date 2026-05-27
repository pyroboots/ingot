using ingot.Common;
using Newtonsoft.Json;

namespace ingot.Content;

public class Block : Identifiable, ICompileableFragment
{
    public Block(string identifier) : base(identifier) {}
    public Block(Identifier identifier) : base(identifier) {}
    
    public void Compile(ref JsonTextWriter writer)
    {
        throw new NotImplementedException();
    }
}