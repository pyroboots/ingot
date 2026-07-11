using System.Reflection;

using ingot.Core;
using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;

namespace ingot.Example.BricksGalore;

/// <summary>
/// Builds closed <see cref="BrickRecipe{TToken}"/> types (shapeless).
/// Plain (no inlay): body material + pattern catalyst + stone -> plain block.
/// Inlay/mortar: plain block + overlay material -> inlay block.
/// Bulk: 4x body + stone -> 4x plain offset bricks.
/// </summary>
public static class RecipeGenerator
{
    private static readonly Identifier Stone = new("minecraft:stone");

    public static IEnumerable<Type> GenerateRecipeTypes()
    {
        int total = EstimateTotal();
        int c = 0;

        foreach (string body in BrickStats.Materials)
        {
            Identifier bodyId = new(BrickStats.MaterialIngredient(body));

            foreach (string pattern in BrickStats.PatternIds)
            {
                Identifier catalystId = new(BrickStats.PatternCatalyst(pattern));
                string catalystName = ShortName(catalystId.ToString());
                string plainName = BrickStats.BlockName(body, pattern, overlayMaterial: null);
                Identifier plainBlockId = new(BrickStats.Namespace, plainName);

                // plain: body + pattern catalyst + stone
                {
                    c++;
                    RecipeSpec plain = new()
                    {
                        Identifier = new Identifier(
                            BrickStats.Namespace,
                            $"craft_{plainName}_{catalystName}"),
                        Ingredients =
                        [
                            new RecipeItem { Item = bodyId },
                            new RecipeItem { Item = catalystId },
                            new RecipeItem { Item = Stone },
                        ],
                        Result = new RecipeItem
                        {
                            Item = plainBlockId,
                            Count = 1,
                        },
                    };

                    yield return CreateRecipeType(plain, c, total);
                }

                if (!BrickStats.HasOverlay(pattern))
                    continue;

                // inlay: plain block + overlay material
                foreach (string overlay in BrickStats.Materials)
                {
                    c++;
                    Identifier overlayId = new(BrickStats.MaterialIngredient(overlay));
                    string resultName = BrickStats.BlockName(body, pattern, overlay);
                    string overlayShort = ShortName(overlayId.ToString());

                    RecipeSpec inlay = new()
                    {
                        Identifier = new Identifier(
                            BrickStats.Namespace,
                            $"craft_{resultName}_{overlayShort}"),
                        Ingredients =
                        [
                            new RecipeItem { Item = plainBlockId },
                            new RecipeItem { Item = overlayId },
                        ],
                        Result = new RecipeItem
                        {
                            Item = new Identifier(BrickStats.Namespace, resultName),
                            Count = 1,
                        },
                    };

                    yield return CreateRecipeType(inlay, c, total);
                }
            }
        }

        // bulk: 4x body + stone -> 4x plain offset bricks
        foreach (string body in BrickStats.Materials)
        {
            c++;
            Identifier bodyId = new(BrickStats.MaterialIngredient(body));
            string resultName = BrickStats.BlockName(body, "offset_bricks", overlayMaterial: null);
            string matShort = ShortName(bodyId.ToString());

            RecipeSpec bulk = new()
            {
                Identifier = new Identifier(
                    BrickStats.Namespace,
                    $"craft_{resultName}_bulk_{matShort}"),
                Ingredients =
                [
                    new RecipeItem { Item = bodyId },
                    new RecipeItem { Item = bodyId },
                    new RecipeItem { Item = bodyId },
                    new RecipeItem { Item = bodyId },
                    new RecipeItem { Item = Stone },
                ],
                Result = new RecipeItem
                {
                    Item = new Identifier(BrickStats.Namespace, resultName),
                    Count = 4,
                },
            };

            yield return CreateRecipeType(bulk, c, total);
        }
    }

    private static int EstimateTotal()
    {
        int n = 0;
        int mats = BrickStats.Materials.Length;
        foreach (string pattern in BrickStats.PatternIds)
        {
            n += mats; // plain
            if (BrickStats.HasOverlay(pattern))
                n += mats * mats; // inlay
        }

        n += mats; // bulk
        return n;
    }

    private static string ShortName(string id) =>
        id.Contains(':') ? id.Split(':')[^1] : id;

    private static Type CreateRecipeType(RecipeSpec spec, int c, int total)
    {
        Type token = DynamicTypeFactory.CreateToken(spec.Identifier.Name);
        Type recipeType = typeof(BrickRecipe<>).MakeGenericType(token);
        recipeType.GetProperty(nameof(BrickRecipe<object>.Spec), BindingFlags.Public | BindingFlags.Static)!
            .SetValue(null, spec);

        CompilerState.Info(
            $"({c}/{total}) prepared recipe {spec.Identifier} -> {spec.Result.Item}" +
            (spec.Result.Count > 1 ? $" x{spec.Result.Count}" : ""));

        return recipeType;
    }
}
