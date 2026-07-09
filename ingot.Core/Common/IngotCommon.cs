using Spectre.Console;

namespace ingot.Core.Common;

/// <summary>
/// Common static ingot values
/// </summary>
public static class IngotCommon
{
    /// <summary>
    /// 16x16 ingot icon as bytes
    /// </summary>
    public static readonly byte[] IconBytes = {
        137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82,
        0, 0, 0, 16, 0, 0, 0, 16, 8, 6, 0, 0, 0, 31, 243, 255, 97, 0,
        0, 1, 87, 73, 68, 65, 84, 120, 1, 188, 144, 59, 75, 3, 65, 20,
        133, 239, 174, 141, 40, 104, 97, 161, 22, 54, 190, 64, 16, 49,
        4, 109, 214, 202, 70, 68, 176, 48, 141, 96, 27, 172, 173, 173,
        197, 214, 127, 160, 133, 8, 90, 216, 8, 98, 47, 110, 10, 5, 9,
        34, 8, 190, 26, 193, 71, 97, 161, 98, 32, 16, 72, 242, 221, 112,
        135, 205, 155, 16, 72, 224, 100, 102, 103, 206, 119, 238, 217,
        245, 165, 205, 95, 231, 2, 182, 55, 206, 243, 168, 178, 112, 195,
        6, 0, 166, 233, 145, 5, 65, 201, 165, 253, 124, 52, 164, 42, 192,
        0, 214, 213, 185, 64, 161, 129, 254, 65, 249, 248, 125, 82, 46,
        185, 188, 38, 209, 16, 13, 192, 108, 98, 10, 32, 122, 253, 244,
        28, 56, 220, 55, 33, 163, 67, 165, 225, 139, 177, 132, 11, 241,
        1, 119, 182, 98, 58, 9, 200, 76, 245, 224, 249, 32, 35, 235, 137,
        127, 177, 38, 218, 224, 58, 236, 209, 67, 237, 88, 252, 3, 46,
        46, 194, 212, 96, 114, 92, 239, 0, 209, 241, 105, 175, 236, 29,
        188, 75, 248, 248, 44, 217, 191, 156, 104, 0, 102, 46, 48, 176,
        167, 5, 83, 16, 103, 12, 224, 30, 241, 45, 30, 94, 238, 176, 169,
        92, 0, 23, 24, 226, 241, 148, 0, 177, 103, 18, 178, 70, 120, 12,
        166, 29, 9, 46, 128, 7, 12, 93, 221, 179, 2, 204, 158, 51, 51,
        242, 12, 60, 53, 54, 163, 213, 47, 46, 79, 228, 48, 181, 233, 249,
        187, 71, 43, 222, 217, 77, 168, 239, 11, 128, 48, 179, 70, 225,
        116, 250, 86, 65, 86, 64, 132, 71, 27, 16, 114, 255, 118, 197,
        179, 240, 190, 108, 128, 153, 130, 162, 144, 129, 120, 144, 6,
        176, 33, 228, 251, 231, 75, 104, 3, 0, 136, 217, 132, 167, 150,
        92, 0, 151, 132, 160, 102, 16, 94, 83, 89, 128, 29, 182, 178, 22,
        0, 0, 0, 255, 255, 152, 11, 39, 41, 0, 0, 0, 6, 73, 68, 65, 84,
        3, 0, 197, 90, 207, 33, 150, 209, 5, 227, 0, 0, 0, 0, 73, 69,
        78, 68, 174, 66, 96, 130
    };

    /// <summary>
    /// Primary purple colour used in the ingot icon
    /// </summary>
    public static readonly Color PrimaryColor = Color.FromHex("a678f1");
    /// <summary>
    /// Primary peach-beige colour used in the ingot icon
    /// </summary>
    public static readonly Color SecondaryColor = Color.FromHex("fecbe6");
    
    /// <summary>
    /// Current ingot version
    /// </summary>
    public static readonly Version IngotVersion = new(1, 1, 0);

    public static void WriteHeader()
    {
        CanvasImage img = new(IconBytes);
        AnsiConsole.Write(img);
        AnsiConsole.Write(new Text("ingot compiler ", new Style(PrimaryColor, null, Decoration.Italic)));
        AnsiConsole.Write(new Text($"{IngotVersion}\n\n", new Style(SecondaryColor, null, Decoration.Bold)));
    }
}
