using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;
using Formatting = Newtonsoft.Json.Formatting;
using Version = System.Version;

namespace ingot.Core.Behaviour;

public abstract class Entity
{
    public abstract Identifier Identifier { get; }
    public virtual Version FormatVersion => new("1.20.10");
    
    public static string Compile<TEntity>() where TEntity : Entity, new() => Compile(typeof(TEntity));
    public static string Compile(Type tEntity)
    {
        Entity inst = (Activator.CreateInstance(tEntity) as Entity)!;
        
        CompileTimeLogging.Push(inst.Identifier.ToString());

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

        CompileTimeLogging.Pop();

        return sw.ToString();
    }
}