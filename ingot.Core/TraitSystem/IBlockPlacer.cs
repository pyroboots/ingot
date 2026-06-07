namespace ingot.Core.TraitSystem;

[Trait("minecraft:block_placer")]
public interface IBlockPlacer
{
    [TraitProperty("@=*")]
    public virtual bool AlignedPlacement() => false;
}