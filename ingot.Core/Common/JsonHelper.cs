using Newtonsoft.Json;

namespace ingot.Core.Common;

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
        if (value is string && ((string)value) == string.Empty) return;
        
        w.WritePropertyName(key);
        if (value is ICompileableFragment)
            ((ICompileableFragment)value).Compile(ref w);
        else
            JsonSerializer.CreateDefault().Serialize(w, value);
    }

    public static void Array(ref JsonTextWriter w, string key, Action<JsonTextWriter> items)
    {
        if (key != "")
            w.WritePropertyName(key);
        w.WriteStartArray();
        items(w);
        w.WriteEndArray();
    }
}