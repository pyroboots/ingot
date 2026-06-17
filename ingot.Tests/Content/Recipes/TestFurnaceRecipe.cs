using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;

namespace ingot.Tests.Content.Recipes;

internal class TestFurnaceRecipe : FurnaceRecipe
{
    public override Identifier Identifier => new("test:furnace_recipe");
    public override Identifier Input => new("test:test_item");
    public override Identifier Output => new("test:food_item");
}