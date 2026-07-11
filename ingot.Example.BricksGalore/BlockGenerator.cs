using System.Reflection;

using ingot.Core;
using ingot.Core.Common;

using Newtonsoft.Json;

namespace ingot.Example.BricksGalore;

/// <summary>
/// Builds one closed <see cref="BrickBlock{TToken}"/> per composite texture
/// (body x pattern x overlay, or body x pattern when no overlay exists).
/// </summary>
public static class BlockGenerator
{
    /// <summary>
    /// Call after composites have been written to disk.
    /// </summary>
    public static IEnumerable<Type> GenerateBlockTypes()
    {
        string compositesDir = Path.Combine(AppContext.BaseDirectory, "Textures", "Composite");
        if (!Directory.Exists(compositesDir))
            yield break;

        string[] files = Directory.GetFiles(compositesDir, "*.png");
        Array.Sort(files, StringComparer.Ordinal);

        int c = 0;
        foreach (string path in files)
        {
            c++;
            string name = Path.GetFileNameWithoutExtension(path);
            if (!TryParseCompositeName(name, out string body, out string pattern, out string? overlay))
            {
                JsonTextWriter? dummy = null;
                CompilerState.Warn(ref dummy, $"skipping composite with unrecognised name: {name}");
                continue;
            }

            BlockSpec spec = new()
            {
                Identifier = new Identifier(BrickStats.Namespace, name),
                Material = body,
                Pattern = pattern,
                OverlayMaterial = overlay,
                DisplayName = BrickStats.DisplayName(body, pattern, overlay),
                Texture = name,
                TexturePath = path,
                Sound = BrickStats.Sound(body),
                SecondsToDestroy = BrickStats.SecondsToDestroy(body),
                ExplosionResistance = BrickStats.ExplosionResistance(body),
                Tags = BrickStats.Tags(body),
            };

            Type token = DynamicTypeFactory.CreateToken(name);
            Type blockType = typeof(BrickBlock<>).MakeGenericType(token);
            blockType.GetProperty(nameof(BrickBlock<object>.Spec), BindingFlags.Public | BindingFlags.Static)!
                .SetValue(null, spec);

            CompilerState.Info($"({c}/{files.Length}) prepared block type {spec.Identifier}");
            yield return blockType;
        }
    }

    /// <summary>
    /// Parses <c>{body}_{pattern}</c> or <c>{body}_{pattern}_{overlay}</c>.
    /// </summary>
    public static bool TryParseCompositeName(
        string name,
        out string bodyMaterial,
        out string pattern,
        out string? overlayMaterial)
    {
        bodyMaterial = "";
        pattern = "";
        overlayMaterial = null;

        foreach (string body in BrickStats.Materials)
        {
            string prefix = body + "_";
            if (!name.StartsWith(prefix, StringComparison.Ordinal))
                continue;

            string rest = name[prefix.Length..];

            // prefer trailing overlay material form first.
            foreach (string overlay in BrickStats.Materials)
            {
                string suffix = "_" + overlay;
                if (!rest.EndsWith(suffix, StringComparison.Ordinal))
                    continue;

                string maybePattern = rest[..^suffix.Length];
                if (!BrickStats.IsPattern(maybePattern))
                    continue;

                if (!BrickStats.HasOverlay(maybePattern))
                    continue;

                bodyMaterial = body;
                pattern = maybePattern;
                overlayMaterial = overlay;
                return true;
            }

            // plain no-inlay form: {body}_{pattern} (base texture only).
            if (BrickStats.IsPattern(rest))
            {
                bodyMaterial = body;
                pattern = rest;
                overlayMaterial = null;
                return true;
            }
        }

        return false;
    }
}
