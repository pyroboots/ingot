using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.Scripting;
using ingot.Core.TraitSystem.Traits.Item;

namespace ingot.Example.CombinationCooking;

/// <summary>
/// Parameterized food item. Each closed <typeparamref name="TToken"/> type carries its own
/// static <see cref="Spec"/> so ingot's type-based compile path yields unique JSON per combo.
/// </summary>
public class FoodItem<TToken> : Item, ITags, IFood, IUseAnimation, IUseModifiers
    where TToken : class
{
    /// <summary>
    /// Must be assigned on the closed generic type before the pack is compiled.
    /// </summary>
    public static FoodSpec Spec { get; set; } = null!;

    public override Identifier Identifier => Spec.Identifier;
    public override string Texture => Spec.Texture;
    public override string? TexturePath => Spec.TexturePath;
    public override string DisplayName => Spec.DisplayName;
    public override Enums.CatalogueCategory Category => Enums.CatalogueCategory.Items;

    string[] ITags.Tags => Spec.Tags;

    bool IFood.CanAlwaysEat => true;
    int IFood.Nutrition => Spec.Nutrition;
    float IFood.SaturationModifier => Spec.SaturationModifier;
    string IFood.UsingConvertsTo => "minecraft:bowl";

    string IUseAnimation.Value => "eat";
    float IUseModifiers.UseDuration => 1.6f;
    float IUseModifiers.MovementModifier => 0.3f;
    dynamic? IUseModifiers.StartSound => null;
    dynamic? IUseModifiers.StartUsing => "always";

    public override ItemEvents? ItemEvents => new()
    {
        ConsumeEvent = ScriptHandler.FromFile(
            Path.Combine(AppContext.BaseDirectory, "Scripts", "ConsumeEvent.js")),
    };
}
