using ingot.Core.Common;

namespace ingot.Core.Behaviour.Recipe;

/// <summary>
/// Tag interface for recipes
/// </summary>
public interface IRecipe : IIdentifiable
{
    /// <summary>
    /// Compiles this recipe to JSON.
    /// </summary>
    /// <returns>Compiled JSON</returns>
    string Compile();
}