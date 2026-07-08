using Newtonsoft.Json;

namespace ingot.Core.Common;

internal class JsonHelper
{
    public static void Object(ref JsonTextWriter w, string key, Action<JsonTextWriter> content)
    {
        bool trace = !string.IsNullOrEmpty(key);
        if (trace)
            CompilerState.Push(key);

        try
        {
            if (key != "")
                w.WritePropertyName(key);
            w.WriteStartObject();
            content(w);
            w.WriteEndObject();
        }
        finally
        {
            if (trace)
                CompilerState.Pop();
        }
    }

    public static void Property(ref JsonTextWriter w, string key, object? value)
    {
        if (value is null) return;
        if (value is string && ((string)value) == string.Empty) return;

        w.WritePropertyName(key);
        if (value is ICompilableFragment)
            ((ICompilableFragment)value).Compile(ref w);
        else
            JsonSerializer.CreateDefault().Serialize(w, value);
    }

    public static void Array(ref JsonTextWriter w, string key, Action<JsonTextWriter> items)
    {
        bool trace = !string.IsNullOrEmpty(key);
        if (trace)
            CompilerState.Push(key);

        try
        {
            if (key != "")
                w.WritePropertyName(key);
            w.WriteStartArray();
            items(w);
            w.WriteEndArray();
        }
        finally
        {
            if (trace)
                CompilerState.Pop();
        }
    }

    public JsonTextWriter Writer;
    public JsonHelper(ref JsonTextWriter w) => Writer = w;
    public void Property(string key, object? value) => Property(ref Writer, key, value);
    public void Object(string key, Action content) => Object(ref Writer, key, (_writer) => content());
    public void Array(string key, Action content) => Array(ref Writer, key, (_writer) => content());
}
