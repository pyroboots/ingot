using ingot.Common;
using Version = ingot.Common.Version;

namespace ingot.Components;

public interface IComponent<T> : ICompileableFragment
{
    public Type ComponentType => typeof(T);
    
    public Identifier Identifier { get; }
    
    public Version MinimumFormatVersion { get; }
}