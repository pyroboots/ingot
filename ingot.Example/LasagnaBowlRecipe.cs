using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;

namespace ingot.Example;

public class LasagnaBowlRecipe : ShapelessRecipe
{
    public override Identifier Identifier => new("test:lasagna_from_bowl");

    public override RecipeItem[] Ingredients =>
    [
        new() { Item = new Identifier("minecraft:bowl") },
        new() { Item = new Identifier("test:lasagna") }
    ];

    public override RecipeItem Result => new() { Item = new Identifier("test:lasagna"), Count = 1 };
}