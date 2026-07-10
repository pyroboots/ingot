using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Item;

namespace ingot.Example.CombinationCooking;

public class BaseItem : ingot.Core.Behaviour.Item.Item, ingot.Core.TraitSystem.Traits.Item.ITags, ingot.Core.TraitSystem.Traits.Item.IFood, ingot.Core.TraitSystem.Traits.Item.IUseAnimation, ingot.Core.TraitSystem.Traits.Item.IUseModifiers
{
    public override Identifier Identifier => new("combinationcooking", "bowl_black_magic_red");
    public override string Texture => "bowl_black_magic_red";
    public string[] Tags => ["magic", "red"];
    public override string DisplayName => "Magic Red Soup";
    public override ingot.Core.Common.Enums.CatalogueCategory Category => Enums.CatalogueCategory.None;

    string IUseAnimation.Value => "eat";
    bool IFood.CanAlwaysEat => true;
    string IFood.UsingConvertsTo => new ingot.Core.Common.Identifier("combinationcooking", Tags[0]).ToString();
    int IFood.Nutrition => 1;
    float IFood.SaturationModifier => 2;

    float IUseModifiers.MovementModifier => 0.3f;
    dynamic? IUseModifiers.StartSound => null;

    public override ItemEvents? ItemEvents => new()
    {
        ConsumeEvent = ingot.Core.Scripting.ScriptHandler.FromFile(Path.Combine(AppContext.BaseDirectory, "Scripts", "ConsumeEvent.js")),
    };
}