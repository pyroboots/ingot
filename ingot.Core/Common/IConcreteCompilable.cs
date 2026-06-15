namespace ingot.Core.Common;

/// <summary>
/// Internal use interface to implement concrete type compiling 
/// </summary>
/// <typeparam name="TType">Type reference to the inheriting class</typeparam>
public interface IConcreteCompilable<TType> where TType : IConcreteCompilable<TType>
{
    /// <summary>
    /// Compiles the <paramref name="tType"/> to JSON as <see cref="TType"/>
    /// </summary>
    /// <param name="tType"></param>
    /// <returns>Compiled JSON</returns>
    static abstract string Compile(Type tType);
    
    /// <summary>
    /// Compiles the <see cref="TConcreteType"/> to JSON as <see cref="TType"/>
    /// </summary>
    /// <typeparam name="TConcreteType"></typeparam>
    /// <returns></returns>
    static string Compile<TConcreteType>() where TConcreteType : TType, new() => TType.Compile(typeof(TConcreteType));
}