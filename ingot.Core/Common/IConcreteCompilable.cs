namespace ingot.Core.Common;

/// <summary>
/// Internal use interface to implement concrete type compiling 
/// </summary>
/// <typeparam name="TType">Type reference to the inheriting class</typeparam>
public interface IConcreteCompilable<in TType> where TType : IConcreteCompilable<TType>
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
    static abstract string Compile<TConcreteType>() where TConcreteType : TType, new();

    /// <summary>
    /// Compiles a pre-constructed instance of <typeparamref name="TType"/> to JSON.
    /// Useful for runtime configuration and deriving multiple objects from a single parent concrete type (e.g. having a <c>MasterStone</c> type and changing the explosion resistance at runtime to create a new block)
    /// </summary>
    /// <param name="inst">Instance to compile</param>
    /// <returns>Compiled JSON</returns>
    static abstract string CompileFromInstance(TType inst);
}