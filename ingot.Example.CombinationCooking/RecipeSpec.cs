using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;

namespace ingot.Example.CombinationCooking;

/// <summary>
/// Per-recipe data for closed <see cref="FoodRecipe{TToken}"/> types.
/// </summary>
public sealed class RecipeSpec
{
    public required Identifier Identifier { get; init; }
    public required RecipeItem[] Ingredients { get; init; }
    public required RecipeItem Result { get; init; }
}
