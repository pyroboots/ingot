using SkiaSharp;

using ingot.Core;

namespace ingot.Example.BricksGalore;

/// <summary>
/// Which slice of a material palette to use when recolouring a layer.
/// </summary>
public enum PaletteRange
{
    /// <summary>Normal material brightness (body faces, and cross-material overlays).</summary>
    Body,
    /// <summary>
    /// Darker half - used only when overlay material matches the body so same-colour
    /// mortar/inlay still reads as recessed detail.
    /// </summary>
    Mortar,
}

/// <summary>
/// Recolours greyscale brick templates and pattern-matched overlays, then composites them.
/// </summary>
public static class TextureGenerator
{
    public static void GenerateAll(Dictionary<string, string[]> palettes)
    {
        string texturesRoot = Path.Combine(AppContext.BaseDirectory, "Textures");
        string outDir = Path.Combine(texturesRoot, "Composite");

        // drop stale composites from earlier naming schemes.
        if (Directory.Exists(outDir))
        {
            foreach (string stale in Directory.EnumerateFiles(outDir, "*.png"))
                File.Delete(stale);
        }
        else
            Directory.CreateDirectory(outDir);

        Dictionary<string, string[]> cleaned = palettes.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Select(h => h.Trim()).Where(h => h.Length > 0).ToArray(),
            StringComparer.Ordinal);

        foreach ((string material, string[] colors) in cleaned)
        {
            if (colors.Length == 0)
                CompilerState.Warn(ref _dummy, $"palette '{material}' is empty");
        }

        // pre-count: always body-only plain, plus body x overlay when an overlay exists.
        int total = 0;
        foreach (string pattern in BrickStats.PatternIds)
        {
            total += cleaned.Count; // plain (no inlay)
            if (BrickStats.HasOverlay(pattern))
                total += cleaned.Count * cleaned.Count;
        }

        int c = 0;
        CompilerState.Info(
            $"overlay map: {BrickStats.PatternOverlays.Count} patterns with mortar/inlay " +
            $"({string.Join(", ", BrickStats.PatternOverlays.Keys.OrderBy(k => k))})");

        foreach ((string pattern, string baseRelative) in BrickStats.Patterns)
        {
            CompilerState.Push(pattern);

            string baseSource = Path.Combine(
                texturesRoot,
                baseRelative.Replace('/', Path.DirectorySeparatorChar) + ".png");

            if (!File.Exists(baseSource))
            {
                CompilerState.Warn(ref _dummy, $"missing base texture: {baseSource}");
                CompilerState.Pop();
                continue;
            }

            string? overlayRelative = BrickStats.OverlayPath(pattern);
            string? overlaySource = overlayRelative is null
                ? null
                : Path.Combine(texturesRoot, overlayRelative.Replace('/', Path.DirectorySeparatorChar) + ".png");

            if (overlayRelative is not null && !File.Exists(overlaySource))
            {
                CompilerState.Warn(ref _dummy, $"missing overlay texture: {overlaySource}");
                overlaySource = null;
            }

            foreach ((string bodyMaterial, string[] bodyPalette) in cleaned)
            {
                if (bodyPalette.Length == 0)
                    continue;

                // plain block: base texture only (no mortar/inlay composite).
                {
                    c++;
                    string plainName = BrickStats.BlockName(bodyMaterial, pattern, overlayMaterial: null);
                    string plainPath = Path.Combine(outDir, plainName + ".png");
                    using SKBitmap plain = RecolorByLuminance(baseSource, bodyPalette, PaletteRange.Body);
                    SavePng(plain, plainPath);
                    CompilerState.Info($"({c}/{total}) generated {plainName} (plain)");
                }

                if (overlaySource is null)
                    continue;

                // pattern-matched overlay - recolour with each overlay material and composite.
                // same material as body -> darker recessed inlay; different material -> normal brightness.
                foreach ((string overlayMaterial, string[] overlayPalette) in cleaned)
                {
                    if (overlayPalette.Length == 0)
                        continue;

                    c++;
                    string name = BrickStats.BlockName(bodyMaterial, pattern, overlayMaterial);
                    string outPath = Path.Combine(outDir, name + ".png");

                    PaletteRange overlayRange =
                        string.Equals(bodyMaterial, overlayMaterial, StringComparison.Ordinal)
                            ? PaletteRange.Mortar
                            : PaletteRange.Body;

                    using SKBitmap body = RecolorByLuminance(baseSource, bodyPalette, PaletteRange.Body);
                    using SKBitmap overlay = RecolorByLuminance(overlaySource, overlayPalette, overlayRange);
                    CompositeSrcOver(body, overlay);
                    SavePng(body, outPath);

                    CompilerState.Info($"({c}/{total}) generated {name}");
                }
            }

            CompilerState.Pop();
        }
    }

    /// <summary>
    /// Maps each opaque pixel's luminance onto a palette slice (dark -> light).
    /// </summary>
    public static SKBitmap RecolorByLuminance(
        string inputPath,
        string[] paletteHex,
        PaletteRange range = PaletteRange.Body)
    {
        using SKBitmap source = SKBitmap.Decode(inputPath)
            ?? throw new InvalidOperationException($"failed to decode {inputPath}");

        // copy so we never mutate a shared decode buffer oddly across calls.
        SKBitmap bitmap = source.Copy()
            ?? throw new InvalidOperationException($"failed to copy {inputPath}");

        SKColor[] palette = SelectPaletteRange(paletteHex, range);

        HashSet<byte> luminances = new();
        for (int y = 0; y < bitmap.Height; y++)
        for (int x = 0; x < bitmap.Width; x++)
        {
            SKColor px = bitmap.GetPixel(x, y);
            if (px.Alpha == 0)
                continue;
            luminances.Add(Luminance(px));
        }

        byte[] sortedLuma = luminances.OrderBy(l => l).ToArray();
        Dictionary<byte, SKColor> map = new();

        if (sortedLuma.Length == 0)
            return bitmap;

        if (sortedLuma.Length == 1)
        {
            // single tone: body -> mid of its slice, mortar -> darkest stop.
            int idx = range == PaletteRange.Mortar ? 0 : palette.Length / 2;
            map[sortedLuma[0]] = palette[idx];
        }
        else
        {
            for (int i = 0; i < sortedLuma.Length; i++)
            {
                float t = i / (float)(sortedLuma.Length - 1);
                int index = (int)Math.Round(t * (palette.Length - 1));
                map[sortedLuma[i]] = palette[index];
            }
        }

        for (int y = 0; y < bitmap.Height; y++)
        for (int x = 0; x < bitmap.Width; x++)
        {
            SKColor px = bitmap.GetPixel(x, y);
            if (px.Alpha == 0)
                continue;

            if (!map.TryGetValue(Luminance(px), out SKColor mapped))
                continue;

            // light touch: keep mortar a bit recessed without crushing to near-black.
            if (range == PaletteRange.Mortar)
                mapped = ScaleRgb(mapped, 0.92f);

            bitmap.SetPixel(x, y, new SKColor(mapped.Red, mapped.Green, mapped.Blue, px.Alpha));
        }

        return bitmap;
    }

    /// <summary>
    /// Sorts a palette dark -> light, then picks the slice for body vs mortar.
    /// Body uses mid-light tones; mortar uses the darker half so joints stay recessed
    /// without looking crushed.
    /// </summary>
    public static SKColor[] SelectPaletteRange(string[] paletteHex, PaletteRange range)
    {
        SKColor[] sorted = paletteHex
            .Select(SKColor.Parse)
            .OrderBy(Luminance)
            .ThenBy(c => c.Red)
            .ToArray();

        if (sorted.Length == 0)
            throw new ArgumentException("palette is empty", nameof(paletteHex));

        if (sorted.Length == 1)
            return sorted;

        return range switch
        {
            PaletteRange.Mortar =>
                // darker half (at least 2). e.g. 7 -> 4, 6 -> 3, 9 -> 5.
                sorted[..Math.Max(2, (sorted.Length + 1) / 2)],

            PaletteRange.Body when sorted.Length >= 6 =>
                // skip darkest (mortar territory) and chalky lightest highlight.
                sorted[1..^1],

            PaletteRange.Body when sorted.Length >= 4 =>
                sorted[1..],

            _ => sorted,
        };
    }

    private static SKColor ScaleRgb(SKColor c, float factor) =>
        new(
            (byte)Math.Clamp((int)(c.Red * factor), 0, 255),
            (byte)Math.Clamp((int)(c.Green * factor), 0, 255),
            (byte)Math.Clamp((int)(c.Blue * factor), 0, 255),
            c.Alpha);

    public static void CompositeSrcOver(SKBitmap baseBitmap, SKBitmap overlayBitmap)
    {
        using SKCanvas canvas = new(baseBitmap);
        using SKPaint paint = new() { BlendMode = SKBlendMode.SrcOver };
        canvas.DrawImage(SKImage.FromBitmap(overlayBitmap), 0, 0, new SKSamplingOptions(SKFilterMode.Nearest), paint);
        canvas.Flush();
    }

    public static void SavePng(SKBitmap bitmap, string outputPath)
    {
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        using Stream stream = File.Create(outputPath);
        data.SaveTo(stream);
    }

    public static string[] ParsePalette(string palette)
    {
        List<string> hexColors = new();

        foreach (string ln in palette.Split('\n'))
        {
            string line = ln.TrimEnd('\r');
            if (line.StartsWith("Columns:", StringComparison.Ordinal)
                || line.StartsWith("Name:", StringComparison.Ordinal)
                || line.StartsWith("GIMP", StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length >= 4 && parts[3].StartsWith('#'))
                hexColors.Add(parts[3]);
            else if (parts.Length == 1 && parts[0].StartsWith('#'))
                hexColors.Add(parts[0]);
            else
            {
                string[] spaces = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                string? hex = spaces.LastOrDefault(p => p.StartsWith('#'));
                if (hex is not null)
                    hexColors.Add(hex);
            }
        }

        return hexColors.ToArray();
    }

    private static byte Luminance(SKColor c) =>
        (byte)Math.Clamp((c.Red * 299 + c.Green * 587 + c.Blue * 114) / 1000, 0, 255);

    private static Newtonsoft.Json.JsonTextWriter? _dummy;
}
