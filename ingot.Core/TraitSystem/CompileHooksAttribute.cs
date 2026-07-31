using ingot.Core.Common;

namespace ingot.Core.TraitSystem;

/// <summary>
/// Sets hooks to fire before and after the compilation of this type
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CompileHooksAttribute : Attribute
{
    /// <summary>
    /// Type containing the precompile and post-compile hook methods. Must be derived from <see cref="ICompileHooks"/>
    /// </summary>
    public readonly Type HookType;
    
    /// <summary>
    /// Sets hooks to fire before and after the compilation of this type
    /// </summary>
    /// <param name="hook">Type containing the precompile and post-compile hook methods. Must be derived from <see cref="ICompileHooks"/></param>
    public CompileHooksAttribute(Type hook) => HookType = hook;
}

/// <summary>
/// Type containing the precompile and post-compile hook methods
/// </summary>
public interface ICompileHooks
{
    /// <summary>
    /// Fired before the type is compiled
    /// </summary>
    /// <param name="inst">Instance of the to-be-compiled type</param>
    public void PreCompile(object inst);
    
    /// <summary>
    /// Fired after the type is compiled. Can modify outputted JSON via return
    /// </summary>
    /// <param name="json">JSON of the compiled type</param>
    /// <returns>JSON to write, null to return compiled</returns>
    public string? PostCompile(string json);
}