using ingot.Common;
using Newtonsoft.Json;

namespace ingot;

internal class JsonHelper
{
    public static void Object(ref JsonTextWriter w, string key, Action<JsonTextWriter> content)
    {
        if (key != "")
            w.WritePropertyName(key);
        w.WriteStartObject();
        content(w);
        w.WriteEndObject();
    }

    public static void Property(ref JsonTextWriter w, string key, object? value)
    {
        if (value is null) return;
        
        w.WritePropertyName(key);
        if (value is ICompileableFragment)
            ((ICompileableFragment)value).Compile(ref w);
        else
            JsonSerializer.CreateDefault().Serialize(w, value);
    }

    public static void Array(ref JsonTextWriter w, string key, object?[]? values)
    {
        if (values is null) return;
        w.WritePropertyName(key);
        w.WriteStartArray();
        foreach (object? val in values)
            if (val is not null)
                JsonSerializer.CreateDefault().Serialize(w, val);
        w.WriteEndArray();
    }
}