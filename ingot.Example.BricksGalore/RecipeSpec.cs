using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;

namespace ingot.Example.BricksGalore;

/// <summary>
/// Per-recipe data held on a <see cref="BrickRecipe"/> instance.
/// </summary>
public sealed class RecipeSpec
{
    public required Identifier Identifier { get; init; }
    public required RecipeItem[] Ingredients { get; init; }
    public required RecipeItem Result { get; init; }
}
