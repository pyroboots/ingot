using ingot.Core.Behaviour;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Item;

namespace ingot.Example.Items;

public class SauceItem : Item
{
    public override Identifier Identifier => new("test:spooky_special_sauce");
    public override string Texture => "blaze_powder";

    public override string DisplayName => "Spooky Special Sauce";
}