using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits.Item;

namespace ingot.Test;

public class Lasagna : Item, IFood, IBlockPlacer
{
    public override string Identifier => "test:lasagna";
    public override string Texture => "lasagna";

    public int Nutrition => 100;
    public dynamic Block => "cobblestone";
}