using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;

namespace ingot.Tests.Content.Recipes;

internal class TestBrewingMixRecipe : BrewingMixRecipe
{
    public override Identifier Identifier => new("test:brewing_mix_recipe");
    public override Identifier Input => Identifier.VanillaAuxiliary("potion_type", "awkward");
    public override Identifier Reagent => Identifier.Vanilla("nether_wart");
    public override Identifier Output => Identifier.VanillaAuxiliary("potion_type", "strength");
}