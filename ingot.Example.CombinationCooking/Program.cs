using System.Reflection;

using ingot.Core;
using ingot.Core.Common;

namespace ingot.Example.CombinationCooking;

class Program
{
    private static string Palette(string p) 
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Palettes", $"{p}.gpl"));
    
    private static readonly string[] BlackPalette = TextureGenerator.ParsePalette(Palette("black"));
    private static readonly string[] BluePalette = TextureGenerator.ParsePalette(Palette("blue"));
    private static readonly string[] CyanPalette = TextureGenerator.ParsePalette(Palette("cyan"));
    private static readonly string[] GrayPalette = TextureGenerator.ParsePalette(Palette("gray"));
    private static readonly string[] GreenPalette = TextureGenerator.ParsePalette(Palette("green"));
    private static readonly string[] LimePalette = TextureGenerator.ParsePalette(Palette("lime"));
    private static readonly string[] MagentaPalette = TextureGenerator.ParsePalette(Palette("magenta"));
    private static readonly string[] OrangePalette = TextureGenerator.ParsePalette(Palette("orange"));
    private static readonly string[] PinkPalette = TextureGenerator.ParsePalette(Palette("pink"));
    private static readonly string[] PurplePalette = TextureGenerator.ParsePalette(Palette("purple"));
    private static readonly string[] RedPalette = TextureGenerator.ParsePalette(Palette("red"));
    private static readonly string[] WhitePalette = TextureGenerator.ParsePalette(Palette("white"));
    private static readonly string[] YellowPalette = TextureGenerator.ParsePalette(Palette("yellow"));
    
    static void Main(string[] args)
    {
        CompilerState.ShowInfoLogs = true;
        CompilerState.Push("precompile");
        CompilerState.Push("texture generation");
        
        IngotCommon.WriteHeader();
        
        TextureGenerator.GenerateBowlTextures(new()
        {
            ["black"] = BlackPalette,
            ["blue"] = BluePalette,
            ["cyan"] = CyanPalette,
            ["gray"] = GrayPalette,
            ["green"] = GreenPalette,
            ["lime"] = LimePalette,
            ["magenta"] = MagentaPalette,
            ["orange"] = OrangePalette,
            ["pink"] = PinkPalette,
            ["purple"] = PurplePalette,
            ["red"] = RedPalette,
            ["white"] = WhitePalette,
            ["yellow"] = YellowPalette,
        });

        int vibrancy = 3;
        TextureGenerator.GenerateOverlayTextures(new()
        {
            ["black"] = BlackPalette[vibrancy],
            ["blue"] = BluePalette[vibrancy],
            ["cyan"] = CyanPalette[vibrancy],
            ["gray"] = GrayPalette[vibrancy],
            ["green"] = GreenPalette[vibrancy],
            ["lime"] = LimePalette[vibrancy],
            ["magenta"] = MagentaPalette[vibrancy],
            ["orange"] = OrangePalette[vibrancy],
            ["pink"] = PinkPalette[vibrancy],
            ["purple"] = PurplePalette[vibrancy],
            ["red"] = RedPalette[vibrancy],
            ["white"] = WhitePalette[vibrancy],
            ["yellow"] = YellowPalette[vibrancy],
        });

        List<Tuple<string, string>> bowls = new();
        foreach (string bowl in Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "Textures", "Bowls")))
        {
            string name = Path.GetFileNameWithoutExtension(bowl);
            bowls.Add(new Tuple<string, string>(name, bowl));
        }
        List<Tuple<string, string>> overlays = new();
        foreach (string overlay in Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "Textures", "Overlays")))
        {
            string name = Path.GetFileNameWithoutExtension(overlay).Replace("overlay_", "");
            overlays.Add(new Tuple<string, string>(name, overlay));
        }
        TextureGenerator.GenerateCompositeTextures(bowls.ToArray(), overlays.ToArray());
        
        CompilerState.Pop();
        CompilerState.Pop();
    }
}