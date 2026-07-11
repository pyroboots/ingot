using System.Reflection;
using System.Reflection.Emit;

namespace ingot.Example.BricksGalore;

/// <summary>
/// Emits empty marker types used as generic type arguments for <see cref="BrickBlock{TToken}"/>
/// and <see cref="BrickRecipe{TToken}"/>.
/// </summary>
internal static class DynamicTypeFactory
{
    private static readonly AssemblyBuilder AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
        new AssemblyName("ingot.Example.BricksGalore.Generated"),
        AssemblyBuilderAccess.Run);

    private static readonly ModuleBuilder ModuleBuilder =
        AssemblyBuilder.DefineDynamicModule("Generated");

    private static int _counter;

    /// <summary>
    /// Creates a unique empty public class type with a parameterless constructor.
    /// </summary>
    public static Type CreateToken(string nameHint)
    {
        int id = Interlocked.Increment(ref _counter);
        string safe = Sanitize(nameHint);
        string typeName = $"Gen_{safe}_{id}";

        TypeBuilder tb = ModuleBuilder.DefineType(
            typeName,
            TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed);

        tb.DefineDefaultConstructor(MethodAttributes.Public);

        return tb.CreateType()
               ?? throw new InvalidOperationException($"failed to emit type {typeName}");
    }

    private static string Sanitize(string name)
    {
        Span<char> buffer = stackalloc char[Math.Min(name.Length, 64)];
        int n = 0;
        foreach (char c in name)
        {
            if (n >= buffer.Length)
                break;
            buffer[n++] = char.IsLetterOrDigit(c) ? c : '_';
        }

        return n == 0 ? "token" : new string(buffer[..n]);
    }
}
