using ingot.Core.Behaviour.Recipe;

namespace ingot.Core.Resource;

/// <summary>
/// Represents a recipe reference
/// </summary>
/// <typeparam name="TRecipe">Recipe to reference</typeparam>
public class RecipeReference<TRecipe> where TRecipe : IRecipe, new()
{
    private readonly string _id;
    
    /// <summary>
    /// Implicitly registers and references an <see cref="IRecipe"/>
    /// </summary>
    /// <exception cref="InvalidOperationException">recipe registration only valid during pack compilation</exception>
    public RecipeReference()
    {
        Pack pack = CompilerState.CurrentPack 
                    ?? throw new InvalidOperationException("recipe registration only valid during pack compilation");
        
        _id = new TRecipe().Identifier;

        if (pack.BehaviourPack.Recipes.All((r) => r.Identifier != _id))
        {
            CompilerState.Info($"implicitly registered recipe {_id}");
            pack.BehaviourPack.AddRecipe<TRecipe>();
        }
    }

    /// <summary/>
    public static implicit operator string(RecipeReference<TRecipe> recipe) => recipe._id;
    /// <summary/>
    public static implicit operator RecipeReference(RecipeReference<TRecipe> recipe) => new(typeof(TRecipe), recipe._id);
}

/// <summary/>
public class RecipeReference(Type parent, string reference)
{
    /// <summary>
    /// Underlying type of the reference
    /// </summary>
    public Type Parent = parent;
    /// <summary>
    /// Implicit reference string of the asset
    /// </summary>
    public string Reference = reference;
}