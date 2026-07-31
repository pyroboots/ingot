namespace ingot.Core.Common;

/// <summary>
/// Common static ingot values
/// </summary>
public static class IngotCommon
{
    /// <summary>
    /// Current ingot version
    /// </summary>
    public static readonly Version IngotVersion = new(1, 1, 0);

    /// <summary>
    /// Writes the branded ingot header (icon + version) when the console supports it.
    /// Falls back to a plain text line when width/height are unavailable (CI, unit tests, Rider).
    /// </summary>
    public static void WriteHeader()
    {
        Console.WriteLine($"ingot compiler - {IngotVersion}");
    }
}
