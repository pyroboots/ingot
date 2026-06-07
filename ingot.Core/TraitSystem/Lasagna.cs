namespace ingot.Core.TraitSystem;

public class Lasagna : Item, IFood, IBlockPlacer
{
    public int Nutrition() => 100;
}