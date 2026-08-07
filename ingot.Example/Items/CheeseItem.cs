using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.Resource;

namespace ingot.Example.Items;

public class CheeseItem : Item
{
    public override Identifier Identifier => new("test:cheese");
    public override string Texture => new TextureReference<CheeseItem>(Path.Combine(AppContext.BaseDirectory, "Data", "cheese.png"));

    public override string DisplayName => "Cheese";
}