using ingot.Core.Common;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using static ingot.Core.Common.JsonHelper;

namespace ingot.Core.Behaviour.Recipe;

/// <summary>
/// Represents a crafting recipe with a specific pattern
/// </summary>
public abstract class ShapedRecipe : IConcreteCompilable<ShapedRecipe>, IRecipe
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
    public abstract Identifier?[][] Pattern { get; }
    /// <summary>
    /// Resulting item from the <see cref="Pattern"/>
    /// </summary>
    public abstract Identifier Result { get; }
    /// <summary>
    /// Amount of <see cref="Result"/> to get upon crafting
    /// </summary>
    public virtual int ResultAmount => 1;

    /// <inheritdoc/>
    public string Compile() => Compile(GetType());
    
    /// <summary>
    /// Compiles the <see cref="ShapedRecipe"/> (as <paramref name="tType"/>) to JSON
    /// </summary>
    /// <param name="tType">Concrete type of <see cref="ShapedRecipe"/></param>
    /// <returns>Compiled JSON</returns>
    public static string Compile(Type tType)
    {
        ShapedRecipe inst = (Activator.CreateInstance(tType) as ShapedRecipe)!;
        
        CompilerState.Push(inst.Identifier.ToString());

        StringWriter sw = new();
        JsonTextWriter w = new(sw);
        w.Formatting = Formatting.Indented;
        w.Indentation = 4;

        JsonHelper json = new(ref w);
        
        w.WriteStartObject();
        
        json.Property("format_version", "1.12");
        json.Object("minecraft:recipe_shaped", () =>
        {
            json.Object("description", () =>
            {
                json.Property("identifier", inst.Identifier.ToString());
            });
            json.Property("tags", inst.Tags);

            if (inst.Pattern.Length > 3)
                CompilerState.Warn(ref w, "crafting pattern height should not be higher than 3");
            foreach (Identifier[] row in inst.Pattern)
            {
                if (row.Length > 3)
                    CompilerState.Warn(ref w, "crafting pattern width should not be longer than 3");
            }
            
            // yes i hate var, but i aint typing a tuple
            var symbols = Symbolize(inst.Pattern);
            json.Array("pattern", () =>
            {
                foreach (string[] row in symbols.symbolized)
                    w.WriteValue(string.Join("", row));
            });
            
            json.Object("key", () =>
            {
                foreach (var kvp in symbols.mapping)
                    json.Object(kvp.Key.ToString(), () => 
                        json.Property("item", kvp.Value.ToString()));
            });
            
            json.Object("result", () =>
            {
                json.Property("item", inst.Result.ToString());
                json.Property("count", inst.ResultAmount);
            });
        });
        
        w.WriteEndObject();
        
        CompilerState.Pop();
        return sw.ToString();
    }
    
    private static (string[][] symbolized, Dictionary<char, Identifier> mapping) Symbolize(Identifier?[][] pattern)
    {
        // collect unique non null identifiers in order of first appearance
        List<Identifier> uniqueIds = new();
        HashSet<Identifier> seen = new();

        foreach (Identifier?[] row in pattern)
        {
            if (row == null) continue;

            foreach (Identifier? id in row)
            {
                if (id != null && seen.Add(id))
                    uniqueIds.Add(id);
            }
        }

        // ascii symbols (! to ~) - more than enough (we only need 9)
        List<char> availableSymbols = new();
        for (char c = '!'; c <= '~'; c++)
            availableSymbols.Add(c);

        // assign symbols
        Dictionary<char, Identifier> mapping = new();
        Dictionary<Identifier, char> idToSymbol = new();

        for (int i = 0; i < uniqueIds.Count; i++)
        {
            char symbol = availableSymbols[i];
            mapping[symbol] = uniqueIds[i];
            idToSymbol[uniqueIds[i]] = symbol;
        }

        // build grid
        string[][] result = new string[pattern.Length][];

        for (int i = 0; i < pattern.Length; i++)
        {
            if (pattern[i] == null)
            {
                result[i] = null;
                continue;
            }

            result[i] = new string[pattern[i].Length];

            for (int j = 0; j < pattern[i].Length; j++)
            {
                Identifier? original = pattern[i][j];

                if (original == null)
                {
                    // null identifier = single space
                    result[i][j] = " ";
                }
                else if (idToSymbol.TryGetValue(original, out char sym))
                {
                    result[i][j] = sym.ToString();
                }
                else
                {
                    // fallback
                    result[i][j] = original.ToString();
                }
            }
        }

        return (result, mapping);
    }
}