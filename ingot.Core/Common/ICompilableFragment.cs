using Newtonsoft.Json;

namespace ingot.Core.Common;

/// <summary>
/// Internal use interface make a class compilable to JSON
/// </summary>
public interface ICompilableFragment
{
    /// <summary>
    /// Compiles this class to JSON
    /// </summary>
    /// <param name="writer">JSON source stream to write to</param>
    public void Compile(ref JsonWriter writer);
}

internal class CompilableFragmentJsonConverter<T> : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        ICompilableFragment frag = (value as ICompilableFragment)!;
        frag.Compile(ref writer);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => throw new InvalidOperationException();

    public override bool CanConvert(Type objectType) => objectType.IsAssignableTo(typeof(T));
}