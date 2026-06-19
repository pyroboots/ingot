using ingot.Core.Common;

using Newtonsoft.Json;

using static ingot.Core.Common.JsonHelper;

using Formatting = Newtonsoft.Json.Formatting;
using Version = System.Version;

namespace ingot.Core.Behaviour;

/// <summary>
/// Implements basic properties of an item
/// </summary>
public abstract class Entity : IConcreteCompilable<Entity>, IIdentifiable
{
    /// <inheritdoc/>
    public abstract Identifier Identifier { get; }
    /// <summary>
    /// Minimum component version written to <c>format_version</c> in the generated entity JSON.
    /// </summary>
    public virtual Version FormatVersion => new("1.20.10");

    /// <summary>
    /// Compiles the <see cref="Entity"/> (as <paramref name="tType"/>) to JSON
    /// </summary>
    /// <param name="tType">Concrete type of <see cref="Entity"/></param>
    /// <returns>Compiled JSON</returns>
    public static string Compile(Type tType)
    {
        Entity inst = (Activator.CreateInstance(tType) as Entity)!;

        CompilerState.Push(inst.Identifier.ToString());

        StringWriter sw = new();
        JsonTextWriter w = new(sw);
        w.Formatting = Formatting.Indented;
        w.Indentation = 4;

        JsonHelper json = new(ref w);

        w.WriteStartObject();

        json.Property("format_version", inst.FormatVersion.ToString());
        json.Object("minecraft:entity", () =>
        {

        });

        w.WriteEndObject();

        CompilerState.Pop();

        return sw.ToString();
    }
}