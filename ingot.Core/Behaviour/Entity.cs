using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;
using Formatting = Newtonsoft.Json.Formatting;
using Version = System.Version;

namespace ingot.Core.Behaviour;

/// <summary>
/// Implements basic properties of an item
/// </summary>
public abstract class Entity : IConcreteCompilable<Entity>
{
    public abstract Identifier Identifier { get; }
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

        w.WriteStartObject();

        Property(ref w, "format_version", inst.FormatVersion.ToString());
        Object(ref w, "minecraft:entity", w =>
        {
            
        });
        
        w.WriteEndObject();

        CompilerState.Pop();

        return sw.ToString();
    }
}