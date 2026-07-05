using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

namespace ingot.Core;

using Newtonsoft.Json;

using static ingot.Core.Common.JsonHelper;

using Version = Common.Version;

/// <summary>
/// C# representation of a Minecraft resource pack
/// </summary>
public class ResourcePack
{
    private readonly record struct TextureSource(string SourcePath, string RpName);
    private readonly record struct GeometrySource(string SourcePath, string RpName);

    /// <summary>
    /// Minecraft UUID to be used at runtime
    /// </summary>
    public string Uuid;
    /// <summary>
    /// Version of the <see cref="ResourcePack"/>. When <see cref="BehaviourPack"/> is linked, it will require at least this version.
    /// </summary>
    public Version ResourcePackVersion;
    /// <summary>
    /// Helper factory method to initiate API-style syntax
    /// </summary>
    /// <param name="uuid">Minecraft UUID to be used at runtime</param>
    /// <param name="version">Version of the <see cref="ResourcePack"/>. When <see cref="BehaviourPack"/> is linked, it will require at least this version.</param>
    public static ResourcePack Create(string uuid, Version? version = null) => new(uuid, version);

    /// <summary>
    /// Creates a <see cref="ResourcePack"/> with the given runtime UUID and optional version.
    /// </summary>
    /// <param name="uuid">Minecraft UUID to be used at runtime</param>
    /// <param name="version">Version of the <see cref="ResourcePack"/>. When <see cref="BehaviourPack"/> is linked, it will require at least this version.</param>
    public ResourcePack(string uuid, Version? version = null)
    {
        Uuid = uuid;
        ResourcePackVersion = version ?? new Version(1, 0, 0);
    }

    private readonly Dictionary<string, TextureSource> _blockTextureSources = new();
    private readonly Dictionary<string, TextureSource> _itemTextureSources = new();
    private readonly Dictionary<string, GeometrySource> _geometrySources = new();

    /// <summary>
    /// Registers a texture (PNG on disk) that will be copied into the resource pack under
    /// <c>textures/blocks/</c> and referenced from <c>terrain_texture.json</c>.
    /// The <paramref name="key"/> must match the texture name(s) used in your block
    /// <c>MaterialInstances</c> (or <c>IDestructionParticles</c>).
    /// </summary>
    /// <param name="key">The texture key (the value used in behaviour-side material instances).</param>
    /// <param name="sourcePngPath">Path to the source .png file on disk (will be copied as-is).</param>
    /// <param name="rpName">Optional filename (without extension) under <c>textures/blocks/</c>. Defaults to <paramref name="key"/>.</param>
    public ResourcePack AddBlockTexture(string key, string sourcePngPath, string? rpName = null)
    {
        RegisterTexture(_blockTextureSources, key, sourcePngPath, rpName);
        return this;
    }

    /// <summary>
    /// Registers a texture (PNG on disk) that will be copied into the resource pack under
    /// <c>textures/items/</c> and referenced from <c>item_texture.json</c>.
    /// The <paramref name="key"/> must match the <c>Texture</c> property on your <c>Item</c> definitions.
    /// </summary>
    /// <param name="key">The texture key (the value used in behaviour-side <c>minecraft:icon</c>).</param>
    /// <param name="sourcePngPath">Path to the source .png file on disk (will be copied as-is).</param>
    /// <param name="rpName">Optional filename (without extension) under <c>textures/items/</c>. Defaults to <paramref name="key"/>.</param>
    public ResourcePack AddItemTexture(string key, string sourcePngPath, string? rpName = null)
    {
        RegisterTexture(_itemTextureSources, key, sourcePngPath, rpName);
        return this;
    }

    /// <summary>
    /// Registers a block texture if the key has not already been added manually.
    /// </summary>
    /// <returns><see langword="true"/> when the texture was registered.</returns>
    internal bool TryAddBlockTexture(string key, string? sourcePngPath) =>
        TryRegisterTexture(_blockTextureSources, key, sourcePngPath);

    /// <summary>
    /// Registers an item texture if the key has not already been added manually.
    /// </summary>
    /// <returns><see langword="true"/> when the texture was registered.</returns>
    internal bool TryAddItemTexture(string key, string? sourcePngPath) =>
        TryRegisterTexture(_itemTextureSources, key, sourcePngPath);

    /// <summary>
    /// Registers a block geometry file (<c>.geo.json</c>) that will be copied into the resource pack under
    /// <c>models/blocks/</c>. The <paramref name="identifier"/> must match the geometry referenced from
    /// behaviour-side <c>minecraft:geometry</c> (for example <c>geometry.my_block</c>).
    /// </summary>
    /// <param name="identifier">The geometry identifier used in behaviour definitions.</param>
    /// <param name="sourceGeoJsonPath">Path to the source <c>.geo.json</c> file on disk (copied as-is).</param>
    /// <param name="rpName">Optional filename (without extension) under <c>models/blocks/</c>. Defaults to the last segment of <paramref name="identifier"/>.</param>
    public ResourcePack AddGeometry(string identifier, string sourceGeoJsonPath, string? rpName = null)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("geometry identifier cannot be empty", nameof(identifier));
        if (string.IsNullOrWhiteSpace(sourceGeoJsonPath))
            throw new ArgumentException("source geo json path cannot be empty", nameof(sourceGeoJsonPath));

        string resolvedRpName = rpName ?? ResolveGeometryRpName(identifier);
        _geometrySources[identifier] = new GeometrySource(Path.GetFullPath(sourceGeoJsonPath), resolvedRpName);
        return this;
    }

    private static void RegisterTexture(
        Dictionary<string, TextureSource> sources,
        string key,
        string sourcePngPath,
        string? rpName = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("texture key cannot be empty", nameof(key));
        if (string.IsNullOrWhiteSpace(sourcePngPath))
            throw new ArgumentException("source png path cannot be empty", nameof(sourcePngPath));

        sources[key] = new TextureSource(Path.GetFullPath(sourcePngPath), rpName ?? key);
    }

    private static bool TryRegisterTexture(
        Dictionary<string, TextureSource> sources,
        string key,
        string? sourcePngPath)
    {
        if (string.IsNullOrWhiteSpace(key) || sources.ContainsKey(key))
            return false;

        sources[key] = new TextureSource(
            string.IsNullOrWhiteSpace(sourcePngPath) ? string.Empty : Path.GetFullPath(sourcePngPath),
            key);
        return true;
    }

    /// <summary>
    /// Compiles the <see cref="ResourcePack"/> to output <paramref name="dir"/>
    /// </summary>
    /// <param name="dir">Output directory</param>
    public void Compile(string dir)
    {
        CompilerState.Push("rp");

        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(Path.Combine(dir, "entity"));
        Directory.CreateDirectory(Path.Combine(dir, "models"));
        Directory.CreateDirectory(Path.Combine(dir, "models", "blocks"));
        Directory.CreateDirectory(Path.Combine(dir, "textures"));
        Directory.CreateDirectory(Path.Combine(dir, "textures", "blocks"));
        Directory.CreateDirectory(Path.Combine(dir, "textures", "entity"));
        Directory.CreateDirectory(Path.Combine(dir, "textures", "items"));
        Directory.CreateDirectory(Path.Combine(dir, "textures", "particle"));
        Directory.CreateDirectory(Path.Combine(dir, "sounds"));
        Directory.CreateDirectory(Path.Combine(dir, "texts"));
        CompilerState.Info("created folder structure");

        string packName = CompilerState.CurrentPack?.Name ?? "ingot pack";

        if (_blockTextureSources.Count > 0)
            EmitTextureAtlas("terrain_texture.json", _blockTextureSources, "blocks", dir, packName, "atlas.terrain", 4, 8);

        EmitTextureAtlas("item_texture.json", _itemTextureSources, "items", dir, packName, "atlas.items");

        if (_geometrySources.Count > 0)
            EmitGeometries(dir);

        WriteBlocksJson(dir);
        WriteLanguageFiles(dir);
        WriteStubFiles(dir);

        CompilerState.Pop();
    }

    private static void WriteBlocksJson(string dir)
    {
        CompilerState.Push("blocks.json");

        if (CompilerState.CurrentPack is null)
        {
            CompilerState.Info("no pack context, skipping blocks.json");
            CompilerState.Pop();
            return;
        }

        IReadOnlyList<Block> blocks = CompilerState.CurrentPack.BehaviourPack.Blocks;
        if (blocks.Count == 0)
        {
            CompilerState.Info("no blocks to compile");
            CompilerState.Pop();
            return;
        }

        using StringWriter sw = new();
        JsonTextWriter w = new(sw)
        {
            Formatting = Formatting.Indented,
            Indentation = 4,
        };

        JsonHelper json = new(ref w);

        w.WriteStartObject();
        json.Property("format_version", new[] { 1, 1, 0 });

        foreach (Block block in blocks)
        {
            if (block.ResourceTexture is null && block.Sound is null)
                continue;

            json.Object(block.Identifier.ToString(), () =>
            {
                json.Property("sound", block.Sound);
                json.Property("textures", block.ResourceTexture);
            });
        }

        w.WriteEndObject();
        File.WriteAllText(Path.Combine(dir, "blocks.json"), sw.ToString());
        CompilerState.Info($"wrote blocks.json with {blocks.Count} entries");
        CompilerState.Pop();
    }

    private static void WriteLanguageFiles(string dir)
    {
        CompilerState.Push("texts");

        if (CompilerState.CurrentPack is null)
        {
            CompilerState.Info("no pack context, skipping language files");
            CompilerState.Pop();
            return;
        }

        List<string> langEntries = new();

        foreach (Block block in CompilerState.CurrentPack.BehaviourPack.Blocks)
        {
            if (block.LangName is not null)
                langEntries.Add($"tile.{block.Identifier}.name={block.LangName}");
        }

        foreach (Behaviour.Item item in CompilerState.CurrentPack.BehaviourPack.Items)
        {
            if (item.DisplayName is not null)
                langEntries.Add($"item.{item.Identifier}.name={item.DisplayName}");
        }

        File.WriteAllText(
            Path.Combine(dir, "texts", "languages.json"),
            JsonConvert.SerializeObject(new[] { "en_US" }, Formatting.Indented) + Environment.NewLine);

        if (langEntries.Count > 0)
        {
            File.WriteAllText(Path.Combine(dir, "texts", "en_US.lang"), string.Join('\n', langEntries) + '\n');
            CompilerState.Info($"wrote en_US.lang with {langEntries.Count} entries");
        }
        else
        {
            File.WriteAllText(Path.Combine(dir, "texts", "en_US.lang"), string.Empty);
            CompilerState.Info("wrote empty en_US.lang");
        }

        CompilerState.Pop();
    }

    private static void WriteStubFiles(string dir)
    {
        CompilerState.Push("stubs");

        File.WriteAllText(Path.Combine(dir, "biomes_client.json"), "{\n\t\"biomes\": {}\n}\n");
        File.WriteAllText(Path.Combine(dir, "splashes.json"), "{\n\t\"canMerge\": false,\n\t\"splashes\": []\n}\n");
        File.WriteAllText(Path.Combine(dir, "sounds.json"), "{}\n");
        File.WriteAllText(
            Path.Combine(dir, "sounds", "sound_definitions.json"),
            "{\n\t\"format_version\": \"1.14.0\",\n\t\"sound_definitions\": {}\n}\n");
        File.WriteAllText(Path.Combine(dir, "textures", "flipbook_textures.json"), "[]\n");

        CompilerState.Info("wrote resource pack stub files");
        CompilerState.Pop();
    }

    private static string ResolveGeometryRpName(string identifier)
    {
        string normalized = identifier.Trim();
        const string minecraftPrefix = "minecraft:geometry.";
        const string geometryPrefix = "geometry.";

        if (normalized.StartsWith(minecraftPrefix, StringComparison.Ordinal))
            normalized = normalized[minecraftPrefix.Length..];
        else if (normalized.StartsWith(geometryPrefix, StringComparison.Ordinal))
            normalized = normalized[geometryPrefix.Length..];

        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException($"geometry identifier '{identifier}' does not contain a usable file name", nameof(identifier));

        return normalized;
    }

    private void EmitGeometries(string outputDir)
    {
        CompilerState.Push("geometries");

        string targetDir = Path.Combine(outputDir, "models", "blocks");
        Directory.CreateDirectory(targetDir);

        JsonTextWriter? dummyWriter = null;
        int c = 0;
        foreach (var (identifier, source) in _geometrySources)
        {
            c++;
            string targetFull = Path.Combine(targetDir, $"{source.RpName}.geo.json");

            try
            {
                if (!File.Exists(source.SourcePath))
                {
                    CompilerState.Warn(ref dummyWriter, $"source geometry not found for identifier '{identifier}': {source.SourcePath}");
                    continue;
                }

                File.Copy(source.SourcePath, targetFull, overwrite: true);
                CompilerState.Info($"({c}/{_geometrySources.Count}) registered geometry '{identifier}' -> models/blocks/{source.RpName}.geo.json");
            }
            catch (Exception ex)
            {
                CompilerState.Warn(ref dummyWriter, $"failed to process geometry identifier '{identifier}': {ex.Message}");
            }
        }

        CompilerState.Info($"wrote {_geometrySources.Count} geometry file(s)");
        CompilerState.Pop();
    }

    private static void EmitTextureAtlas(
        string atlasFileName,
        Dictionary<string, TextureSource> sources,
        string subdir,
        string outputDir,
        string resourcePackName,
        string textureName,
        int? numMipLevels = null,
        int? padding = null)
    {
        CompilerState.Push(atlasFileName);

        string texturesRoot = Path.Combine(outputDir, "textures");
        string targetSubdir = Path.Combine(texturesRoot, subdir);
        Directory.CreateDirectory(targetSubdir);

        Dictionary<string, object> textureDataEntries = new();
        JsonTextWriter? dummyWriter = null;
        int c = 0;
        foreach (var (key, source) in sources)
        {
            c++;
            string rpPath = $"textures/{subdir}/{source.RpName}";
            string targetFull = Path.Combine(targetSubdir, $"{source.RpName}.png");

            try
            {
                if (string.IsNullOrWhiteSpace(source.SourcePath))
                    CompilerState.Warn(ref dummyWriter, $"texture key '{key}' has no source PNG registered");
                else if (!File.Exists(source.SourcePath))
                    CompilerState.Warn(ref dummyWriter, $"source texture not found for key '{key}': {source.SourcePath}");
                else
                    File.Copy(source.SourcePath, targetFull, overwrite: true);

                textureDataEntries[key] = new { textures = rpPath };
                CompilerState.Info($"({c}/{sources.Count}) registered texture '{key}' -> {rpPath}");
            }
            catch (Exception ex)
            {
                CompilerState.Warn(ref dummyWriter, $"failed to process texture key '{key}': {ex.Message}");
            }
        }

        using (StringWriter sw = new())
        {
            JsonTextWriter w = new(sw);
            w.Formatting = Formatting.Indented;
            w.Indentation = 4;

            JsonHelper json = new(ref w);

            w.WriteStartObject();
            if (numMipLevels is not null)
                json.Property("num_mip_levels", numMipLevels);
            if (padding is not null)
                json.Property("padding", padding);
            json.Property("resource_pack_name", resourcePackName);
            json.Property("texture_name", textureName);
            json.Object("texture_data", () =>
            {
                foreach (var kvp in textureDataEntries)
                    json.Property(kvp.Key, kvp.Value);
            });
            w.WriteEndObject();

            string outPath = Path.Combine(texturesRoot, atlasFileName);
            File.WriteAllText(outPath, sw.ToString());
        }

        CompilerState.Info($"wrote {atlasFileName} with {textureDataEntries.Count} entries");
        CompilerState.Pop();
    }
}