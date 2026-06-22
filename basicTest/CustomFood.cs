using ingot.Core.Behaviour;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Item;

using Version = ingot.Core.Common.Version;

namespace basicTest;

public class CustomFood : Item, IFood, IUseModifiers, IUseAnimation
{
    public override Version FormatVersion => new(1, 21, 0);
    public override Identifier Identifier => new("test", "custom_food");
    public override string Texture => "bread";

    public override string DisplayName => "yummy custom food";

    int IFood.Nutrition => 4;
    bool IFood.CanAlwaysEat => true;
    
    float IUseModifiers.MovementModifier => 0.35f;
    dynamic? IUseModifiers.StartSound => null;
    float IUseModifiers.UseDuration => 1.6f;
    
    string IUseAnimation.Value => "eat";
}