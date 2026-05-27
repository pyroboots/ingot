using ingot.Core.Common;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Components;

public interface IComponent<T> : ICompileableFragment
{
    public Type ComponentType => typeof(T);
    
    public Identifier Identifier { get; }
    
    public Version MinimumFormatVersion { get; }
}