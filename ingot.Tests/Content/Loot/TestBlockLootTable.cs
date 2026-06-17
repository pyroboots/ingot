using ingot.Core.Behaviour.Loot;
using ingot.Core.Common;

namespace ingot.Tests.Content.Loot;

internal class TestBlockLootTable : LootTable
{
    public override Identifier Identifier => new("test", "loot_block");
    public override LootTableCategory Category => LootTableCategory.Blocks;

    public override LootPool[] Pools =>
    [
        new()
        {
            Rolls = new IntRange(1),
            Entries =
            [
                new ItemLootEntry(new Identifier("test", "test_item"))
                {
                    Weight = 1,
                    Functions = [new SetCount { Count = new IntRange(1, 2) }]
                }
            ]
        }
    ];
}