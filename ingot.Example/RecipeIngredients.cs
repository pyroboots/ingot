using ingot.Core.Behaviour;
using ingot.Core.Common;

namespace ingot.Example;

public class CheeseItem : Item
{
    public override Identifier Identifier => new("test:cheese");
    public override string Texture => "cheese";
}

public class PastaItem : Item
{
    public override Identifier Identifier => new("test:pasta");
    public override string Texture => "pasta";
}

public class SauceItem : Item
{
    public override Identifier Identifier => new("test:spooky_special_sauce");
    public override string Texture => "spooky_special_sauce";
}