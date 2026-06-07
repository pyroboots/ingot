namespace ingot.Core.TraitSystem;

[Trait("minecraft:block_placer", TraitSystem.TraitType.Item)]
public interface IBlockPlacer
{
    [TraitProperty]
    public virtual bool BlockPlacer_AlignedPlacement() => false;

    [TraitProperty]
    public virtual string BlockPlacer_Block() => "";
}