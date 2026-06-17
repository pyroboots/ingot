using ingot.Core.Common;

namespace ingot.Core;

using Newtonsoft.Json;
using Version = Common.Version;
using static ingot.Core.Common.JsonHelper;

/// <summary>
/// C# representation of a Minecraft resource pack
/// </summary>
public class ResourcePack
{
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
    public ResourcePack(string uuid, Version? version = null)
    {
        Uuid = uuid;
        ResourcePackVersion = version ?? new Version(1, 0, 0);
    }

    private readonly Dictionary<string, string> _blockTextureSources = new();
    private readonly Dictionary<string, string> _itemTextureSources = new();

    /// <summary>
    /// Registers a texture (PNG on disk) that will be copied into the resource pack under
    /// <c>textures/blocks/</c> and referenced from <c>terrain_texture.json</c>.
    /// The <paramref name="key"/> must match the texture name(s) used in your block
    /// <c>MaterialInstances</c> (or <c>IDestructionParticles</c>).
    /// </summary>
    /// <param name="key">The texture key (the value used in behaviour-side material instances).</param>
    /// <param name="sourcePngPath">Path to the source .png file on disk (will be copied as-is).</param>
    public ResourcePack AddBlockTexture(string key, string sourcePngPath)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("texture key cannot be empty", nameof(key));
        if (string.IsNullOrWhiteSpace(sourcePngPath))
            throw new ArgumentException("source png path cannot be empty", nameof(sourcePngPath));

        _blockTextureSources[key] = Path.GetFullPath(sourcePngPath);
        return this;
    }

    /// <summary>
    /// Registers a texture (PNG on disk) that will be copied into the resource pack under
    /// <c>textures/items/</c> and referenced from <c>item_texture.json</c>.
    /// The <paramref name="key"/> must match the <c>Texture</c> property on your <c>Item</c> definitions.
    /// </summary>
    /// <param name="key">The texture key (the value used in behaviour-side <c>minecraft:icon</c>).</param>
    /// <param name="sourcePngPath">Path to the source .png file on disk (will be copied as-is).</param>
    public ResourcePack AddItemTexture(string key, string sourcePngPath)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("texture key cannot be empty", nameof(key));
        if (string.IsNullOrWhiteSpace(sourcePngPath))
            throw new ArgumentException("source PNG path cannot be empty", nameof(sourcePngPath));

        _itemTextureSources[key] = Path.GetFullPath(sourcePngPath);
        return this;
    }

    /// <summary>
    /// Registers a block texture if the key has not already been added manually.
    /// </summary>
    /// <returns><see langword="true"/> when the texture was registered.</returns>
    internal bool TryAddBlockTexture(string key, string? sourcePngPath)
    {
        if (string.IsNullOrWhiteSpace(key) || _blockTextureSources.ContainsKey(key))
            return false;

        _blockTextureSources[key] = string.IsNullOrWhiteSpace(sourcePngPath)
            ? string.Empty
            : Path.GetFullPath(sourcePngPath);
        return true;
    }

    /// <summary>
    /// Registers an item texture if the key has not already been added manually.
    /// </summary>
    /// <returns><see langword="true"/> when the texture was registered.</returns>
    internal bool TryAddItemTexture(string key, string? sourcePngPath)
    {
        if (string.IsNullOrWhiteSpace(key) || _itemTextureSources.ContainsKey(key))
            return false;

        _itemTextureSources[key] = string.IsNullOrWhiteSpace(sourcePngPath)
            ? string.Empty
            : Path.GetFullPath(sourcePngPath);
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
        Directory.CreateDirectory(Path.Combine(dir, "textures"));
            Directory.CreateDirectory(Path.Combine(dir, "textures", "blocks"));
            Directory.CreateDirectory(Path.Combine(dir, "textures", "entity"));
            Directory.CreateDirectory(Path.Combine(dir, "textures", "items"));
            Directory.CreateDirectory(Path.Combine(dir, "textures", "particle"));
        CompilerState.Info("created folder structure");

        EmitTextureAtlas("terrain_texture.json", _blockTextureSources, "blocks", dir);
        EmitTextureAtlas("item_texture.json", _itemTextureSources, "items", dir);

        // TODO: models, particles, sounds, entity resources, flipbooks, etc.
        CompilerState.Pop();
    }

    private static void EmitTextureAtlas(string atlasFileName, Dictionary<string, string> sources, string subdir, string outputDir)
    {
        CompilerState.Push(atlasFileName);

        if (sources.Count == 0)
        {
            CompilerState.Info("no textures to compile");
            CompilerState.Pop();
            return;
        }

        string texturesRoot = Path.Combine(outputDir, "textures");
        string targetSubdir = Path.Combine(texturesRoot, subdir);
        Directory.CreateDirectory(targetSubdir);

        // make texture_data entries while copying assets
        Dictionary<string, object> textureDataEntries = new();
        JsonTextWriter? dummyWriter = null;
        int c = 0;
        foreach (var (key, srcPath) in sources)
        {
            c++;
            string rpPath = $"textures/{subdir}/{key}"; // no extension per bedrock convention
            string targetFull = Path.Combine(targetSubdir, $"{key}.png");

            try
            {
                if (string.IsNullOrWhiteSpace(srcPath))
                    CompilerState.Warn(ref dummyWriter, $"texture key '{key}' has no source PNG registered");
                else if (!File.Exists(srcPath))
                    CompilerState.Warn(ref dummyWriter, $"source texture not found for key '{key}': {srcPath}");
                else
                    File.Copy(srcPath, targetFull, overwrite: true);

                textureDataEntries[key] = new { textures = rpPath };
                CompilerState.Info($"({c}/{sources.Count}) registered texture '{key}' -> {rpPath}");
            }
            catch (Exception ex)
            {
                CompilerState.Warn(ref dummyWriter, $"failed to process texture key '{key}': {ex.Message}");
            }
        }

        // write the atlas json
        using (StringWriter sw = new())
        {
            JsonTextWriter w = new(sw);
            w.Formatting = Formatting.Indented;
            w.Indentation = 4;

            JsonHelper json = new(ref w);
            
            w.WriteStartObject();
            json.Object("texture_data", () =>
            {
                foreach (var kvp in textureDataEntries)
                {
                    json.Property(kvp.Key, kvp.Value);
                }
            });
            w.WriteEndObject();

            string outPath = Path.Combine(texturesRoot, atlasFileName);
            File.WriteAllText(outPath, sw.ToString());
        }

        CompilerState.Info($"wrote {atlasFileName} with {textureDataEntries.Count} entries");
        CompilerState.Pop();
    }
}