using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;

using ingot.Core;

namespace ingot.Example.CombinationCooking;

public class TextureGenerator
{
    public static void GenerateBowlTextures(Dictionary<string, string[]> colors)
    {
        CompilerState.Push("bowl");
        
        int c = 0;
        foreach (var kvp in colors)
        {
            c++;
            GenerateBowlTexture(kvp.Key, kvp.Value.ToList());
            CompilerState.Info($"({c}/{colors.Count}) generated {kvp.Key} bowl texture");
        }
        
        CompilerState.Pop();
    }
    
    public static void GenerateBowlTexture(string color, List<string> colors)
    {
        string sourceBowl = Path.Combine(AppContext.BaseDirectory, "Textures", "Bowls", "bowl.png");
        string outputBowl = Path.Combine(AppContext.BaseDirectory, "Textures", "Bowls", $"bowl_{color}.png");

        // 2 colors use the same one because theyre so close visually
        // so we insert a copy to line up
        colors.Insert(2, colors[1]);

        string[] colsToReplace =
        {
            "#1f1502",
            "#302109", // same
            "#3a2706", // same
            "#3f2b0b",
            "#43300b",
            "#533909",
            "#714f0f",
        };

        Dictionary<string, string> colorMap = new();
        for (int i = colors.Count - 1; i >= 0; i--)
            colorMap[colsToReplace[i]] = colors[i];

        RecolorTexture(sourceBowl, outputBowl, colorMap);
    }
    
    public static void GenerateOverlayTextures(Dictionary<string, string> tints)
    {
        CompilerState.Push("overlay");
        
        CompilerState.Push("magic");
        int c = 0;
        foreach (var kvp in tints)
        {
            c++;
            GenerateOverlayTexture(kvp.Key, kvp.Value, "magic");
            CompilerState.Info($"({c}/{tints.Count}) generated {kvp.Key} magic overlay texture");
        }
        CompilerState.Pop();
        
        CompilerState.Push("pasta");
        c = 0;
        foreach (var kvp in tints)
        {
            c++;
            GenerateOverlayTexture(kvp.Key, kvp.Value, "pasta");
            CompilerState.Info($"({c}/{tints.Count}) generated {kvp.Key} pasta overlay texture");
        }
        CompilerState.Pop();
        
        CompilerState.Push("soup");
        c = 0;
        foreach (var kvp in tints)
        {
            c++;
            GenerateOverlayTexture(kvp.Key, kvp.Value, "soup");
            CompilerState.Info($"({c}/{tints.Count}) generated {kvp.Key} soup overlay texture");
        }
        CompilerState.Pop();
        
        CompilerState.Pop();
    }
    
    public static void GenerateOverlayTexture(string color, string tint, string type)
    {
        string sourceOverlay = Path.Combine(AppContext.BaseDirectory, "Textures", "Overlays", $"overlay_{type}.png");
        string outputOverlay = Path.Combine(AppContext.BaseDirectory, "Textures", "Overlays", $"overlay_{type}_{color}.png");

        TintTexture(sourceOverlay, outputOverlay, tint);
    }

    public static void GenerateCompositeTextures(Tuple<string, string>[] bowls, Tuple<string, string>[] overlays)
    {
        CompilerState.Push("composite");

        string outDir = Path.Combine(AppContext.BaseDirectory, "Textures", "Composites");
        Directory.CreateDirectory(outDir);

        int permutations = bowls.Length * overlays.Length;
        int c = 0;
        foreach (var bowl in bowls)
        foreach (var overlay in overlays)
        {
            c++;
            string name = $"{bowl.Item1}_{overlay.Item1}";
            string outPath = Path.Combine(outDir, name + ".png");
            CompositeTextures(bowl.Item2, outPath, overlay);

            CompilerState.Info($"({c}/{permutations}) composited texture {name} from bowl {bowl.Item1} and overlay {overlay.Item1}");
        }

        CompilerState.Pop();
    }

    public static void RecolorTexture(string inputPath, string outputPath, Dictionary<string, string> colorMap)
    {
        using SKBitmap bitmap = SKBitmap.Decode(inputPath);

        Dictionary<SKColor, SKColor> replacements = new();

        foreach (var kvp in colorMap)
        {
            SKColor oldColor = SKColor.Parse(kvp.Key);
            SKColor newColor = SKColor.Parse(kvp.Value);
            replacements[oldColor] = newColor;
        }

        // replace pixels
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                SKColor pixel = bitmap.GetPixel(x, y);

                if (replacements.TryGetValue(pixel, out SKColor newColor))
                {
                    bitmap.SetPixel(x, y, newColor);
                }
            }
        }

        // save as png
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        using Stream stream = File.OpenWrite(outputPath);
        data.SaveTo(stream);
    }
    
    public static void TintTexture(string inputPath, string outputPath, string tintHex)
    {
        using SKBitmap bitmap = SKBitmap.Decode(inputPath);
    
        SKColor tint = SKColor.Parse(tintHex);

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                SKColor pixel = bitmap.GetPixel(x, y);
            
                // multiply tint
                byte r = (byte)(pixel.Red * tint.Red / 255);
                byte g = (byte)(pixel.Green * tint.Green / 255);
                byte b = (byte)(pixel.Blue * tint.Blue / 255);
                byte a = pixel.Alpha; // keep original alpha

                bitmap.SetPixel(x, y, new SKColor(r, g, b, a));
            }
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        using Stream stream = File.OpenWrite(outputPath);
        data.SaveTo(stream);
    }
    
    public static void CompositeTextures(string basePath, string outputPath, params Tuple<string, string>[] layers)
    {
        using SKBitmap baseBitmap = SKBitmap.Decode(basePath);
        using SKCanvas canvas = new(baseBitmap);

        foreach (var layer in layers)
        {
            using SKBitmap layerBitmap = SKBitmap.Decode(layer.Item2);
            using SKPaint paint = new() { BlendMode = SKBlendMode.SrcOver };

            canvas.DrawBitmap(layerBitmap, 0, 0, paint);
        }

        canvas.Flush();

        using SKImage image = SKImage.FromBitmap(baseBitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 95);
        using Stream stream = File.OpenWrite(outputPath);
        data.SaveTo(stream);
    }
    
    public static string[] ParsePalette(string palette)
    {
        List<string> hexColors = new();

        foreach (string ln in palette.Split('\n'))
        {
            if (ln.StartsWith("Columns:") || ln.StartsWith("Name:") || ln.StartsWith("GIMP") || ln.IsWhiteSpace())
                continue;
            //  0   1    2     3
            // 42	56	123	#2a387b
            string[] parts = ln.Split('\t');
            hexColors.Add(parts[3]);
        }
        
        return hexColors.ToArray();
    }
}