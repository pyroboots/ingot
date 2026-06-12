namespace ingot.Core.TraitSystem;

[Trait("minecraft:food", TraitSystem.TraitType.Item)]
public interface IFood
{
    [TraitProperty]
    /* If true you can always eat this item (even when not hungry). Default is set to false. */
    public virtual bool CanAlwaysEat => false;

    [TraitProperty]
    /* Value that is added to the entity's nutrition when the item is used. Default is set to 0. */
    public virtual int Nutrition => 0;

    [TraitProperty]
    /* saturation_modifier is used in this formula: (nutrition * saturation_modifier * 2) when applying the saturation buff. Default is set to 0.6. */
    public virtual float SaturationModifier => 0.6000000238418579f;

    [TraitProperty]
    /* When used, converts to the item specified by the string in this field. Default does not convert item. */
    public virtual string UsingConvertsTo => "";
}
