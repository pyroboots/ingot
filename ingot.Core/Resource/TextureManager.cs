using ingot.Core.Common;

using Newtonsoft.Json;

namespace ingot.Core.Resource;

/// <summary>
/// Texture atlases, loose entity/particle PNGs, and flipbook definitions for a resource pack.
/// </summary>
public class TextureManager(ResourcePack pack)
{
    internal readonly record struct TextureSource(string SourcePath, string RpName);

    /// <summary>
    /// Terrain atlas written to <c>textures/terrain_texture.json</c> with PNGs under <c>textures/blocks/</c>.
    /// </summary>
    public TextureAtlas BlockTextureAtlas { get; } = new(pack, "atlas.terrain", "blocks", includeMipSettings: true);

    /// <summary>
    /// Item atlas written to <c>textures/item_texture.json</c> with PNGs under <c>textures/items/</c>.
    /// </summary>
    public TextureAtlas ItemTextureAtlas { get; } = new(pack, "atlas.items", "items");

    /// <summary>
    /// Flipbook (animated) texture entries written to <c>textures/flipbook_textures.json</c>.
    /// </summary>
    public FlipbookTextures Flipbook { get; } = new();

    internal readonly Dictionary<string, TextureSource> EntityTextures = new();
    internal readonly Dictionary<string, TextureSource> ParticleTextures = new();

    /// <summary>
    /// Particle texture keys registered under <c>textures/particle/</c>.
    /// </summary>
    public IReadOnlyCollection<string> ParticleTextureKeys => ParticleTextures.Keys;

    /// <summary>
    /// Entity texture keys registered under <c>textures/entity/</c>.
    /// </summary>
    public IReadOnlyCollection<string> EntityTextureKeys => EntityTextures.Keys;

    /// <summary>
    /// Registers a block texture copied under <c>textures/blocks/</c> and referenced from the terrain atlas.
    /// </summary>
    public void AddBlockTexture(string key, string sourcePngPath, string? rpName = null) =>
        BlockTextureAtlas.Add(key, sourcePngPath, rpName);

    /// <summary>
    /// Registers an item texture copied under <c>textures/items/</c> and referenced from the item atlas.
    /// </summary>
    public void AddItemTexture(string key, string sourcePngPath, string? rpName = null) =>
        ItemTextureAtlas.Add(key, sourcePngPath, rpName);

    /// <summary>
    /// Registers an entity texture copied under <c>textures/entity/</c>.
    /// </summary>
    public void AddEntityTexture(string key, string sourcePngPath, string? rpName = null) =>
        RegisterLooseTexture(EntityTextures, key, sourcePngPath, rpName);

    /// <summary>
    /// Registers a particle texture copied under <c>textures/particle/</c>.
    /// </summary>
    public void AddParticleTexture(string key, string sourcePngPath, string? rpName = null) =>
        RegisterLooseTexture(ParticleTextures, key, sourcePngPath, rpName);

    internal bool TryAddBlockTexture(string key, string? sourcePngPath) =>
        BlockTextureAtlas.TryAdd(key, sourcePngPath);

    internal bool TryAddItemTexture(string key, string? sourcePngPath) =>
        ItemTextureAtlas.TryAdd(key, sourcePngPath);

    internal bool TryAddEntityTexture(string key, string? sourcePngPath) =>
        TryRegisterLooseTexture(EntityTextures, key, sourcePngPath);

    internal IEnumerable<ResourceCopy> EnumerateEntityCopies() =>
        EnumerateLooseCopies(EntityTextures, "textures/entity", "entity texture");

    internal IEnumerable<ResourceCopy> EnumerateParticleCopies() =>
        EnumerateLooseCopies(ParticleTextures, "textures/particle", "particle texture");

    private static void RegisterLooseTexture(
        Dictionary<string, TextureSource> sources,
        string key,
        string sourcePngPath,
        string? rpName)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("texture key cannot be empty", nameof(key));
        if (string.IsNullOrWhiteSpace(sourcePngPath))
            throw new ArgumentException("source png path cannot be empty", nameof(sourcePngPath));

        sources[key] = new TextureSource(Path.GetFullPath(sourcePngPath), rpName ?? key);
    }

    private static bool TryRegisterLooseTexture(
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

    private static IEnumerable<ResourceCopy> EnumerateLooseCopies(
        Dictionary<string, TextureSource> sources,
        string folder,
        string kind)
    {
        foreach ((string key, TextureSource source) in sources)
        {
            string rpRelative = source.RpName.Replace('\\', '/').Trim('/');
            yield return new ResourceCopy(
                source.SourcePath,
                $"{folder}/{rpRelative}.png",
                key,
                kind);
        }
    }
}

internal class TextureAtlasJsonConverter : JsonConverter<TextureAtlas>
{
    public override void WriteJson(JsonWriter writer, TextureAtlas? value, JsonSerializer serializer)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));

        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            if (value.IncludeMipSettings)
            {
                json.Property("num_mip_levels", value.NumMipLevels);
                json.Property("padding", value.Padding);
            }

            json.Property("resource_pack_name", value.ResourcePackName);
            json.Property("texture_name", value.TextureName);
            json.Object("texture_data", () =>
            {
                foreach (AtlasTextureCollection collection in value.TextureData)
                    serializer.Serialize(json.Writer, collection);
            });
        });
    }

    public override TextureAtlas? ReadJson(JsonReader reader, Type objectType, TextureAtlas? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        throw new InvalidOperationException();
    }
}

/// <summary>
/// A Minecraft texture atlas (<c>terrain_texture.json</c> / <c>item_texture.json</c>).
/// </summary>
[JsonConverter(typeof(TextureAtlasJsonConverter))]
public class TextureAtlas
{
    private readonly ResourcePack _pack;

    internal TextureAtlas(ResourcePack pack, string atlasName, string subdirectory, bool includeMipSettings = false)
    {
        _pack = pack;
        TextureName = atlasName;
        Subdirectory = subdirectory;
        IncludeMipSettings = includeMipSettings;
    }

    /// <summary>
    /// Name of the resource pack this texture atlas belongs to
    /// </summary>
    public string ResourcePackName =>
        !string.IsNullOrWhiteSpace(_pack.Name)
            ? _pack.Name
            : CompilerState.CurrentPack?.Name ?? "ingot pack";

    /// <summary>
    /// Name of the texture atlas. Typically prefixed with <c>atlas.</c>
    /// </summary>
    public readonly string TextureName;

    /// <summary>
    /// Folder under <c>textures/</c> that PNG files are copied into.
    /// </summary>
    public readonly string Subdirectory;

    internal readonly bool IncludeMipSettings;

    internal readonly Dictionary<string, TextureManager.TextureSource> Sources = new();

    /// <summary>
    /// List of <see cref="AtlasTextureCollection"/>s
    /// </summary>
    public List<AtlasTextureCollection> TextureData { get; } = [];

    /// <summary>
    /// Refers to the stretched out area around textures that prevents them from bleeding into each other due to imprecise rendering
    /// </summary>
    /// <exception cref="Exception"><c>padding must be at least 2^(n-1) where n represents number of mip levels</c></exception>
    public int Padding
    {
        get; set
        {
            double min = Math.Pow(2, NumMipLevels - 1);
            if (min > value)
                throw new Exception($"padding must be at least 2^(n-1) where n represents number of mip levels ({NumMipLevels})" +
                                    $" - minimum padding level for the current number of mip levels is {min}");

            if (int.IsPow2(value) == false)
                CompilerState.Warn("padding recommended to be a power of 2");
            field = value;
        }
    } = 8;

    /// <summary>
    /// Used by Minecraft to reduce the resolution of textures as they get further away from the camera
    /// </summary>
    public int NumMipLevels
    {
        get; set
        {
            if (value > 4)
                CompilerState.Warn("effect of more than 4 mip map levels is negligible for 16x16 textures" +
                                   " - if you have higher texture resolutions, ignore this warning");

            if (int.IsPow2(value) == false)
                CompilerState.Warn("number of mip map levels recommended to be a power of 2");
            field = value;
        }
    } = 4;

    /// <summary>
    /// Adds a texture shortname that maps <paramref name="key"/> to a PNG copied under <c>textures/{Subdirectory}/</c>.
    /// Overwrites a previous registration for the same key.
    /// </summary>
    public void Add(string key, string sourcePngPath, string? rpName = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("texture key cannot be empty", nameof(key));
        if (string.IsNullOrWhiteSpace(sourcePngPath))
            throw new ArgumentException("source png path cannot be empty", nameof(sourcePngPath));

        Register(key, Path.GetFullPath(sourcePngPath), rpName ?? key);
    }

    internal bool TryAdd(string key, string? sourcePngPath)
    {
        if (string.IsNullOrWhiteSpace(key) || Sources.ContainsKey(key))
            return false;

        Register(
            key,
            string.IsNullOrWhiteSpace(sourcePngPath) ? string.Empty : Path.GetFullPath(sourcePngPath),
            key);
        return true;
    }

    private void Register(string key, string sourcePath, string rpName)
    {
        Sources[key] = new TextureManager.TextureSource(sourcePath, rpName);
        string packPath = $"textures/{Subdirectory}/{rpName.Replace('\\', '/').Trim('/')}";

        int existing = TextureData.FindIndex(c => c.Identifier == key);
        AtlasTextureCollection collection = new(key) { Textures = packPath };
        if (existing >= 0)
            TextureData[existing] = collection;
        else
            TextureData.Add(collection);
    }

    internal IEnumerable<ResourceCopy> EnumerateCopies()
    {
        foreach ((string key, TextureManager.TextureSource source) in Sources)
        {
            string rpRelative = source.RpName.Replace('\\', '/').Trim('/');
            yield return new ResourceCopy(
                source.SourcePath,
                $"textures/{Subdirectory}/{rpRelative}.png",
                key,
                "texture");
        }
    }
}

/// <summary>
/// A single texture layer inside an <see cref="AtlasTextureCollection"/>.
/// </summary>
[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public struct AtlasTexture(string packPath, string? sourcePath = null)
{
    /// <summary>
    /// File path to a PNG on disk. Copied into the pack when set.
    /// </summary>
    [JsonIgnore] public readonly string? SourcePath = sourcePath;
    /// <summary>
    /// Path written to the atlas (no extension), e.g. <c>textures/blocks/stone</c>.
    /// </summary>
    [JsonProperty("path")] public string PackPath = packPath;
    /// <summary>
    /// Whether this texture occupies a 2x2 quad in the atlas.
    /// </summary>
    [JsonProperty("quad")] public bool Quad = false;
    /// <summary>
    /// Tint applied to the texture.
    /// </summary>
    [JsonProperty("tint_color")] public string? TintColor = null;
    /// <summary>
    /// Overlay color applied to the texture.
    /// </summary>
    [JsonProperty("overlay_color")] public string? OverlayColor = null;
}

internal class AtlasTextureCollectionJsonConverter : JsonConverter<AtlasTextureCollection>
{
    public override void WriteJson(JsonWriter writer, AtlasTextureCollection value, JsonSerializer serializer)
    {
        JsonHelper json = new(ref writer);
        json.Object(value.Identifier, () =>
        {
            if (value.Additive)
                json.Property("additive", value.Additive);

            writer.WritePropertyName("textures");
            serializer.Serialize(writer, value.Textures.Value);
        });
    }

    public override AtlasTextureCollection ReadJson(JsonReader reader, Type objectType, AtlasTextureCollection existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        throw new InvalidOperationException();
    }
}

/// <summary>
/// A named shortname in a texture atlas, mapping to one or more textures.
/// </summary>
[JsonConverter(typeof(AtlasTextureCollectionJsonConverter))]
public struct AtlasTextureCollection(string identifier)
{
    /// <summary>
    /// Identifier of the texture shortname
    /// </summary>
    public readonly string Identifier = identifier;
    /// <summary>
    /// Layers the specified textures on top of each other to create a new combined texture.
    /// Translucent texels fully override previous layers.
    /// Overlay color only works when placed in the first textures entry and affects all layers
    /// </summary>
    public bool Additive = false;
    /// <summary>
    /// The textures this shortname encapsulates
    /// </summary>
    public Either<AtlasTexture, AtlasTexture[], string, string[]> Textures = "";
}

/// <summary>
/// Represents <c>textures/flipbook_textures.json</c> in the resource pack.
/// </summary>
public class FlipbookTextures : List<FlipbookTexture>;

/// <summary>
/// A single animated (flipbook) texture entry.
/// </summary>
public class FlipbookTexture
{
    /// <summary>
    /// Path of the flipbook strip (no extension), e.g. <c>textures/blocks/fire_0</c>.
    /// </summary>
    [JsonProperty("flipbook_texture")]
    public required string Texture { get; set; }

    /// <summary>
    /// Atlas shortname this animation replaces.
    /// </summary>
    [JsonProperty("atlas_tile")]
    public required string AtlasTile { get; set; }

    /// <summary>
    /// Ticks between frames.
    /// </summary>
    [JsonProperty("ticks_per_frame")]
    public int TicksPerFrame { get; set; } = 1;

    /// <summary>
    /// Whether frames are blended while animating.
    /// </summary>
    [JsonProperty("blend_frames")]
    public bool? BlendFrames { get; set; }

    /// <summary>
    /// How many times to replicate the texture in the atlas.
    /// </summary>
    [JsonProperty("replicate")]
    public int? Replicate { get; set; }
}
