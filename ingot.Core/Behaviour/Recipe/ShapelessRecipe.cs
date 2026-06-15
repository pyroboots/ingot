using ingot.Core.Common;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using static ingot.Core.Common.JsonHelper;

namespace ingot.Core.Behaviour.Recipe;

/// <summary>
/// Represents a crafting recipe with no pattern
/// </summary>
public abstract class ShapelessRecipe : IConcreteCompilable<ShapelessRecipe>, IRecipe
{
    /// <inheritdoc/>
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

        JsonHelper json = new(ref w);
        
        w.WriteStartObject();
        
        json.Property("format_version", "1.12");
        json.Object("minecraft:recipe_shapeless", () =>
        {
            json.Object("description", () =>
            {
                json.Property("identifier", inst.Identifier.ToString());
            });
            json.Property("tags", inst.Tags);
            
            CompilerState.Push("ingredients");
            CompilerState.Info("compiling ingredients...");
            json.Array("ingredients", () =>
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
            
            json.Property("result", inst.Result);
        });
        
        w.WriteEndObject();
        
        CompilerState.Pop();
        return sw.ToString();
    }
}

/// <summary>
/// Represents an ingredient or output item in a crafting recipe
/// </summary>
public record RecipeItem : ICompileableFragment
{
    /// <summary>
    /// Identifier of the item
    /// </summary>
    public required Identifier Item;
    /// <summary>
    /// Amount of <see cref="Item"/> required
    /// </summary>
    public int Count = 1;
    /// <summary>
    /// An item tag that matches multiple items
    /// </summary>
    public string? Tag = null;

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        
        writer.WriteStartObject();
        json.Property("item", Item.ToString());
        json.Property("count", Count);
        json.Property("tag", Tag);
        writer.WriteEndObject();
    }
}