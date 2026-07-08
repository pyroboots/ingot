using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Entity;
using ingot.Core.Behaviour.Item;
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
    private readonly Dictionary<string, TextureSource> _entityTextureSources = new();
    private readonly Dictionary<string, GeometrySource> _geometrySources = new();
    private readonly List<ClientEntity> _clientEntities = new();
    private readonly List<RenderController> _renderControllers = new();
    private readonly HashSet<string> _registeredRenderControllerIds = new(StringComparer.Ordinal);

    /// <summary>
    /// Client entity definitions registered on this pack.
    /// </summary>
    public IReadOnlyList<ClientEntity> ClientEntities => _clientEntities;

    /// <summary>
    /// Render controllers registered on this pack.
    /// </summary>
    public IReadOnlyList<RenderController> RenderControllers => _renderControllers;

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

    /// <summary>
    /// Registers an entity texture (PNG on disk) that will be copied into the resource pack under
    /// <c>textures/entity/</c>. The path used in client-entity short-names should be
    /// <c>textures/entity/{rpName}</c>.
    /// </summary>
    /// <param name="key">Relative path under <c>textures/entity/</c> (e.g. <c>my_mob</c> or <c>spider/cave_spider</c>).</param>
    /// <param name="sourcePngPath">Path to the source .png file on disk (will be copied as-is).</param>
    /// <param name="rpName">Optional relative path under <c>textures/entity/</c>. Defaults to <paramref name="key"/>.</param>
    public ResourcePack AddEntityTexture(string key, string sourcePngPath, string? rpName = null)
    {
        RegisterTexture(_entityTextureSources, key, sourcePngPath, rpName);
        return this;
    }

    /// <summary>
    /// Registers an entity texture if the key has not already been added manually.
    /// </summary>
    /// <returns><see langword="true"/> when the texture was registered.</returns>
    internal bool TryAddEntityTexture(string key, string? sourcePngPath) =>
        TryRegisterTexture(_entityTextureSources, key, sourcePngPath);

    /// <summary>
    /// Adds a client entity definition to the resource pack.
    /// </summary>
    public ResourcePack AddClientEntity<TClientEntity>() where TClientEntity : ClientEntity, new() =>
        AddClientEntity(typeof(TClientEntity));

    /// <summary>
    /// Adds a client entity definition to the resource pack.
    /// </summary>
    public ResourcePack AddClientEntity(Type tClientEntity)
    {
        ClientEntity inst = (Activator.CreateInstance(tClientEntity) as ClientEntity)!;
        _clientEntities.Add(inst);
        return this;
    }

    /// <summary>
    /// Adds a render controller to the resource pack.
    /// </summary>
    public ResourcePack AddRenderController<TRenderController>() where TRenderController : RenderController, new() =>
        AddRenderController(typeof(TRenderController));

    /// <summary>
    /// Adds a render controller to the resource pack.
    /// </summary>
    public ResourcePack AddRenderController(Type tRenderController)
    {
        RenderController inst = (Activator.CreateInstance(tRenderController) as RenderController)!;
        RegisterRenderControllerInstance(inst);
        return this;
    }

    /// <summary>
    /// Adds a pre-built render controller instance to the resource pack.
    /// </summary>
    public ResourcePack AddRenderController(RenderController controller)
    {
        RegisterRenderControllerInstance(controller);
        return this;
    }

    private void RegisterRenderControllerInstance(RenderController inst)
    {
        _renderControllers.Add(inst);
        _registeredRenderControllerIds.Add(inst.ControllerId);
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
        Directory.CreateDirectory(Path.Combine(dir, "render_controllers"));
        Directory.CreateDirectory(Path.Combine(dir, "models"));
        Directory.CreateDirectory(Path.Combine(dir, "models", "blocks"));
        Directory.CreateDirectory(Path.Combine(dir, "models", "entity"));
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

        // Client entities may auto-register entity textures during compile.
        EmitClientEntities(dir);

        if (_entityTextureSources.Count > 0)
            EmitEntityTextures(dir);

        EmitRenderControllers(dir);

        WriteBlocksJson(dir);
        WriteLanguageFiles(dir);
        WriteStubFiles(dir);
        WriteEntitySoundsJson(dir);

        CompilerState.Pop();
    }

    private void EmitClientEntities(string dir)
    {
        if (_clientEntities.Count == 0)
            return;

        CompilerState.Push("entity");
        CompilerState.Info("compiling client entities...");

        string entityDir = Path.Combine(dir, "entity");
        Directory.CreateDirectory(entityDir);

        int c = 0;
        foreach (ClientEntity clientEntity in _clientEntities)
        {
            c++;
            Type tType = clientEntity.GetType();
            string json = ClientEntity.Compile(tType);
            string path = Path.Combine(entityDir, $"{clientEntity.Identifier.Name}.json");
            File.WriteAllText(path, json);
            CompilerState.Info($"({c}/{_clientEntities.Count}) compiled client entity {clientEntity.Identifier}");
        }

        CompilerState.Pop();
    }

    private void EmitRenderControllers(string dir)
    {
        CompilerState.Push("render_controllers");

        string rcDir = Path.Combine(dir, "render_controllers");
        Directory.CreateDirectory(rcDir);

        // Explicitly registered controllers
        int c = 0;
        foreach (RenderController controller in _renderControllers)
        {
            c++;
            string json = RenderController.CompileInstance(controller);
            string path = Path.Combine(rcDir, $"{controller.FileName}.json");
            File.WriteAllText(path, json);
            CompilerState.Info($"({c}/{_renderControllers.Count}) compiled render controller {controller.ControllerId}");
        }

        // Auto-emit simple controllers referenced by client entities but not registered
        HashSet<string> emittedIds = new(_registeredRenderControllerIds, StringComparer.Ordinal);
        int autoCount = 0;
        foreach (ClientEntity clientEntity in _clientEntities)
        {
            if (!clientEntity.EmitDefaultRenderController)
                continue;

            foreach (string controllerId in clientEntity.RenderControllers)
            {
                if (string.IsNullOrWhiteSpace(controllerId) || emittedIds.Contains(controllerId))
                    continue;

                // Only auto-emit controller.render.* ids; leave vanilla/reused controllers alone
                // when they don't start with controller.render. - actually always skip if looks like
                // a well-known reuse without a custom definition. Auto-emit any unregistered id
                // that starts with controller.render. so simple entities work out of the box.
                if (!controllerId.StartsWith("controller.render.", StringComparison.Ordinal))
                    continue;

                RenderController simple = RenderController.CreateSimple(controllerId);
                string json = RenderController.CompileInstance(simple);
                string path = Path.Combine(rcDir, $"{simple.FileName}.json");
                File.WriteAllText(path, json);
                emittedIds.Add(controllerId);
                autoCount++;
                CompilerState.Info($"auto-emitted default render controller {controllerId}");
            }
        }

        if (_renderControllers.Count == 0 && autoCount == 0)
            CompilerState.Info("no render controllers to compile");
        else if (autoCount > 0)
            CompilerState.Info($"wrote {_renderControllers.Count} registered + {autoCount} auto-emitted render controller(s)");

        CompilerState.Pop();
    }

    private void EmitEntityTextures(string outputDir)
    {
        CompilerState.Push("entity textures");

        string texturesRoot = Path.Combine(outputDir, "textures", "entity");
        Directory.CreateDirectory(texturesRoot);

        int c = 0;
        foreach (var (key, source) in _entityTextureSources)
        {
            c++;
            string rpRelative = source.RpName.Replace('\\', '/').Trim('/');
            string targetFull = Path.Combine(texturesRoot, $"{rpRelative}.png");
            string? targetDir = Path.GetDirectoryName(targetFull);
            if (!string.IsNullOrEmpty(targetDir))
                Directory.CreateDirectory(targetDir);

            if (string.IsNullOrWhiteSpace(source.SourcePath))
                throw new ArgumentException($"entity texture key '{key}' has no source PNG registered");
            if (!File.Exists(source.SourcePath))
                throw new FileNotFoundException(
                    $"source entity texture not found for key '{key}': {source.SourcePath}",
                    source.SourcePath);

            File.Copy(source.SourcePath, targetFull, overwrite: true);
            CompilerState.Info($"({c}/{_entityTextureSources.Count}) registered entity texture '{key}' -> textures/entity/{rpRelative}.png");
        }

        CompilerState.Info($"wrote {_entityTextureSources.Count} entity texture(s)");
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

        foreach (Item item in CompilerState.CurrentPack.BehaviourPack.Items)
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
        // sounds.json is written by WriteEntitySoundsJson (may include entity_sounds)
        File.WriteAllText(
            Path.Combine(dir, "sounds", "sound_definitions.json"),
            "{\n\t\"format_version\": \"1.14.0\",\n\t\"sound_definitions\": {}\n}\n");
        File.WriteAllText(Path.Combine(dir, "textures", "flipbook_textures.json"), "[]\n");

        CompilerState.Info("wrote resource pack stub files");
        CompilerState.Pop();
    }

    private void WriteEntitySoundsJson(string dir)
    {
        CompilerState.Push("sounds.json");

        Dictionary<string, object> entities = new();
        foreach (ClientEntity clientEntity in _clientEntities)
        {
            ClientEntitySounds? sounds = clientEntity.EntitySounds;
            if (sounds is null || sounds.Events.Count == 0)
                continue;

            Dictionary<string, object> entry = new()
            {
                ["volume"] = sounds.Volume,
                ["events"] = sounds.Events,
            };
            if (sounds.Pitch is { Length: > 0 })
                entry["pitch"] = sounds.Pitch.Length == 1 ? sounds.Pitch[0] : sounds.Pitch;

            entities[clientEntity.Identifier.ToString()] = entry;
        }

        using StringWriter sw = new();
        JsonTextWriter w = new(sw)
        {
            Formatting = Formatting.Indented,
            Indentation = 4,
        };
        JsonHelper json = new(ref w);

        w.WriteStartObject();
        if (entities.Count > 0)
        {
            json.Object("entity_sounds", () =>
            {
                json.Object("entities", () =>
                {
                    foreach (var kvp in entities)
                        json.Property(kvp.Key, kvp.Value);
                });
            });
        }
        w.WriteEndObject();

        File.WriteAllText(Path.Combine(dir, "sounds.json"), sw.ToString());
        CompilerState.Info(
            entities.Count > 0
                ? $"wrote sounds.json with {entities.Count} entity sound mapping(s)"
                : "wrote empty sounds.json");
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

        int c = 0;
        foreach (var (identifier, source) in _geometrySources)
        {
            c++;
            string targetFull = Path.Combine(targetDir, $"{source.RpName}.geo.json");

            if (string.IsNullOrWhiteSpace(source.SourcePath))
                throw new ArgumentException($"geometry identifier '{identifier}' has no source geo.json registered");
            if (!File.Exists(source.SourcePath))
                throw new FileNotFoundException(
                    $"source geometry not found for identifier '{identifier}': {source.SourcePath}",
                    source.SourcePath);

            File.Copy(source.SourcePath, targetFull, overwrite: true);
            CompilerState.Info($"({c}/{_geometrySources.Count}) registered geometry '{identifier}' -> models/blocks/{source.RpName}.geo.json");
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
        int c = 0;
        foreach (var (key, source) in sources)
        {
            c++;
            string rpPath = $"textures/{subdir}/{source.RpName}";
            string targetFull = Path.Combine(targetSubdir, $"{source.RpName}.png");

            if (string.IsNullOrWhiteSpace(source.SourcePath))
                throw new ArgumentException($"texture key '{key}' has no source PNG registered");
            if (!File.Exists(source.SourcePath))
                throw new FileNotFoundException(
                    $"source texture not found for key '{key}': {source.SourcePath}",
                    source.SourcePath);

            File.Copy(source.SourcePath, targetFull, overwrite: true);
            textureDataEntries[key] = new { textures = rpPath };
            CompilerState.Info($"({c}/{sources.Count}) registered texture '{key}' -> {rpPath}");
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