using ingot.Core.Common;
using Newtonsoft.Json;
using Formatting = ingot.Core.Common.Formatting;

namespace ingot.Core.Behaviour.Recipe;

/// <summary>
/// Represents a smelting recipe
/// </summary>
public abstract class FurnaceRecipe : IRecipe, IConcreteCompilable<FurnaceRecipe>
{
    /// <inheritdoc/>
    public abstract Identifier Identifier { get; }

    /// <summary>
    /// Array of valid smelting interfaces this recipe can be used on
    /// </summary>
    public virtual string[] Tags => ["furnace"]; 
    
    /// <summary>
    /// The item to be smelted
    /// </summary>
    public abstract Identifier Input { get; }
    /// <summary>
    /// The result item after smelting <see cref="Input"/>
    /// </summary>
    public abstract Identifier Output { get; }

    /// <inheritdoc/>
    public string Compile() => Compile(GetType());
    
    /// <summary>
    /// Compiles the <see cref="FurnaceRecipe"/> (as <paramref name="tType"/>) to JSON
    /// </summary>
    /// <param name="tType">Concrete type of <see cref="FurnaceRecipe"/></param>
    /// <returns>Compiled JSON</returns>
    public static string Compile(Type tType)
    {
        FurnaceRecipe inst = (Activator.CreateInstance(tType) as FurnaceRecipe)!;
        
        CompilerState.Push(inst.Identifier.ToString());

        StringWriter sw = new();
        JsonTextWriter w = new(sw);
        w.Formatting = Newtonsoft.Json.Formatting.Indented;
        w.Indentation = 4;

        JsonHelper json = new(ref w);
        
        w.WriteStartObject();
        
        json.Property("format_version", "1.12");
        json.Object("minecraft:recipe_furnace", () =>
        {
            json.Object("description", () =>
            {
                json.Property("identifier", inst.Identifier.ToString());
            });
            json.Property("tags", inst.Tags);

            json.Property("input", inst.Input);
            json.Property("output", inst.Output);
        });
        
        w.WriteEndObject();
        
        CompilerState.Pop();
        return sw.ToString();
    }
}