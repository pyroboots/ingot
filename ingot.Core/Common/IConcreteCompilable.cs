namespace ingot.Core.Common;

/// <summary>
/// Internal use interface to implement concrete type compiling 
/// </summary>
/// <typeparam name="TType">Type reference to the inheriting class</typeparam>
public interface IConcreteCompilable<TType> where TType : IConcreteCompilable<TType>
{
    /// <summary>
    /// Compiles the concrete type <paramref name="tType"/> to JSON.
    /// </summary>
    /// <param name="tType">Concrete type to compile.</param>
    /// <returns>Compiled JSON</returns>
    static abstract string Compile(Type tType);
    
    /// <summary>
    /// Compiles <typeparamref name="TConcreteType"/> to JSON.
    /// </summary>
    /// <typeparam name="TConcreteType">Concrete type to compile.</typeparam>
    /// <returns>Compiled JSON</returns>
    static string Compile<TConcreteType>() where TConcreteType : TType, new() => TType.Compile(typeof(TConcreteType));
}