using ingot.Core;
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.Common.SharedConstructs;
using ingot.Core.Resource;
using ingot.Core.TraitSystem.Traits.Item;

namespace ingot.Example.Items;

public class CheeseItem : Item, IRepairable
{
    public override Identifier Identifier => new("test:cheese");
    public override string Texture => new TextureReference<CheeseItem>(Path.Combine(AppContext.BaseDirectory, "Data", "cheese.png"));

    public override string DisplayName => "Cheese";

    public IRepairable.RepairItem[] RepairItems =>
    [
        new()
        {
            Items = [
                new Either<Identifier, ItemTagsDescriptor>(new Identifier("minecraft:diamond")),
                new Either<Identifier, ItemTagsDescriptor>(new ItemTagsDescriptor()
                {
                    AnyTags = [new("minecraft:planks")]
                })
            ],
            RepairAmount = new Either<int, Molang>(1)
        }
    ];
}