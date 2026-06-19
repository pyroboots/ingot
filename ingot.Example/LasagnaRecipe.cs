using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;

namespace ingot.Example;

public class LasagnaRecipe : ShapedRecipe
{
    public override Identifier Identifier => new("test:lasagna");

    private Identifier Cheese => new("test:cheese");
    private Identifier Pasta => new("test:pasta");
    private Identifier Sauce => new("test:spooky_special_sauce");
    public override Identifier?[][] Pattern =>
    [
        [Cheese, Pasta, Cheese],
        [Pasta, Sauce, Pasta],
        [Sauce, Sauce, Sauce]
    ];

    public override Identifier Result => new("test:lasagna");
}