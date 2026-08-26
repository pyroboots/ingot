using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

using Version = ingot.Core.Common.Version;

namespace ingot.Core.Resource;

/// <summary>
/// C# representation of a Minecraft resource pack.
/// Houses the per-file handlers and compiles them, copies source assets, and
/// writes <see cref="IConcreteCompilable{TType}"/> client entities / render controllers.
/// </summary>
public class ResourcePack
{
    /// <summary>
    /// Minecraft UUID to be used at runtime
    /// </summary>
    public string Uuid;
    /// <summary>
    /// Name of the resource pack
    /// </summary>
    public string Name;
    /// <summary>
    /// Version of the <see cref="ResourcePack"/>. When <see cref="BehaviourPack"/> is linked, it will require at least this version.
    /// </summary>
    public Version ResourcePackVersion;

    /// <summary>
    /// Helper factory method to initiate API-style syntax
    /// </summary>
    public static ResourcePack Create(string uuid, Version? version = null) => new(uuid, name: null, version);

    /// <summary>
    /// Helper factory method to initiate API-style syntax
    /// </summary>
    public static ResourcePack Create(string uuid, string name, Version? version = null) => new(uuid, name, version);

    /// <summary>
    /// Creates a <see cref="ResourcePack"/> with the given runtime UUID and optional version.
    /// </summary>
    public ResourcePack(string uuid, Version? version = null)
        : this(uuid, name: null, version)
    {
    }

    /// <summary>
    /// Creates a <see cref="ResourcePack"/> with the given runtime UUID, name, and optional version.
    /// </summary>
    public ResourcePack(string uuid, string? name, Version? version = null)
    {
        Uuid = uuid;
        Name = name ?? "";
        ResourcePackVersion = version ?? new Version(1, 0, 0);

        ClientBiomeDefinitions = new();
        ClientBlockDefinitions = new();
        GameEventSoundBindings = new();
        SoundDefinitions = new();
        Textures = new(this);
        Models = new();
        Animations = new();
        Particles = new();
        Ui = new();
        ExtraFiles = new();
        Texts = new();
        Splashes = new();
    }

    /// <summary>
    /// Handler for <c>biomes_client.json</c>.
    /// </summary>
    public ClientBiomeDefinitions ClientBiomeDefinitions;
    /// <summary>
    /// Handler for <c>blocks.json</c>.
    /// </summary>
    public ClientBlockDefinitions ClientBlockDefinitions;
    /// <summary>
    /// Handler for <c>sounds.json</c> (block / entity / individual event bindings).
    /// </summary>
    public GameEventSoundBindings GameEventSoundBindings;
    /// <summary>
    /// Handler for <c>sounds/sound_definitions.json</c>.
    /// </summary>
    public SoundDefinitions SoundDefinitions;
    /// <summary>
    /// Texture atlases, entity/particle PNGs, and flipbook definitions.
    /// </summary>
    public TextureManager Textures;
    /// <summary>
    /// Geometry files copied under <c>models/</c>.
    /// </summary>
    public GeometryManager Models;
    /// <summary>
    /// Animation JSON files copied under <c>animations/</c>.
    /// </summary>
    public AnimationManager Animations;
    /// <summary>
    /// Particle effect JSON files copied under <c>particles/</c>.
    /// </summary>
    public ParticleManager Particles;
    /// <summary>
    /// JSON UI files copied under <c>ui/</c>, plus generated <c>_ui_defs.json</c>.
    /// </summary>
    public UiManager Ui;
    /// <summary>
    /// Arbitrary extra files copied at caller-specified paths (UI libraries, nineslice JSON, ...).
    /// Copied last so they can overlay generated files such as a provided <c>ui/_ui_defs.json</c>.
    /// </summary>
    public ExtraFileManager ExtraFiles;
    /// <summary>
    /// Language files under <c>texts/</c>.
    /// </summary>
    public LanguageDefinitions Texts;
    /// <summary>
    /// Handler for <c>splashes.json</c>.
    /// </summary>
    public SplashTexts Splashes;

    private readonly List<ClientEntity> _clientEntities = [];
    private readonly List<RenderController> _renderControllers = [];
    internal readonly HashSet<string> RegisteredRenderControllerIds = new(StringComparer.Ordinal);

    /// <summary>
    /// Client entity definitions registered on this pack.
    /// </summary>
    public IReadOnlyList<ClientEntity> ClientEntities => _clientEntities;

    /// <summary>
    /// Render controllers registered on this pack.
    /// </summary>
    public IReadOnlyList<RenderController> RenderControllers => _renderControllers;

    /// <summary>
    /// Sound definition ids registered on this pack (keys in <c>sound_definitions.json</c>).
    /// </summary>
    public IReadOnlyCollection<string> SoundDefinitionIds => SoundDefinitions.Ids;

    /// <summary>
    /// Particle effect identifiers registered on this pack (keys used with <see cref="ParticleManager.Add"/>).
    /// </summary>
    public IReadOnlyCollection<string> ParticleIds => Particles.Identifiers;

    /// <summary>
    /// Particle texture keys registered on this pack (under <c>textures/particle/</c>).
    /// </summary>
    public IReadOnlyCollection<string> ParticleTextureKeys => Textures.ParticleTextureKeys;

    /// <summary>
    /// UI file names registered on this pack (under <c>ui/</c>).
    /// </summary>
    public IReadOnlyCollection<string> UiIds => Ui.Names;

    /// <summary>
    /// UI texture keys registered on this pack (under <c>textures/ui/</c>).
    /// </summary>
    public IReadOnlyCollection<string> UiTextureKeys => Textures.UiTextureKeys;

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
        RegisteredRenderControllerIds.Add(inst.ControllerId);
    }

    /// <summary>
    /// Compiles the <see cref="ResourcePack"/> to output <paramref name="dir"/>
    /// </summary>
    /// <param name="dir">Output directory</param>
    public void Compile(string dir)
    {
        CompilerState.Push("rp");

        if (string.IsNullOrWhiteSpace(Name))
            Name = CompilerState.CurrentPack?.Name ?? "ingot pack";

        CreateFolderStructure(dir);
        SeedHandlers();

        if (Textures.BlockTextureAtlas.Sources.Count > 0)
        {
            ResourcePackIo.CopyFiles(dir, Textures.BlockTextureAtlas.EnumerateCopies(), "block textures");
            ResourcePackIo.WriteJson(
                Path.Combine(dir, "textures", "terrain_texture.json"),
                Textures.BlockTextureAtlas,
                "terrain_texture.json",
                $"wrote terrain_texture.json with {Textures.BlockTextureAtlas.TextureData.Count} entries");
        }

        ResourcePackIo.CopyFiles(dir, Textures.ItemTextureAtlas.EnumerateCopies(), "item textures");
        ResourcePackIo.WriteJson(
            Path.Combine(dir, "textures", "item_texture.json"),
            Textures.ItemTextureAtlas,
            "item_texture.json",
            $"wrote item_texture.json with {Textures.ItemTextureAtlas.TextureData.Count} entries");

        ResourcePackIo.CopyFiles(dir, Models.EnumerateCopies(), "geometries");
        ResourcePackIo.CopyFiles(dir, Animations.EnumerateCopies(), "animations");
        ResourcePackIo.CopyFiles(dir, Particles.EnumerateCopies(), "particles");
        ResourcePackIo.CopyFiles(dir, Textures.EnumerateParticleCopies(), "particle textures");
        ResourcePackIo.CopyFiles(dir, Ui.EnumerateCopies(), "ui");
        Ui.WriteUiDefs(dir);
        ResourcePackIo.CopyFiles(dir, Textures.EnumerateUiCopies(), "ui textures");

        CompileClientEntities(dir);

        ResourcePackIo.CopyFiles(dir, Textures.EnumerateEntityCopies(), "entity textures");
        CompileRenderControllers(dir);

        ResourcePackIo.WriteJson(
            Path.Combine(dir, "blocks.json"),
            ClientBlockDefinitions,
            "blocks.json",
            $"wrote blocks.json with {ClientBlockDefinitions.Blocks.Count} entries");

        Texts.Write(dir);

        ResourcePackIo.WriteJson(
            Path.Combine(dir, "biomes_client.json"),
            ClientBiomeDefinitions,
            "biomes_client.json");
        ResourcePackIo.WriteJson(
            Path.Combine(dir, "splashes.json"),
            Splashes,
            "splashes.json");
        ResourcePackIo.WriteJson(
            Path.Combine(dir, "textures", "flipbook_textures.json"),
            Textures.Flipbook,
            "flipbook_textures.json");
        CompilerState.Info("wrote resource pack stub files");

        ResourcePackIo.CopyFiles(dir, SoundDefinitions.EnumerateCopies(), "sound files");
        ResourcePackIo.WriteJson(
            Path.Combine(dir, "sounds", "sound_definitions.json"),
            SoundDefinitions,
            "sound_definitions.json",
            SoundDefinitions.Definitions.Count > 0
                ? $"wrote sound_definitions.json with {SoundDefinitions.Definitions.Count} definition(s)"
                : "wrote empty sound_definitions.json");

        ResourcePackIo.WriteJson(
            Path.Combine(dir, "sounds.json"),
            GameEventSoundBindings,
            "sounds.json",
            GameEventSoundBindings.EntitySounds.Count > 0
                ? $"wrote sounds.json with {GameEventSoundBindings.EntitySounds.Count} entity sound mapping(s)"
                : "wrote empty sounds.json");

        ResourcePackIo.CopyFiles(dir, ExtraFiles.EnumerateCopies(), "extra resource files");

        CompilerState.Pop();
    }

    private static void CreateFolderStructure(string dir)
    {
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
        Directory.CreateDirectory(Path.Combine(dir, "textures", "particles"));
        Directory.CreateDirectory(Path.Combine(dir, "particles"));
        Directory.CreateDirectory(Path.Combine(dir, "ui"));
        Directory.CreateDirectory(Path.Combine(dir, "textures", "ui"));
        Directory.CreateDirectory(Path.Combine(dir, "sounds"));
        Directory.CreateDirectory(Path.Combine(dir, "texts"));
        CompilerState.Info("created folder structure");
    }

    private void SeedHandlers()
    {
        if (CompilerState.CurrentPack is null)
            return;

        ClientBlockDefinitions.SeedFromPack(CompilerState.CurrentPack);
        Texts.SeedFromPack(CompilerState.CurrentPack);

        foreach (ClientEntity clientEntity in _clientEntities)
            GameEventSoundBindings.BindClientEntitySounds(clientEntity);
    }

    private void CompileClientEntities(string dir)
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

    private void CompileRenderControllers(string dir)
    {
        CompilerState.Push("render_controllers");

        string rcDir = Path.Combine(dir, "render_controllers");
        Directory.CreateDirectory(rcDir);

        int c = 0;
        foreach (RenderController controller in _renderControllers)
        {
            c++;
            string json = RenderController.CompileFromInstance(controller);
            string path = Path.Combine(rcDir, $"{controller.FileName}.json");
            File.WriteAllText(path, json);
            CompilerState.Info($"({c}/{_renderControllers.Count}) compiled render controller {controller.ControllerId}");
        }

        HashSet<string> emittedIds = new(RegisteredRenderControllerIds, StringComparer.Ordinal);
        int autoCount = 0;
        foreach (ClientEntity clientEntity in _clientEntities)
        {
            if (!clientEntity.EmitDefaultRenderController)
                continue;

            foreach (string controllerId in clientEntity.RenderControllers)
            {
                if (string.IsNullOrWhiteSpace(controllerId) || emittedIds.Contains(controllerId))
                    continue;

                if (!controllerId.StartsWith("controller.render.", StringComparison.Ordinal))
                    continue;

                RenderController simple = RenderController.CreateSimple(controllerId);
                string json = RenderController.CompileFromInstance(simple);
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
}
