using ingot.Core;
using ingot.Core.Common;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using static ingot.Core.Common.JsonHelper;

namespace ingot.Core.Behaviour.Loot;

/// <summary>
/// Represents a Minecraft loot table
/// </summary>
public abstract class LootTable : IConcreteCompilable<LootTable>, IIdentifiable
{
    /// <inheritdoc/>
    public abstract Identifier Identifier { get; }

    /// <summary>
    /// Subfolder category for <see cref="LootTable"/>s
    /// </summary>
    public enum LootTableCategory
    {
        /// <summary>
        /// Place the compiled <see cref="LootTable"/> in <c>loot_tables/items</c>
        /// </summary>
        Items,
        /// <summary>
        /// Place the compiled <see cref="LootTable"/> in <c>loot_tables/blocks</c>
        /// </summary>
        Blocks,
        /// <summary>
        /// Place the compiled <see cref="LootTable"/> in <c>loot_tables/entities</c>
        /// </summary>
        Entities,
        /// <summary>
        /// Place the compiled <see cref="LootTable"/> in <c>loot_tables/</c>
        /// </summary>
        General,
    }
    /// <summary>
    /// Optional
    /// </summary>
    public virtual LootTableCategory Category => LootTableCategory.General;
    /// <summary>
    /// Pools rolled when this loot table is called
    /// </summary>
    public abstract LootPool[] Pools { get; }

    /// <summary>
    /// Compiles the <see cref="LootTable"/> (as <paramref name="tType"/>) to JSON
    /// </summary>
    /// <param name="tType">Concrete type of <see cref="LootTable"/></param>
    /// <returns>Compiled JSON</returns>
    public static string Compile(Type tType)
    {
        LootTable inst = (Activator.CreateInstance(tType) as LootTable)!;

        CompilerState.Push(inst.Identifier.ToString());

        if (inst.Pools.Length == 0)
            CompilerState.Warn(ref _dummyWriter, "loot table has no pools");

        StringWriter sw = new();
        JsonTextWriter w = new(sw);
        w.Formatting = Formatting.Indented;
        w.Indentation = 4;

        JsonHelper json = new(ref w);

        w.WriteStartObject();
        json.Array("pools", () =>
        {
            int c = 0;
            foreach (LootPool pool in inst.Pools)
            {
                c++;
                pool.Compile(ref w);
                CompilerState.Info($"({c}/{inst.Pools.Length}) compiled loot pool");
            }
        });
        w.WriteEndObject();

        CompilerState.Pop();
        return sw.ToString();
    }

    private Dictionary<LootTableCategory, string> _subdir = new()
    {
        [LootTableCategory.Items] = "items",
        [LootTableCategory.Blocks] = "blocks",
        [LootTableCategory.Entities] = "entities",
        [LootTableCategory.General] = "",
    };
    /// <summary>
    /// Relative directory of the <see cref="LootTable"/> to the root of the <see cref="Pack"/>
    /// </summary>
    public string Reference => Path.Combine("loot_tables", _subdir[Category]);
    /// <summary>
    /// Relative file path used by <c>minecraft:loot</c> components (e.g. <c>loot_tables/blocks/my_block.json</c>)
    /// </summary>
    public string RelativePath => Path.Combine(Reference, $"{Identifier.Name}.json");

    private static JsonTextWriter? _dummyWriter;
}