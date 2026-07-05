using ingot.Core.Behaviour;
using ingot.Core.Common;

namespace ingot.Example.Items;

public class CheeseItem : Item
{
    public override Identifier Identifier => new("test:cheese");
    public override string Texture => "cheese";

    public override string DisplayName => "Cheese";
}