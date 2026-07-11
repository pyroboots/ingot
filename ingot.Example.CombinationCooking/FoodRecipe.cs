using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;

namespace ingot.Example.CombinationCooking;

/// <summary>
/// Parameterized shapeless recipe. Each closed <typeparamref name="TToken"/> carries its own
/// static <see cref="Spec"/>.
/// </summary>
public class FoodRecipe<TToken> : ShapelessRecipe
    where TToken : class
{
    public static RecipeSpec Spec { get; set; } = null!;

    public override Identifier Identifier => Spec.Identifier;
    public override RecipeItem[] Ingredients => Spec.Ingredients;
    public override RecipeItem Result => Spec.Result;
}
