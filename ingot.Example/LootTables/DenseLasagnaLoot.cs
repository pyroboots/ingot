using ingot.Core.Behaviour.Loot;
using ingot.Core.Common;

namespace ingot.Example;

public class DenseLasagnaLoot : LootTable
{
    public override Identifier Identifier => new("test", "block_of_dense_lasagna");
    public override LootTableCategory Category => LootTableCategory.Blocks;

    public override LootPool[] Pools =>
    [
        new()
        {
            Rolls = new IntRange(1, 2),
            Entries =
            [
                new ItemLootEntry(new Identifier("test", "lasagna"))
                {
                    Weight = 3,
                    Functions = [new SetCount { Count = new IntRange(1, 3) }]
                },
                new EmptyLootEntry { Weight = 1 }
            ]
        }
    ];
}