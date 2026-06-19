using ingot.Core;
using ingot.Core.Common;

using Newtonsoft.Json;

using static ingot.Core.Common.JsonHelper;

namespace ingot.Core.Behaviour.Loot;

/// <summary>
/// A pool of weighted loot entries rolled one or more times
/// </summary>
public class LootPool : ICompilableFragment
{
    /// <summary>
    /// How many times to roll this pool
    /// </summary>
    public IntRange Rolls { get; set; } = 1;
    /// <summary>
    /// Weighted entries to select from on each roll
    /// </summary>
    public required LootEntry[] Entries { get; init; }

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        JsonTextWriter w = writer;

        if (Entries.Length == 0)
            CompilerState.Warn(ref w, "loot pool has no entries");

        JsonHelper json = new(ref w);

        w.WriteStartObject();
        json.Property("rolls", Rolls);
        json.Array("entries", () =>
        {
            foreach (LootEntry entry in Entries)
                entry.Compile(ref w);
        });
        w.WriteEndObject();
    }
}