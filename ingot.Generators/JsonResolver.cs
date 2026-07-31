using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ingot.Generators;

/// <summary>
/// Resolves JSON schema <c>$ref</c> objects
/// </summary>
public class JsonResolver
{
    private readonly JObject? _definitions;
    private readonly HashSet<string> _resolving = new(StringComparer.Ordinal);

    private JsonResolver(JObject? definitions)
    {
        _definitions = definitions;
    }

    /// <summary>
    /// Parses <paramref name="json"/>, resolves all local <c>$ref</c> values, and returns the
    /// expanded document as indented JSON
    /// </summary>
    public static string Resolve(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        JToken root = JToken.Parse(json);
        JToken resolved = Resolve(root);
        return resolved.ToString(Formatting.Indented);
    }

    /// <summary>
    /// Deep-clones <paramref name="token"/> with all local <c>$ref</c> values expanded.
    /// The root <c>definitions</c> property is omitted from the result when present.
    /// </summary>
    public static JToken Resolve(JToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        JObject? definitions = token is JObject obj
            ? obj["definitions"] as JObject
            : null;

        JsonResolver resolver = new(definitions);
        JToken resolved = resolver.ResolveToken(token.DeepClone());

        if (resolved is JObject resolvedObj)
            resolvedObj.Remove("definitions");

        return resolved;
    }

    private JToken ResolveToken(JToken token)
    {
        return token.Type switch
        {
            JTokenType.Object => ResolveObject((JObject)token),
            JTokenType.Array => ResolveArray((JArray)token),
            _ => token,
        };
    }

    private JToken ResolveObject(JObject obj)
    {
        JToken? refToken = obj["$ref"];
        if (refToken is { Type: JTokenType.String } && refToken.Value<string>() is { } reference)
            return ResolveRefObject(obj, reference);

        JObject result = new();
        foreach (JProperty property in obj.Properties())
        {
            // definitions are kept only as a lookup table while resolving
            if (property.Name == "definitions")
                continue;

            result[property.Name] = ResolveToken(property.Value.DeepClone());
        }

        return result;
    }

    private JToken ResolveRefObject(JObject refObject, string reference)
    {
        if (!_resolving.Add(reference))
            throw new InvalidOperationException($"Circular $ref detected: '{reference}'.");

        try
        {
            JToken target = ResolvePointer(reference).DeepClone();
            JToken resolvedTarget = ResolveToken(target);

            // just a ref = return expanded as-is
            if (refObject.Count == 1)
                return resolvedTarget;

            if (resolvedTarget is not JObject targetObject)
            {
                // non object targets cannot merge sibling keywords only pure refs can
                throw new InvalidOperationException(
                    $"cannot merge sibling properties onto non-object $ref target '{reference}'.");
            }

            // definition first, then siblings
            JObject merged = (JObject)targetObject.DeepClone();
            foreach (JProperty property in refObject.Properties())
            {
                if (property.Name == "$ref")
                    continue;

                merged[property.Name] = ResolveToken(property.Value.DeepClone());
            }

            return merged;
        }
        finally
        {
            _resolving.Remove(reference);
        }
    }

    private JArray ResolveArray(JArray array)
    {
        JArray result = new();
        foreach (JToken item in array)
            result.Add(ResolveToken(item.DeepClone()));
        return result;
    }

    private JToken ResolvePointer(string reference)
    {
        if (!reference.StartsWith('#') && !reference.StartsWith("#/", StringComparison.Ordinal))
            throw new NotSupportedException(
                $"Only local JSON Pointer $ref values are supported (got '{reference}').");

        // "#" "#/" "#/definitions/123"
        string pointer = reference.Length > 1 && reference[1] == '/'
            ? reference[1..]
            : reference == "#"
                ? ""
                : throw new InvalidOperationException($"Invalid JSON Pointer $ref: '{reference}'.");

        if (pointer.Length == 0)
            throw new InvalidOperationException("Root document $ref is not supported after definitions are isolated.");

        string[] segments = pointer.Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < segments.Length; i++)
            segments[i] = UnescapePointerSegment(segments[i]);
        
        if (segments is ["definitions", { } id] && _definitions is not null)
        {
            JToken? def = _definitions[id];
            if (def is null)
                throw new InvalidOperationException($"Unresolved $ref: '{reference}' (definition '{id}' not found).");
            return def;
        }
        
        JToken? current = null;
        if (segments.Length > 0 && segments[0] == "definitions")
        {
            current = _definitions;
            for (int i = 1; i < segments.Length && current is not null; i++)
                current = Step(current, segments[i]);
        }

        if (current is null)
            throw new InvalidOperationException($"Unresolved $ref: '{reference}'.");

        return current;
    }

    private static JToken? Step(JToken current, string segment)
    {
        return current switch
        {
            JObject o => o[segment],
            JArray a when int.TryParse(segment, out int index) && index >= 0 && index < a.Count => a[index],
            _ => null,
        };
    }

    private static string UnescapePointerSegment(string segment) =>
        segment.Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal);
}
