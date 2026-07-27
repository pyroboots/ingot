using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;

namespace ingot.Example.BricksGalore;

/// <summary>
/// Shapeless brick recipe configured by an instance <see cref="Spec"/>.
/// Registered with <c>BehaviourPack.AddRecipeFromInstance</c>.
/// </summary>
public class BrickRecipe : ShapelessRecipe
{
    /// <summary>
    /// Per-recipe data for this instance.
    /// </summary>
    public required RecipeSpec Spec { get; init; }

    public override Identifier Identifier => Spec.Identifier;
    public override RecipeItem[] Ingredients => Spec.Ingredients;
    public override RecipeItem Result => Spec.Result;
}
