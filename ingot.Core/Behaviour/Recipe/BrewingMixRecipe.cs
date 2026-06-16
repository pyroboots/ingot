using ingot.Core.Common;
using Newtonsoft.Json;
using Formatting = ingot.Core.Common.Formatting;

namespace ingot.Core.Behaviour.Recipe;

/// <summary>
/// Represents a crafting recipe with a specific pattern
/// </summary>
public abstract class BrewingMixRecipe : IRecipe, IConcreteCompilable<BrewingMixRecipe>
{
    /// <inheritdoc/>
    public abstract Identifier Identifier { get; }
    
    /// <summary>
    /// Array of valid crafting brewing this recipe can be used on
    /// </summary>
    public virtual string[] Tags => ["brewing_stand"];
    
    public abstract Identifier Input { get; }
    public abstract Identifier Reagent { get; }
    public abstract Identifier Output { get; }
    
    /// <summary>
    /// Compiles the <see cref="BrewingMixRecipe"/> (as <paramref name="tType"/>) to JSON
    /// </summary>
    /// <param name="tType">Concrete type of <see cref="BrewingMixRecipe"/></param>
    /// <returns>Compiled JSON</returns>
    public static string Compile(Type tType)
    {
        BrewingMixRecipe inst = (Activator.CreateInstance(tType) as BrewingMixRecipe)!;
        
        CompilerState.Push(inst.Identifier.ToString());

        StringWriter sw = new();
        JsonTextWriter w = new(sw);
        w.Formatting = Newtonsoft.Json.Formatting.Indented;
        w.Indentation = 4;

        JsonHelper json = new(ref w);
        
        w.WriteStartObject();
        
        json.Property("format_version", "1.12");
        json.Object("minecraft:recipe_brewing_mix", () =>
        {
            json.Object("description", () =>
            {
                json.Property("identifier", inst.Identifier.ToString());
            });
            json.Property("tags", inst.Tags);

            if (inst.Input.HasAuxiliary == false)
                CompilerState.Warn(ref w, "recipe input intended to be potion type, yet has no auxiliary");
            if (inst.Output.HasAuxiliary == false)
                CompilerState.Warn(ref w, "recipe output intended to be potion type, yet has no auxiliary");
            
            json.Property("input", inst.Input);
            json.Property("reagent", inst.Reagent);
            json.Property("output", inst.Output);
        });
        
        w.WriteEndObject();
        
        CompilerState.Pop();
        return sw.ToString();

    }
}