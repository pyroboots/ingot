namespace ingot.Core.TraitSystem.Traits.Block;

[Trait("minecraft:light_emission", TraitSystem.TraitType.Block)]
public interface ILightEmission : IItemTrait
{
    [TraitProperty]
    public virtual int Value => 0;
}