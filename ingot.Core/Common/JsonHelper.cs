using Newtonsoft.Json;

namespace ingot.Core.Common;

internal class JsonHelper
{
    public static void Object(ref JsonWriter w, string key, Action<JsonWriter> content)
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

    public static void Property(ref JsonWriter w, string key, object? value)
    {
        if (value is Either either)
            value = either.Value;

        if (value is null) return;
        if (value is string && ((string)value) == string.Empty) return;

        w.WritePropertyName(key);
        if (value is ICompilableFragment fragment)
            fragment.Compile(ref w);
        else if (value is ICompilableFragment[] fragments)
        {
            // overs Identifier[], BlockTypeDescriptor[], and other fragment arrays via covariance.
            w.WriteStartArray();
            foreach (ICompilableFragment item in fragments)
                item.Compile(ref w);
            w.WriteEndArray();
        }
        else
            JsonSerializer.CreateDefault().Serialize(w, value);
    }

    public static void Array(ref JsonWriter w, string key, Action<JsonWriter> items)
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

    public JsonWriter Writer;
    public JsonHelper(ref JsonWriter w) => Writer = w;
    public void Property(string key, object? value) => Property(ref Writer, key, value);
    public void Object(string key, Action content) => Object(ref Writer, key, (_writer) => content());
    public void Array(string key, Action content) => Array(ref Writer, key, (_writer) => content());
}
