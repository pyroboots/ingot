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

    private static readonly Dictionary<string, string[]> AllPalettes = new()
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
    };

    static void Main(string[] args)
    {
        CompilerState.ShowInfoLogs = true;
        CompilerState.Push("precompile");
        CompilerState.Push("texture generation");

        IngotCommon.WriteHeader();

        TextureGenerator.GenerateBowlTextures(AllPalettes);

        const int vibrancy = 3;
        Dictionary<string, string> overlayTints = AllPalettes.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value[vibrancy]);

        TextureGenerator.GenerateOverlayTextures(overlayTints);

        List<Tuple<string, string>> bowls = new();
        foreach (string bowl in Directory.EnumerateFiles(
                     Path.Combine(AppContext.BaseDirectory, "Textures", "Bowls"), "*.png"))
        {
            string name = Path.GetFileNameWithoutExtension(bowl);
            bowls.Add(new Tuple<string, string>(name, bowl));
        }

        List<Tuple<string, string>> overlays = new();
        foreach (string overlay in Directory.EnumerateFiles(
                     Path.Combine(AppContext.BaseDirectory, "Textures", "Overlays"), "*.png"))
        {
            string name = Path.GetFileNameWithoutExtension(overlay).Replace("overlay_", "");
            overlays.Add(new Tuple<string, string>(name, overlay));
        }

        Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Textures", "Composites"));
        TextureGenerator.GenerateCompositeTextures(bowls.ToArray(), overlays.ToArray());

        CompilerState.Pop(); // texture generation

        CompilerState.Push("item generation");
        List<Type> itemTypes = ItemGenerator.GenerateItemTypes().ToList();
        CompilerState.Info($"prepared {itemTypes.Count} item types");
        CompilerState.Pop();

        CompilerState.Push("recipe generation");
        var swatches = VanillaColorants.BuildSwatches(AllPalettes, vibrancy);
        List<Type> recipeTypes = RecipeGenerator.GenerateRecipeTypes(swatches).ToList();
        CompilerState.Info($"prepared {recipeTypes.Count} recipe types");
        CompilerState.Pop();

        CompilerState.Pop(); // precompile

        const string bpUuid = "c1a2b3d4-e5f6-7890-abcd-ef1234567890";
        const string rpUuid = "d2b3c4e5-f6a7-8901-bcde-f12345678901";

        Pack pack = Pack.Create(
            bpUuid,
            "Combination Cooking",
            "Example pack: hundreds of combinable foods generated with ingot",
            rpUuid);

        pack.ScriptsEnabled = true;
        pack.LinkPacks = false;

        MethodInfo addItem = typeof(Pack).GetMethods()
            .Single(m => m.Name == nameof(Pack.AddItem)
                         && m.IsGenericMethodDefinition
                         && m.GetParameters().Length == 0);

        MethodInfo addRecipe = typeof(Pack).GetMethods()
            .Single(m => m.Name == nameof(Pack.AddRecipe)
                         && m.IsGenericMethodDefinition
                         && m.GetParameters().Length == 0);

        CompilerState.Push("pack registration");
        int i = 0;
        foreach (Type itemType in itemTypes)
        {
            i++;
            addItem.MakeGenericMethod(itemType).Invoke(pack, null);
            if (i % 100 == 0 || i == itemTypes.Count)
                CompilerState.Info($"registered {i}/{itemTypes.Count} items");
        }

        i = 0;
        foreach (Type recipeType in recipeTypes)
        {
            i++;
            addRecipe.MakeGenericMethod(recipeType).Invoke(pack, null);
            if (i % 50 == 0 || i == recipeTypes.Count)
                CompilerState.Info($"registered {i}/{recipeTypes.Count} recipes");
        }
        
        pack.AddService(Path.Combine(AppContext.BaseDirectory, "Scripts", "DescriptionService.js"));
        
        CompilerState.Pop();

        pack.CompileComMojang("/home/pyro/.var/app/io.mrarm.mcpelauncher/data/mcpelauncher/games/com.mojang/", cache: true);
    }
}
