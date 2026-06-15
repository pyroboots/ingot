using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;

namespace ingot.Core.Behaviour.Recipe;

/// <summary>
/// Represents an ingredient or output item in a crafting recipe
/// </summary>
public record RecipeItem : ICompileableFragment
{
    /// <summary>
    /// Identifier of the item
    /// </summary>
    public required Identifier Item;
    /// <summary>
    /// Amount of <see cref="Item"/> required
    /// </summary>
    public int Count = 1;
    /// <summary>
    /// An item tag that matches multiple items
    /// </summary>
    public string? Tag = null;

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        writer.WriteStartObject();
        Property(ref writer, "item", Item.ToString());
        Property(ref writer, "count", Count);
        Property(ref writer, "tag", Tag);
        writer.WriteEndObject();
    }
}