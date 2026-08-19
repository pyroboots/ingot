using Newtonsoft.Json;

namespace ingot.Core.Common.SharedConstructs;

/// <summary>
/// References items with valid tags
/// </summary>
[JsonConverter(typeof(CompilableFragmentJsonConverter))]
public class ItemTagsDescriptor : ICompilableFragment
{
    /// <summary>
    /// Item must have any of these tags to qualify
    /// </summary>
    public Identifier[]? AnyTags = null;
    /// <summary>
    /// Item must have all of these tags to qualify
    /// </summary>
    public Identifier[]? AllTags = null;
    
    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        if (AnyTags is null && AllTags is null)
            throw new ArgumentException("at least one tag selector must not be null");
        if (AnyTags?.Length > 0 && AllTags?.Length > 0)
            throw new ArgumentException("tag selector cannot be empty");

        string molang = "";
        if (AnyTags is not null)
        {
            string[] tags = AnyTags.Select(x => $"'{x}'").ToArray();
            molang += $"q.any_tags({string.Join(',', tags)})";
        }

        if (AnyTags is not null && AllTags is not null) molang += " && ";

        if (AllTags is not null)
        {
            string[] tags = AllTags.Select(x => $"'{x}'").ToArray();
            molang += $"q.all_tags({string.Join(',', tags)})";
        }
        
        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Property("tags", molang);
        });
    }
}