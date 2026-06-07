using ingot.Core.TraitSystem;

namespace ingot.Test;

public class Lasagna : Item, IFood, IBlockPlacer
{
    public override string Identifier { get; }
    public override string Texture { get; }
    public override int MaxStackSize { get; }
    public override string DisplayName { get; }
    public override bool AllowOffhand => true;
}