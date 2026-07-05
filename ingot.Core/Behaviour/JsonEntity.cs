using ingot.Core.Common;

namespace ingot.Core.Behaviour;

/// <summary>
/// An entity defined by pre-authored JSON rather than ingot traits.
/// </summary>
public abstract class JsonEntity : Entity
{
    /// <summary>
    /// Complete entity JSON to write to disk.
    /// </summary>
    protected abstract string Json { get; }

    /// <summary>
    /// Loads entity JSON from a file on disk.
    /// </summary>
    /// <param name="path">Path to the source entity JSON file.</param>
    protected static string LoadJson(string path) => File.ReadAllText(path);

    /// <summary>
    /// Compiles the <see cref="JsonEntity"/> (as <paramref name="tType"/>) to JSON
    /// </summary>
    /// <param name="tType">Concrete type of <see cref="JsonEntity"/></param>
    /// <returns>Compiled JSON</returns>
    public static new string Compile(Type tType)
    {
        JsonEntity inst = (Activator.CreateInstance(tType) as JsonEntity)!;

        CompilerState.Push(inst.Identifier.ToString());
        string json = inst.Json;
        CompilerState.Info("compiled raw entity json");
        CompilerState.Pop();

        return json;
    }
}