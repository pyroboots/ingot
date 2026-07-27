using ingot.Core.Common;

using Newtonsoft.Json;

using static ingot.Core.Common.JsonHelper;

using Formatting = Newtonsoft.Json.Formatting;

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

    /// <inheritdoc/>
    public string Compile() => CompileFromInstance(this);

    /// <inheritdoc/>
    public static string Compile(Type tType)
    {
        ShapelessRecipe inst = RecipeCompileHelper.CreateInstance<ShapelessRecipe>(tType);
        return CompileFromInstance(inst);
    }
    
    /// <inheritdoc/>
    public static string Compile<TConcreteType>() where TConcreteType : ShapelessRecipe, new() => Compile(typeof(TConcreteType));

    /// <inheritdoc/>
    public static string CompileFromInstance(ShapelessRecipe inst)
    {
        CompilerState.Push(inst.Identifier.ToString());

        (StringWriter sw, JsonTextWriter w) = RecipeCompileHelper.CreateWriter();

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
public record RecipeItem : ICompilableFragment
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