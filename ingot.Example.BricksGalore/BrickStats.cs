namespace ingot.Example.BricksGalore;

/// <summary>
/// Facade over the active <see cref="BrickRegistry"/> for generators.
/// Configure content in <c>Program.cs</c> via <see cref="BrickRegistry.Activate"/>.
/// </summary>
public static class BrickStats
{
    public static string Namespace => BrickRegistry.Namespace;

    public static string[] Materials => BrickRegistry.Active.MaterialIds.ToArray();

    public static IReadOnlyDictionary<string, string> Patterns =>
        BrickRegistry.Active.Patterns.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.BaseTexture,
            StringComparer.Ordinal);

    public static string[] PatternIds => BrickRegistry.Active.PatternIds.ToArray();

    public static IReadOnlyDictionary<string, string> PatternOverlays =>
        BrickRegistry.Active.Patterns
            .Where(kvp => kvp.Value.OverlayTexture is not null)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.OverlayTexture!, StringComparer.Ordinal);

    /// <summary>No-op kept for call sites; overlays are resolved at registration time.</summary>
    public static void RefreshOverlays() { }

    public static PatternKind KindOf(string pattern) => BrickRegistry.Active.KindOf(pattern);

    public static bool HasOverlay(string pattern) => BrickRegistry.Active.HasOverlay(pattern);

    public static string? OverlayPath(string pattern) => BrickRegistry.Active.OverlayPath(pattern);

    public static string MaterialIngredient(string material) =>
        BrickRegistry.Active.MaterialIngredient(material);

    public static string PatternCatalyst(string pattern) =>
        BrickRegistry.Active.PatternCatalyst(pattern);

    public static float SecondsToDestroy(string material) =>
        BrickRegistry.Active.SecondsToDestroy(material);

    public static float ExplosionResistance(string material) =>
        BrickRegistry.Active.ExplosionResistance(material);

    public static string Sound(string material) => BrickRegistry.Active.Sound(material);

    public static string[] Tags(string material) => BrickRegistry.Active.Tags(material);

    public static string BlockName(string bodyMaterial, string pattern, string? overlayMaterial) =>
        BrickRegistry.Active.BlockName(bodyMaterial, pattern, overlayMaterial);

    public static string DisplayName(string bodyMaterial, string pattern, string? overlayMaterial) =>
        BrickRegistry.Active.DisplayName(bodyMaterial, pattern, overlayMaterial);

    public static string OverlayLabel(string pattern) => BrickRegistry.Active.OverlayLabel(pattern);

    public static string FormatPattern(string pattern) => BrickRegistry.FormatPattern(pattern);

    public static string TitleCase(string value) => BrickRegistry.TitleCase(value);

    public static bool IsMaterial(string value) => BrickRegistry.Active.IsMaterial(value);

    public static bool IsPattern(string value) => BrickRegistry.Active.IsPattern(value);
}
