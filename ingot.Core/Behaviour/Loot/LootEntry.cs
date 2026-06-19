using ingot.Core;
using ingot.Core.Common;

using Newtonsoft.Json;

using static ingot.Core.Common.JsonHelper;

namespace ingot.Core.Behaviour.Loot;

/// <summary>
/// A single entry within a loot pool
/// </summary>
public abstract class LootEntry : ICompilableFragment
{
    /// <summary>
    /// Relative chance for this entry to be selected. Defaults to 1.
    /// </summary>
    public int Weight = 1;
    /// <summary>
    /// Functions applied to the result when this entry is selected
    /// </summary>
    public LootFunction[] Functions = [];

    /// <summary>
    /// Bedrock entry type (e.g. <c>item</c>, <c>empty</c>)
    /// </summary>
    public abstract string EntryType { get; }

    /// <summary>
    /// Writes entry-type-specific properties
    /// </summary>
    protected abstract void CompileEntry(ref JsonTextWriter writer);

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        JsonTextWriter w = writer;

        if (Weight <= 0)
            CompilerState.Warn(ref w, "loot entry has non-positive weight");

        JsonHelper json = new(ref w);

        w.WriteStartObject();
        json.Property("type", EntryType);
        CompileEntry(ref w);
        if (Weight != 1)
            json.Property("weight", Weight);
        if (Functions.Length > 0)
        {
            json.Array("functions", () =>
            {
                foreach (LootFunction function in Functions)
                    function.Compile(ref w);
            });
        }
        w.WriteEndObject();
    }
}

/// <summary>
/// Drops a specific item
/// </summary>
public class ItemLootEntry : LootEntry
{
    /// <summary>
    /// Item to drop
    /// </summary>
    public Identifier Item;

    /// <summary>
    /// Creates an item loot entry
    /// </summary>
    public ItemLootEntry(Identifier item) => Item = item;

    /// <inheritdoc/>
    public override string EntryType => "item";

    /// <inheritdoc/>
    protected override void CompileEntry(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Property("name", Item.ToString());
    }
}

/// <summary>
/// Represents no loot on a roll
/// </summary>
public class EmptyLootEntry : LootEntry
{
    /// <inheritdoc/>
    public override string EntryType => "empty";

    /// <inheritdoc/>
    protected override void CompileEntry(ref JsonTextWriter writer) { }
}