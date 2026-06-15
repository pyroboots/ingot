using ingot.Core.Common;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using static ingot.Core.Common.JsonHelper;

namespace ingot.Core.Behaviour.Recipe;

/// <summary>
/// Represents a crafting recipe with no pattern
/// </summary>
public abstract class ShapelessRecipe : IConcreteCompilable<ShapelessRecipe>
{
    /// <summary>
    /// Identifier of the <see cref="ShapelessRecipe"/>
    /// </summary>
    public abstract Identifier Identifier { get; }
    
    /// <summary>
    /// Array of valid crafting interfaces this recipe can be used on
    /// </summary>
    public virtual string[] Tags => ["crafting_table"];
    /// <summary>
    /// Array of ingredients used to create the <see cref="Result"/>
    /// </summary>
    public abstract RecipeItem[] Ingredients { get; }
    /// <summary>
    /// Resulting item from the <see cref="Ingredients"/>
    /// </summary>
    public abstract RecipeItem Result { get; }

    /// <summary>
    /// Compiles the <see cref="ShapelessRecipe"/> (as <paramref name="tType"/>) to JSON
    /// </summary>
    /// <param name="tType">Concrete type of <see cref="ShapelessRecipe"/></param>
    /// <returns>Compiled JSON</returns>

    public static string Compile(Type tType)
    {
        ShapelessRecipe inst = (Activator.CreateInstance(tType) as ShapelessRecipe)!;
        
        CompilerState.Push(inst.Identifier.ToString());

        StringWriter sw = new();
        JsonTextWriter w = new(sw);
        w.Formatting = Formatting.Indented;
        w.Indentation = 4;

        w.WriteStartObject();
        
        Property(ref w, "format_version", "1.12");
        Object(ref w, "minecraft:recipe_shapeless", w =>
        {
            Object(ref w, "description", w =>
            {
                Property(ref w, "identifier", inst.Identifier);
            });
            Property(ref w, "tags", inst.Tags);
            
            CompilerState.Push("ingredients");
            CompilerState.Info("compiling ingredients...");
            Array(ref w, "ingredients", w =>
            {
                int c = 0;
                foreach (RecipeItem i in inst.Ingredients)
                {
                    c++;
                    i.Compile(ref w);
                    CompilerState.Info($"({c}/{inst.Ingredients.Length}) compiled ingredient {i.Item}");
                }
            });
            CompilerState.Pop();
            CompilerState.Info("compiled ingredients");
            
            Property(ref w, "result", inst.Result);
        });
        
        CompilerState.Pop();
        return sw.ToString();
    }
}