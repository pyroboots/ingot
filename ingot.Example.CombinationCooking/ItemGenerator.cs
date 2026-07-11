using System.Reflection;
using System.Text;

using ingot.Core;
using ingot.Core.Common;

using Newtonsoft.Json;

namespace ingot.Example.CombinationCooking;

/// <summary>
/// Builds closed <see cref="FoodItem{TToken}"/> types for every composite texture.
/// </summary>
public static class ItemGenerator
{
    public const string Namespace = "combinationcooking";

    /// <summary>
    /// Creates and configures one closed food item type per composite PNG.
    /// Call after composites have been written to disk.
    /// </summary>
    public static IEnumerable<Type> GenerateItemTypes()
    {
        string compositesDir = Path.Combine(AppContext.BaseDirectory, "Textures", "Composites");
        if (!Directory.Exists(compositesDir))
            yield break;

        string[] files = Directory.GetFiles(compositesDir, "*.png");
        Array.Sort(files, StringComparer.Ordinal);

        int c = 0;
        foreach (string path in files)
        {
            c++;
            string name = Path.GetFileNameWithoutExtension(path);
            if (!TryParseCompositeName(name, out string? bowlColor, out string foodType, out string? fillColor))
            {
                JsonTextWriter? dummy = null;
                CompilerState.Warn(ref dummy, $"skipping composite with unrecognised name: {name}");
                continue;
            }

            (int nutrition, float saturation) = FoodStats.ForType(foodType);

            List<string> tags = [foodType];
            if (fillColor is not null)
                tags.Add(fillColor);
            if (bowlColor is not null)
                tags.Add($"bowl_{bowlColor}");
            
            FoodSpec spec = new()
            {
                Identifier = new Identifier(Namespace, name),
                Nutrition = nutrition,
                SaturationModifier = saturation,
                Tags = tags.ToArray(),
                Texture = name,
                TexturePath = path,
                DisplayName = BuildDisplayName(foodType, fillColor, bowlColor),
            };

            Type token = DynamicTypeFactory.CreateToken(name);
            Type itemType = typeof(FoodItem<>).MakeGenericType(token);
            itemType.GetProperty(nameof(FoodItem<object>.Spec), BindingFlags.Public | BindingFlags.Static)!
                .SetValue(null, spec);

            CompilerState.Info($"({c}/{files.Length}) prepared item type {spec.Identifier}");
            yield return itemType;
        }
    }

    /// <summary>
    /// Parses composite names produced by <see cref="TextureGenerator.GenerateCompositeTextures"/>:
    /// <c>{bowlName}_{overlayName}</c> where bowl is <c>bowl</c> or <c>bowl_{color}</c>
    /// and overlay is <c>{type}</c> or <c>{type}_{color}</c>.
    /// </summary>
    public static bool TryParseCompositeName(
        string name,
        out string? bowlColor,
        out string foodType,
        out string? fillColor)
    {
        bowlColor = null;
        foodType = "";
        fillColor = null;

        // Longest match: bowl_{color}_{type}_{fill} | bowl_{color}_{type} | bowl_{type}_{fill} | bowl_{type}
        if (!name.StartsWith("bowl_", StringComparison.Ordinal) && name != "bowl")
        {
            // all composites start with bowl / bowl_
        }

        string rest;
        if (name.StartsWith("bowl_", StringComparison.Ordinal))
            rest = name["bowl_".Length..];
        else if (name == "bowl")
            return false;
        else
            return false;

        // rest = [color_]?type[_fill]?
        string[] parts = rest.Split('_');
        if (parts.Length == 0)
            return false;

        int i = 0;
        if (parts.Length >= 2 && IsColor(parts[0]) && IsFoodType(parts[1]))
        {
            bowlColor = parts[0];
            foodType = parts[1];
            i = 2;
        }
        else if (IsFoodType(parts[0]))
        {
            foodType = parts[0];
            i = 1;
        }
        else
            return false;

        if (i < parts.Length)
        {
            if (i == parts.Length - 1 && IsColor(parts[i]))
                fillColor = parts[i];
            else
                return false;
        }

        return true;
    }

    public static bool IsColor(string value) =>
        FoodStats.Colors.Contains(value, StringComparer.Ordinal);

    public static bool IsFoodType(string value) =>
        FoodStats.FoodTypes.Contains(value, StringComparer.Ordinal);

    public static string BuildDisplayName(string foodType, string? fillColor, string? bowlColor)
    {
        StringBuilder sb = new();

        if (fillColor is not null)
            sb.Append(FoodStats.TitleCase(fillColor)).Append(' ');

        sb.Append(FoodStats.TitleCase(foodType));

        if (foodType == "soup" || foodType == "magic")
        {
            // "Magic" already implies soup fantasy; still label as soup-like for magic
            if (foodType == "magic")
                sb.Append(" Soup");
            else
                sb.Append(" Bowl");
        }
        else if (foodType == "pasta")
            sb.Append(" Bowl");

        if (bowlColor is not null)
            sb.Append(" (").Append(FoodStats.TitleCase(bowlColor)).Append(" Bowl)");

        return sb.ToString();
    }
}
