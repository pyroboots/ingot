namespace ingot.Core.TraitSystem;

[Trait("minecraft:food", TraitSystem.TraitType.Item)]
public interface IFood
{
    [TraitProperty]
    public virtual bool CanAlwaysEat() => true;
    
    [TraitProperty]
    public virtual int Nutrition() => 3;
    
    [TraitProperty]
    public virtual float SaturationModifier() => 0.6f;
    
    [TraitProperty]
    public virtual string UsingConvertsTo() => "";
}