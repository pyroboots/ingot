using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ingot.Core.Common;

internal class OneOfJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        => serializer.Serialize(writer, value is Either oneOf ? oneOf.Value : null);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        Type[] validTypes = objectType.IsGenericType
            ? objectType.GetGenericArguments()
            : existingValue is Either existing
                ? existing.ValidTypes
                : throw new JsonSerializationException($"cannot deserialize non generic {nameof(Either)} without type arguments");

        JToken token = JToken.Load(reader);

        // string last! newtonsoft json coerces numbers/bools into strings, which would
        // screw up OneOf<int, string> or OneOf<bool, string> if string were first
        foreach (Type type in validTypes.OrderBy(t => t == typeof(string)))
        {
            try
            {
                object? parsed = token.ToObject(type, serializer);
                if (parsed is null)
                    continue;

                return objectType.IsGenericType
                    ? Activator.CreateInstance(objectType, parsed)
                    : new Either(parsed, validTypes);
            }
            catch (JsonException) { /* shh */}
        }

        throw new JsonSerializationException(
            $"cannot convert {token.Type} to any of the allowed types: {string.Join(' ', validTypes.Select(t => t.Name))}");
    }

    public override bool CanConvert(Type objectType) =>
        objectType == typeof(Either) || objectType.IsSubclassOf(typeof(Either));
}

/// <summary>
/// Represents a loose typed value that can be one of many types.
/// Useful for JSON conversion
/// </summary>
[JsonConverter(typeof(OneOfJsonConverter))]
public class Either
{
    /// <summary/>
    public Either(object value, params Type[] validTypes)
    {
        Type type = value.GetType();
        if (validTypes.All(t => type.IsAssignableTo(t) == false))
            throw new ArgumentException($"value must be one of the allowed types: {string.Join(' ', validTypes.Select(t => t.Name))}");

        ValidTypes = validTypes;
        Type = type;
        Value = value;
    }

    /// <summary>
    /// Contains all the valid types that <see cref="Value"/> must be one of
    /// </summary>
    public readonly Type[] ValidTypes;
    
    /// <summary>
    /// Chosen type of <see cref="Value"/>.
    /// Should be one of the allowed types
    /// </summary>
    public Type Type;
    
    /// <summary>
    /// The underlying value of the object
    /// </summary>
    public object Value
    {
        get => _value!;
        set
        {
            if (ValidTypes.Contains(value.GetType()) == false)
                throw new ArgumentException(
                    $"value must be one of the allowed types: {string.Join(' ', ValidTypes.Select(t => t.Name))}");

            _value = value;
        }
    }
    private object? _value;
}

/// <inheritdoc/>
public class Either<T1, T2> : Either
{
    /// <summary/>
    public Either(object value) : base(value, typeof(T1), typeof(T2)) {}

    /// <summary>
    /// Wraps a <typeparamref name="T1"/> value.
    /// </summary>
    public static implicit operator Either<T1, T2>(T1 value) => new(value!);

    /// <summary>
    /// Wraps a <typeparamref name="T2"/> value.
    /// </summary>
    public static implicit operator Either<T1, T2>(T2 value) => new(value!);
}

/// <inheritdoc/>
public class Either<T1, T2, T3> : Either
{
    /// <summary/>
    public Either(object value) : base(value, typeof(T1), typeof(T2), typeof(T3)) {}

    /// <summary>
    /// Wraps a <typeparamref name="T1"/> value.
    /// </summary>
    public static implicit operator Either<T1, T2, T3>(T1 value) => new(value!);

    /// <summary>
    /// Wraps a <typeparamref name="T2"/> value.
    /// </summary>
    public static implicit operator Either<T1, T2, T3>(T2 value) => new(value!);
    
    /// <summary>
    /// Wraps a <typeparamref name="T3"/> value.
    /// </summary>
    public static implicit operator Either<T1, T2, T3>(T3 value) => new(value!);
}

/// <inheritdoc/>
public class Either<T1, T2, T3, T4> : Either
{
    /// <summary/>
    public Either(object value) : base(value, typeof(T1), typeof(T2), typeof(T3), typeof(T4)) {}

    /// <summary>
    /// Wraps a <typeparamref name="T1"/> value.
    /// </summary>
    public static implicit operator Either<T1, T2, T3, T4>(T1 value) => new(value!);

    /// <summary>
    /// Wraps a <typeparamref name="T2"/> value.
    /// </summary>
    public static implicit operator Either<T1, T2, T3, T4>(T2 value) => new(value!);
    
    /// <summary>
    /// Wraps a <typeparamref name="T3"/> value.
    /// </summary>
    public static implicit operator Either<T1, T2, T3, T4>(T3 value) => new(value!);
    
    /// <summary>
    /// Wraps a <typeparamref name="T4"/> value.
    /// </summary>
    public static implicit operator Either<T1, T2, T3, T4>(T4 value) => new(value!);
}
