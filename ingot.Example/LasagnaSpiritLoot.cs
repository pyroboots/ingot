using ingot.Core.Behaviour.Loot;
using ingot.Core.Common;

namespace ingot.Example;

public class LasagnaSpiritLoot : LootTable
{
    public override Identifier Identifier => new("test", "lasagna_spirit");
    public override LootTableCategory Category => LootTableCategory.Entities;

    public override LootPool[] Pools =>
    [
        new()
        {
            Rolls = 1,
            Entries =
            [
                new ItemLootEntry(new Identifier("test", "lasagna"))
            ]
        }
    ];
}