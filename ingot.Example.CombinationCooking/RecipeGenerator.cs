using System.Reflection;

using ingot.Core;
using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;

namespace ingot.Example.CombinationCooking;

/// <summary>
/// Builds closed <see cref="FoodRecipe{TToken}"/> types:
/// bowl + type catalyst + colourant → plain-bowl coloured food.
/// </summary>
public static class RecipeGenerator
{
    /// <param name="paletteSwatches">Mid-vibrancy RGB per palette colour name.</param>
    public static IEnumerable<Type> GenerateRecipeTypes(
        IReadOnlyDictionary<string, (byte R, byte G, byte B)> paletteSwatches)
    {
        int total = FoodStats.FoodTypes.Length * VanillaColorants.All.Length;
        int c = 0;

        foreach (string foodType in FoodStats.FoodTypes)
        {
            string catalystId = FoodStats.CatalystForType(foodType);

            foreach (VanillaColorants.Colorant colorant in VanillaColorants.All)
            {
                c++;
                string mappedColor = VanillaColorants.NearestPaletteColor(
                    colorant.R, colorant.G, colorant.B, paletteSwatches);

                // Plain bowl base + typed coloured overlay → bowl_{type}_{color}
                string resultName = $"bowl_{foodType}_{mappedColor}";
                string colorantName = colorant.ItemId.Contains(':')
                    ? colorant.ItemId.Split(':')[^1]
                    : colorant.ItemId;

                RecipeSpec spec = new()
                {
                    Identifier = new Identifier(
                        ItemGenerator.Namespace,
                        $"craft_{resultName}_{colorantName}"),
                    Ingredients =
                    [
                        new RecipeItem { Item = new Identifier("minecraft:bowl") },
                        new RecipeItem { Item = new Identifier(catalystId) },
                        new RecipeItem { Item = colorant.Identifier },
                    ],
                    Result = new RecipeItem
                    {
                        Item = new Identifier(ItemGenerator.Namespace, resultName),
                    },
                };

                Type token = DynamicTypeFactory.CreateToken(spec.Identifier.Name);
                Type recipeType = typeof(FoodRecipe<>).MakeGenericType(token);
                recipeType.GetProperty(nameof(FoodRecipe<object>.Spec), BindingFlags.Public | BindingFlags.Static)!
                    .SetValue(null, spec);

                CompilerState.Info(
                    $"({c}/{total}) prepared recipe {spec.Identifier} → {spec.Result.Item} " +
                    $"(colourant {colorant.ItemId} → {mappedColor})");

                yield return recipeType;
            }
        }
    }
}
