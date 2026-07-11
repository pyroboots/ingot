using System.Text;

using ingot.Core;

namespace ingot.Example.BricksGalore;

public enum PatternKind
{
    Bricks,
    Chiseled,
    Tiles,
    /// <summary>Uncategorized or multi-folder patterns; overlay search covers all overlay dirs.</summary>
    All,
}

/// <summary>
/// One material (palette + recipe ingredient + block stats + lore colour).
/// </summary>
public sealed class MaterialDef
{
    public required string Id { get; init; }
    /// <summary>Hex colours from the .gpl palette (dark -> light as authored).</summary>
    public required string[] Palette { get; init; }
    /// <summary>Vanilla item used when crafting body or overlay.</summary>
    public required string Ingredient { get; init; }
    /// <summary>Minecraft § colour code for lore (e.g. "§d").</summary>
    public string LoreColor { get; init; } = "§f";
    public string Sound { get; init; } = "stone";
    public float SecondsToDestroy { get; init; } = 2f;
    public float ExplosionResistance { get; init; } = 6f;
    public string[] Tags { get; init; } =
    [
        "minecraft:is_pickaxe_item_destructible",
        "minecraft:iron_pick_diggable",
    ];
}

/// <summary>
/// One block pattern (base texture + optional overlay + craft catalyst).
/// </summary>
public sealed class PatternDef
{
    public required string Id { get; init; }
    /// <summary>Relative path under Textures/ without .png (e.g. Bricks/offset_bricks).</summary>
    public required string BaseTexture { get; init; }
    /// <summary>Relative path under Textures/ without .png, or null if none.</summary>
    public string? OverlayTexture { get; init; }
    /// <summary>Vanilla item that selects this pattern in recipes.</summary>
    public required string Catalyst { get; init; }
    public PatternKind Kind { get; init; }

    public bool HasOverlay => OverlayTexture is not null;
}

/// <summary>
/// Content registry. Configure materials and patterns from <c>Program.cs</c>, then call
/// <see cref="BrickRegistry.Activate"/> before generation.
/// </summary>
public sealed class BrickRegistry
{
    public const string Namespace = "bricksgalore";

    private static BrickRegistry? _active;

    /// <summary>Active content after <see cref="Activate"/>.</summary>
    public static BrickRegistry Active =>
        _active ?? throw new InvalidOperationException(
            "BrickRegistry not configured - call BrickRegistry.Activate(...) from Program.cs first.");

    public static void Activate(BrickRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (registry.Materials.Count == 0)
            throw new InvalidOperationException("registry has no materials");
        if (registry.Patterns.Count == 0)
            throw new InvalidOperationException("registry has no patterns");
        _active = registry;
    }

    private readonly Dictionary<string, MaterialDef> _materials = new(StringComparer.Ordinal);
    private readonly Dictionary<string, PatternDef> _patterns = new(StringComparer.Ordinal);

    public IReadOnlyList<string> MaterialIds =>
        _materials.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray();

    public IReadOnlyList<string> PatternIds =>
        _patterns.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray();

    public IReadOnlyDictionary<string, MaterialDef> Materials => _materials;
    public IReadOnlyDictionary<string, PatternDef> Patterns => _patterns;

    public Dictionary<string, string[]> PaletteMap =>
        _materials.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Palette, StringComparer.Ordinal);

    // -------------------------------------------------------------------------
    // registration api (used from program.cs)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Register a material. Loads <c>Palettes/{id}.gpl</c> by default.
    /// </summary>
    public BrickRegistry AddMaterial(
        string id,
        string ingredient,
        string? paletteFile = null,
        string loreColor = "§f",
        string sound = "stone",
        float secondsToDestroy = 2f,
        float explosionResistance = 6f,
        string[]? tags = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(ingredient);

        string gplName = paletteFile ?? id;
        string gplPath = Path.Combine(AppContext.BaseDirectory, "Palettes", $"{gplName}.gpl");
        if (!File.Exists(gplPath))
            throw new FileNotFoundException($"palette not found for material '{id}'", gplPath);

        string[] palette = TextureGenerator.ParsePalette(File.ReadAllText(gplPath));
        if (palette.Length == 0)
            throw new InvalidOperationException($"palette '{gplPath}' has no colours");

        _materials[id] = new MaterialDef
        {
            Id = id,
            Palette = palette,
            Ingredient = ingredient,
            LoreColor = loreColor,
            Sound = sound,
            SecondsToDestroy = secondsToDestroy,
            ExplosionResistance = explosionResistance,
            Tags = tags ??
            [
                "minecraft:is_pickaxe_item_destructible",
                "minecraft:iron_pick_diggable",
            ],
        };

        return this;
    }

    /// <summary>
    /// Register a pattern. Overlay is auto-discovered under <c>Textures/Overlays/</c> unless
    /// <paramref name="overlayTexture"/> is set (or <paramref name="noOverlay"/> is true).
    /// </summary>
    public BrickRegistry AddPattern(
        string id,
        string baseTexture,
        string catalyst,
        PatternKind? kind = null,
        string? overlayTexture = null,
        bool noOverlay = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseTexture);
        ArgumentException.ThrowIfNullOrWhiteSpace(catalyst);

        string texturesRoot = Path.Combine(AppContext.BaseDirectory, "Textures");
        string basePath = Path.Combine(texturesRoot, baseTexture.Replace('/', Path.DirectorySeparatorChar) + ".png");
        if (!File.Exists(basePath))
            throw new FileNotFoundException($"base texture not found for pattern '{id}'", basePath);

        PatternKind resolvedKind = kind ?? InferKind(baseTexture);

        string? overlay = null;
        if (!noOverlay)
        {
            if (overlayTexture is not null)
            {
                string overlayPath = Path.Combine(
                    texturesRoot,
                    overlayTexture.Replace('/', Path.DirectorySeparatorChar) + ".png");
                if (!File.Exists(overlayPath))
                    throw new FileNotFoundException($"overlay texture not found for pattern '{id}'", overlayPath);
                overlay = overlayTexture.Replace('\\', '/');
            }
            else
            {
                overlay = FindOverlay(texturesRoot, id, resolvedKind);
            }
        }

        _patterns[id] = new PatternDef
        {
            Id = id,
            BaseTexture = baseTexture.Replace('\\', '/'),
            OverlayTexture = overlay,
            Catalyst = catalyst,
            Kind = resolvedKind,
        };

        return this;
    }

    /// <summary>
    /// Scan <c>Textures/{Bricks,Chiseled,Tiles,All}/*.png</c> and register any patterns not already added.
    /// Catalysts default to <c>minecraft:brick</c> unless overridden later with another <see cref="AddPattern"/> call.
    /// Prefer explicit <see cref="AddPattern"/> for catalysts; this is a convenience for new art drops.
    /// </summary>
    public BrickRegistry DiscoverUnregisteredPatterns(string defaultCatalyst = "minecraft:brick")
    {
        string texturesRoot = Path.Combine(AppContext.BaseDirectory, "Textures");
        foreach (string folder in new[] { "Bricks", "Chiseled", "Tiles", "All" })
        {
            string dir = Path.Combine(texturesRoot, folder);
            if (!Directory.Exists(dir))
                continue;

            foreach (string file in Directory.EnumerateFiles(dir, "*.png"))
            {
                string id = Path.GetFileNameWithoutExtension(file);
                if (_patterns.ContainsKey(id))
                    continue;

                string baseTexture = $"{folder}/{id}";
                CompilerState.Info($"auto-discovered pattern '{id}' -> {baseTexture} (catalyst {defaultCatalyst})");
                AddPattern(id, baseTexture, defaultCatalyst);
            }
        }

        return this;
    }

    // -------------------------------------------------------------------------
    // lookups used by generators
    // -------------------------------------------------------------------------

    public MaterialDef Material(string id) =>
        _materials.TryGetValue(id, out MaterialDef? m)
            ? m
            : throw new KeyNotFoundException($"unknown material '{id}'");

    public PatternDef Pattern(string id) =>
        _patterns.TryGetValue(id, out PatternDef? p)
            ? p
            : throw new KeyNotFoundException($"unknown pattern '{id}'");

    public bool IsMaterial(string id) => _materials.ContainsKey(id);
    public bool IsPattern(string id) => _patterns.ContainsKey(id);
    public bool HasOverlay(string patternId) =>
        _patterns.TryGetValue(patternId, out PatternDef? p) && p.HasOverlay;

    public string? OverlayPath(string patternId) =>
        _patterns.TryGetValue(patternId, out PatternDef? p) ? p.OverlayTexture : null;

    public PatternKind KindOf(string patternId) => Pattern(patternId).Kind;

    public string MaterialIngredient(string id) => Material(id).Ingredient;
    public string PatternCatalyst(string id) => Pattern(id).Catalyst;
    public float SecondsToDestroy(string id) => Material(id).SecondsToDestroy;
    public float ExplosionResistance(string id) => Material(id).ExplosionResistance;
    public string Sound(string id) => Material(id).Sound;
    public string[] Tags(string id) => Material(id).Tags;

    public string BlockName(string body, string pattern, string? overlay) =>
        overlay is null ? $"{body}_{pattern}" : $"{body}_{pattern}_{overlay}";

    /// <summary>
    /// Natural name: style + material + type, e.g. "Offset Amethyst Bricks".
    /// With overlay: "Offset Amethyst Bricks (Gold Mortar)".
    /// </summary>
    public string DisplayName(string body, string pattern, string? overlay)
    {
        string name = FormatBlockTitle(body, pattern);
        if (overlay is null)
            return name;

        return $"{name} ({TitleCase(overlay)} {OverlayLabel(pattern)})";
    }

    public string OverlayLabel(string pattern) =>
        KindOf(pattern) is PatternKind.Chiseled && !pattern.Contains("beveled", StringComparison.Ordinal)
            ? "Inlay"
            : "Mortar";

    /// <summary>
    /// Builds natural titles, e.g. "Offset Amethyst Bricks", "Medium Resin Tiles",
    /// "Bee Chiseled Gold".
    /// </summary>
    public static string FormatBlockTitle(string body, string pattern)
    {
        string[] parts = pattern.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return TitleCase(body);

        string mat = TitleCase(body);
        string last = parts[^1];

        // offset_bricks -> Offset Amethyst Bricks; medium_tiles -> Medium Resin Tiles
        if (parts.Length >= 2 && last is "bricks" or "tiles")
        {
            string style = string.Join(' ', parts[..^1].Select(TitleCase));
            return $"{style} {mat} {TitleCase(last)}";
        }

        // bee_chiseled -> Bee Chiseled Gold (style words then material)
        if (parts.Length >= 1 && last == "chiseled")
        {
            string style = string.Join(' ', parts.Select(TitleCase));
            return $"{style} {mat}";
        }

        // fallback
        return $"{string.Join(' ', parts.Select(TitleCase))} {mat}";
    }

    public static string FormatPattern(string pattern) =>
        string.Join(' ', pattern.Split('_').Select(TitleCase));

    public static string TitleCase(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToUpperInvariant(value[0]) + value[1..];

    /// <summary>
    /// Writes <c>DescriptionService.js</c> from the active material list so lore colours stay in sync.
    /// </summary>
    public string GenerateDescriptionService(string outputPath)
    {
        StringBuilder materialsArray = new();
        StringBuilder colorsObject = new();
        foreach (string id in MaterialIds)
        {
            materialsArray.AppendLine($"    \"{id}\",");
            colorsObject.AppendLine($"    {id}: \"{Material(id).LoreColor}\",");
        }

        string js = $$"""
            // auto-generated by brickregistry - do not edit by hand
            // tick service: lore shows mortar/inlay only; display name is body + pattern.

            const MATERIALS = [
            {{materialsArray}}];

            const MATERIAL_COLORS = {
            {{colorsObject}}};

            const NS = "{{Namespace}}:";

            function titleCase(value) {
                if (!value) return value;
                return value.charAt(0).toUpperCase() + value.slice(1);
            }

            function materialColor(material) {
                return MATERIAL_COLORS[material] || "§f";
            }

            function overlayLabel(pattern) {
                if (pattern.includes("chiseled") && !pattern.includes("beveled")) {
                    return "Inlay";
                }
                return "Mortar";
            }

            function parseBrickId(typeId) {
                if (typeof typeId !== "string" || !typeId.startsWith(NS)) {
                    return null;
                }

                const name = typeId.slice(NS.length);

                for (const body of MATERIALS) {
                    const prefix = body + "_";
                    if (!name.startsWith(prefix)) {
                        continue;
                    }

                    const rest = name.slice(prefix.length);

                    for (const overlay of MATERIALS) {
                        const suffix = "_" + overlay;
                        if (!rest.endsWith(suffix)) {
                            continue;
                        }

                        const pattern = rest.slice(0, -suffix.length);
                        if (pattern.length === 0) {
                            continue;
                        }

                        return { body, pattern, overlay };
                    }

                    if (rest.length > 0) {
                        return { body, pattern: rest, overlay: null };
                    }
                }

                return null;
            }

            function buildLore(parsed) {
                if (!parsed.overlay) {
                    return [];
                }

                const label = overlayLabel(parsed.pattern);
                const color = materialColor(parsed.overlay);
                return [`§7${label}: ${color}${titleCase(parsed.overlay)}`];
            }

            function loreEquals(a, b) {
                if (!Array.isArray(a) || !Array.isArray(b) || a.length !== b.length) {
                    return false;
                }
                for (let i = 0; i < a.length; i++) {
                    if (a[i] !== b[i]) {
                        return false;
                    }
                }
                return true;
            }

            for (const player of world.getAllPlayers()) {
                const inv = player.getComponent("minecraft:inventory");
                if (!inv || !inv.container) {
                    continue;
                }

                const cont = inv.container;
                for (let i = 0; i < cont.size; i++) {
                    const item = cont.getItem(i);
                    if (!item || typeof item.typeId !== "string") {
                        continue;
                    }

                    if (!item.typeId.startsWith(NS)) {
                        continue;
                    }

                    const parsed = parseBrickId(item.typeId);
                    if (!parsed) {
                        continue;
                    }

                    const lore = buildLore(parsed);
                    const existing =
                        typeof item.getLore === "function" ? item.getLore() : undefined;

                    if (loreEquals(existing, lore)) {
                        continue;
                    }

                    if (typeof item.setLore === "function") {
                        item.setLore(lore);
                        cont.setItem(i, item);
                    }
                }
            }

            """;

        // normalise generated indentation (raw string has leading spaces from template).
        js = string.Join('\n', js.Split('\n').Select(l => l.StartsWith("            ") ? l[12..] : l));

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        File.WriteAllText(outputPath, js);
        CompilerState.Info($"wrote DescriptionService.js from registry ({MaterialIds.Count} materials) -> {outputPath}");
        return outputPath;
    }

    private static PatternKind InferKind(string baseTexture)
    {
        string t = baseTexture.Replace('\\', '/');
        if (t.StartsWith("Bricks/", StringComparison.OrdinalIgnoreCase))
            return PatternKind.Bricks;
        if (t.StartsWith("Tiles/", StringComparison.OrdinalIgnoreCase))
            return PatternKind.Tiles;
        if (t.StartsWith("Chiseled/", StringComparison.OrdinalIgnoreCase))
            return PatternKind.Chiseled;
        return PatternKind.All;
    }

    private static string? FindOverlay(string texturesRoot, string patternId, PatternKind kind)
    {
        string overlaysRoot = Path.Combine(texturesRoot, "Overlays");
        if (!Directory.Exists(overlaysRoot))
            return null;

        List<string> candidates = new();

        if (kind is not PatternKind.All)
        {
            candidates.Add(Path.Combine(overlaysRoot, kind.ToString(), $"{patternId}_mortar.png"));
            candidates.Add(Path.Combine(overlaysRoot, kind.ToString(), $"{patternId}_overlay.png"));
        }

        // also check standard overlay folders (beveled mortar under bricks, or kind all).
        foreach (string folder in new[] { "Bricks", "Chiseled", "Tiles", "All" })
        {
            candidates.Add(Path.Combine(overlaysRoot, folder, $"{patternId}_mortar.png"));
            candidates.Add(Path.Combine(overlaysRoot, folder, $"{patternId}_overlay.png"));
        }

        foreach (string candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(candidate))
                continue;

            string relative = Path.GetRelativePath(texturesRoot, candidate).Replace('\\', '/');
            if (relative.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                relative = relative[..^4];
            return relative;
        }

        // last resort: search by filename anywhere under overlays/
        foreach (string file in Directory.EnumerateFiles(overlaysRoot, "*.png", SearchOption.AllDirectories))
        {
            string name = Path.GetFileNameWithoutExtension(file);
            if (name == $"{patternId}_mortar" || name == $"{patternId}_overlay")
            {
                string relative = Path.GetRelativePath(texturesRoot, file).Replace('\\', '/');
                if (relative.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    relative = relative[..^4];
                return relative;
            }
        }

        return null;
    }
}
