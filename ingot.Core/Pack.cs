using System.Diagnostics;
using System.Reflection;
using System.Text.Encodings.Web;

using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Entity;
using ingot.Core.Behaviour.Item;
using ingot.Core.Behaviour.Loot;
using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;
using ingot.Core.Scripting;

using Newtonsoft.Json;

using Spectre.Console;

using Formatting = Newtonsoft.Json.Formatting;
using Version = ingot.Core.Common.Version;

namespace ingot.Core;

/// <summary>
/// C# representation of a full pack with behaviour and resources
/// </summary>
public class Pack
{
    /// <summary>
    /// Name of the pack that shows up in Minecraft
    /// </summary>
    public required string Name;
    /// <summary>
    /// Short description of the pack
    /// </summary>
    public required string Description;
    /// <summary>
    /// Icon for the behaviour pack and resource pack in the game
    /// </summary>
    public string? PackIcon = null;
    /// <summary>
    /// Version of the pack. Recommended to increment with each pack build
    /// </summary>
    public Version PackVersion = new(1, 0, 0);
    /// <summary>
    /// Minimum game version required to run this pack.
    /// Defaults to <c>1.21.90</c> to match the default block/item format version
    /// (required for Custom Components V2).
    /// </summary>
    public Version MinEngineVersion = new(1, 21, 90);
    /// <summary>
    /// List of authors who helped with the development of the pack
    /// </summary>
    public string[] Authors = [];

    /// <summary>
    /// Whether to initialise the behaviour pack with Script API capabilities
    /// </summary>
    public bool ScriptsEnabled = false;
    /// <summary>
    /// The entry point of the Script API to be loaded when the world is
    /// </summary>
    public string ScriptEntry = "scripts/main.js";
    /// <summary>
    /// Dictionary of Script API modules to import
    /// </summary>
    public Dictionary<string, Version> ScriptApiModules = new()
    {
        ["@minecraft/server"] = new(2, 8, 0),
    };

    /// <summary>
    /// <see cref="BehaviourPack"/> to be compiled
    /// </summary>
    public required BehaviourPack BehaviourPack;
    /// <summary>
    /// <see cref="ResourcePack"/> to be compiled
    /// </summary>
    public required ResourcePack ResourcePack;
    /// <summary>
    /// Whether to make the <see cref="BehaviourPack"/> and <see cref="ResourcePack"/> depend on each other
    /// </summary>
    public bool LinkPacks = false;
    /// <summary>
    /// Omits metadata from the manifests
    /// </summary>
    public bool OmitMetadata = false;

    internal readonly ScriptRegistry ScriptRegistry = new();
    private readonly List<ScriptServiceRegistration> _services = new();

    /// <summary>
    /// Whether the compiled behaviour pack requires a Script API module in the manifest.
    /// </summary>
    internal bool RequiresScriptModule { get; private set; }

    /// <summary>Registered Script API services.</summary>
    internal IReadOnlyList<ScriptServiceRegistration> Services => _services;

    /// <summary>
    /// Defines the JavaScript body in the script entrypoint
    /// </summary>
    public ScriptHandler? ScriptEntryBody { get; set; }

    /// <summary>
    /// Creates a <see cref="Pack"/> with linked behaviour and resource packs. This is the recommended entry point.
    /// </summary>
    /// <param name="behaviourUuid">Minecraft UUID for the behaviour pack.</param>
    /// <param name="name">Pack name shown in Minecraft.</param>
    /// <param name="description">Short pack description.</param>
    /// <param name="resourceUuid">Minecraft UUID for the resource pack. A new UUID is generated when omitted.</param>
    /// <param name="behaviourPackVersion">Behaviour pack version used in cross-pack dependencies.</param>
    /// <param name="resourcePackVersion">Resource pack version used in cross-pack dependencies.</param>
    public static Pack Create(
        string behaviourUuid,
        string name,
        string description,
        string? resourceUuid = null,
        Version? behaviourPackVersion = null,
        Version? resourcePackVersion = null) =>
        new()
        {
            Name = name,
            Description = description,
            BehaviourPack = BehaviourPack.Create(behaviourUuid, behaviourPackVersion),
            ResourcePack = ResourcePack.Create(resourceUuid ?? Guid.NewGuid().ToString(), resourcePackVersion),
            LinkPacks = true,
        };

    /// <summary>
    /// Adds an entity to the pack.
    /// When <paramref name="discoverClient"/> is true (default), also registers a matching
    /// <see cref="ClientEntity{TParent}"/> and nested <see cref="RenderController"/> types if found.
    /// </summary>
    public Pack AddEntity<TEntity>(bool discoverClient = true) where TEntity : Entity, new()
    {
        BehaviourPack.AddEntity<TEntity>();
        if (discoverClient)
            DiscoverAndRegisterClientSide(typeof(TEntity));
        return this;
    }

    /// <summary>
    /// Adds an entity to the pack.
    /// When <paramref name="discoverClient"/> is true (default), also registers a matching
    /// client entity / render controller if found.
    /// </summary>
    public Pack AddEntity(Type tEntity, bool discoverClient = true)
    {
        BehaviourPack.AddEntity(tEntity);
        if (discoverClient)
            DiscoverAndRegisterClientSide(tEntity);
        return this;
    }

    private void DiscoverAndRegisterClientSide(Type tEntity)
    {
        if (!typeof(Entity).IsAssignableFrom(tEntity))
            return;

        Entity? inst = null;
        try
        {
            inst = Activator.CreateInstance(tEntity) as Entity;
        }
        catch
        {
            // ignore construct failures; discovery is best-effort
        }

        Type? clientType = inst?.ClientEntityType;
        if (clientType is null)
            clientType = FindClientEntityType(tEntity);

        if (clientType is not null && typeof(ClientEntity).IsAssignableFrom(clientType))
            ResourcePack.AddClientEntity(clientType);

        // Nested RenderController types on the entity or client type
        foreach (Type nested in EnumerateNestedRenderControllers(tEntity, clientType))
            ResourcePack.AddRenderController(nested);
    }

    private static Type? FindClientEntityType(Type tEntity)
    {
        // Nested type named Client: Entity.Client : ClientEntity<Entity>
        Type? nestedClient = tEntity.GetNestedType("Client", BindingFlags.Public | BindingFlags.NonPublic);
        if (nestedClient is not null && IsClientEntityFor(nestedClient, tEntity))
            return nestedClient;

        List<Type> matches = tEntity.Assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && IsClientEntityFor(t, tEntity))
            .ToList();

        if (matches.Count == 0)
            return null;
        if (matches.Count == 1)
            return matches[0];

        // Prefer conventional name: FooEntity -> FooClientEntity, Bar -> BarClientEntity
        string expectedName = tEntity.Name.EndsWith("Entity", StringComparison.Ordinal)
            ? tEntity.Name[..^"Entity".Length] + "ClientEntity"
            : tEntity.Name + "ClientEntity";

        Type? conventional = matches.FirstOrDefault(t => t.Name == expectedName);
        if (conventional is not null)
            return conventional;

        // Prefer same-namespace exact suffix ClientEntity without extra qualifiers
        Type? simple = matches
            .Where(t => t.Namespace == tEntity.Namespace && t.Name.EndsWith("ClientEntity", StringComparison.Ordinal))
            .OrderBy(t => t.Name.Length)
            .FirstOrDefault();
        return simple ?? matches[0];
    }

    private static bool IsClientEntityFor(Type candidate, Type tEntity)
    {
        Type? t = candidate;
        while (t is not null && t != typeof(object))
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ClientEntity<>))
            {
                Type parentArg = t.GetGenericArguments()[0];
                return parentArg == tEntity;
            }
            t = t.BaseType;
        }

        // Also accept subclasses of ClientEntity`1 closed over tEntity
        if (!typeof(ClientEntity).IsAssignableFrom(candidate))
            return false;

        Type? baseType = candidate.BaseType;
        while (baseType is not null && baseType != typeof(object))
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(ClientEntity<>))
                return baseType.GetGenericArguments()[0] == tEntity;
            baseType = baseType.BaseType;
        }

        return false;
    }

    private static IEnumerable<Type> EnumerateNestedRenderControllers(Type tEntity, Type? clientType)
    {
        foreach (Type nested in tEntity.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (typeof(RenderController).IsAssignableFrom(nested) && !nested.IsAbstract)
                yield return nested;
        }

        if (clientType is null)
            yield break;

        foreach (Type nested in clientType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (typeof(RenderController).IsAssignableFrom(nested) && !nested.IsAbstract)
                yield return nested;
        }
    }

    /// <summary>
    /// Adds a block to the pack.
    /// </summary>
    public Pack AddBlock<TBlock>() where TBlock : Block, new()
    {
        BehaviourPack.AddBlock<TBlock>();
        return this;
    }

    /// <summary>
    /// Adds a block to the pack.
    /// </summary>
    public Pack AddBlock(Type tBlock)
    {
        BehaviourPack.AddBlock(tBlock);
        return this;
    }

    /// <summary>
    /// Adds an item to the pack.
    /// </summary>
    public Pack AddItem<TItem>() where TItem : Item, new()
    {
        BehaviourPack.AddItem<TItem>();
        return this;
    }

    /// <summary>
    /// Adds an item to the pack.
    /// </summary>
    public Pack AddItem(Type tItem)
    {
        BehaviourPack.AddItem(tItem);
        return this;
    }

    /// <summary>
    /// Adds a recipe to the pack.
    /// </summary>
    public Pack AddRecipe<TRecipe>() where TRecipe : IRecipe, new()
    {
        BehaviourPack.AddRecipe<TRecipe>();
        return this;
    }

    /// <summary>
    /// Adds a recipe to the pack.
    /// </summary>
    public Pack AddRecipe(Type tRecipe)
    {
        BehaviourPack.AddRecipe(tRecipe);
        return this;
    }

    /// <summary>
    /// Adds a loot table to the pack.
    /// </summary>
    public Pack AddLootTable<TLootTable>() where TLootTable : LootTable, new()
    {
        BehaviourPack.AddLootTable<TLootTable>();
        return this;
    }

    /// <summary>
    /// Adds a loot table to the pack.
    /// </summary>
    public Pack AddLootTable(Type tLootTable)
    {
        BehaviourPack.AddLootTable(tLootTable);
        return this;
    }

    /// <summary>
    /// Manually registers a block texture. Takes precedence over behaviour-side auto-registration for the same key.
    /// </summary>
    public Pack AddBlockTexture(string key, string sourcePngPath, string? rpName = null)
    {
        ResourcePack.AddBlockTexture(key, sourcePngPath, rpName);
        return this;
    }

    /// <summary>
    /// Manually registers an item texture. Takes precedence over behaviour-side auto-registration for the same key.
    /// </summary>
    public Pack AddItemTexture(string key, string sourcePngPath, string? rpName = null)
    {
        ResourcePack.AddItemTexture(key, sourcePngPath, rpName);
        return this;
    }

    /// <summary>
    /// Registers a geometry file (<c>.geo.json</c>) under <c>models/{modelsSubdir}/</c>
    /// (default <c>blocks</c>).
    /// </summary>
    public Pack AddGeometry(
        string identifier,
        string sourceGeoJsonPath,
        string? rpName = null,
        string modelsSubdir = "blocks")
    {
        ResourcePack.AddGeometry(identifier, sourceGeoJsonPath, rpName, modelsSubdir);
        return this;
    }

    /// <summary>
    /// Registers an entity geometry file under <c>models/entity/</c>.
    /// </summary>
    public Pack AddEntityGeometry(string identifier, string sourceGeoJsonPath, string? rpName = null)
    {
        ResourcePack.AddEntityGeometry(identifier, sourceGeoJsonPath, rpName);
        return this;
    }

    /// <summary>
    /// Registers an animation JSON file under <c>animations/{rpName}.json</c>.
    /// </summary>
    public Pack AddAnimation(string sourceJsonPath, string rpName)
    {
        ResourcePack.AddAnimation(sourceJsonPath, rpName);
        return this;
    }

    /// <summary>
    /// Registers a particle effect JSON under <c>particles/{rpName}.json</c>.
    /// The <paramref name="identifier"/> should match <c>description.identifier</c> in the JSON
    /// (e.g. <c>mynamespace:sparkle</c>) and is used with Script API <c>spawnParticle</c>.
    /// </summary>
    /// <param name="identifier">Particle effect identifier (<c>namespace:name</c>).</param>
    /// <param name="sourceJsonPath">Path to the source particle effect JSON on disk.</param>
    /// <param name="rpName">Optional filename (without extension) under <c>particles/</c>.
    /// Defaults to the name segment of <paramref name="identifier"/>.</param>
    public Pack AddParticle(string identifier, string sourceJsonPath, string? rpName = null)
    {
        ResourcePack.AddParticle(identifier, sourceJsonPath, rpName);
        return this;
    }

    /// <summary>
    /// Registers a particle texture PNG under <c>textures/particles/</c>.
    /// Reference it from particle JSON as <c>textures/particles/{key}</c> (no extension).
    /// </summary>
    public Pack AddParticleTexture(string key, string sourcePngPath, string? rpName = null)
    {
        ResourcePack.AddParticleTexture(key, sourcePngPath, rpName);
        return this;
    }

    /// <summary>
    /// Registers an entity texture PNG that will be copied into the resource pack under <c>textures/entity/</c>.
    /// </summary>
    public Pack AddEntityTexture(string key, string sourcePngPath, string? rpName = null)
    {
        ResourcePack.AddEntityTexture(key, sourcePngPath, rpName);
        return this;
    }

    /// <summary>
    /// Adds a client entity (resource-pack entity visuals) to the pack.
    /// </summary>
    public Pack AddClientEntity<TClientEntity>() where TClientEntity : ClientEntity, new()
    {
        ResourcePack.AddClientEntity<TClientEntity>();
        return this;
    }

    /// <summary>
    /// Adds a client entity (resource-pack entity visuals) to the pack.
    /// </summary>
    public Pack AddClientEntity(Type tClientEntity)
    {
        ResourcePack.AddClientEntity(tClientEntity);
        return this;
    }

    /// <summary>
    /// Adds a render controller to the pack.
    /// </summary>
    public Pack AddRenderController<TRenderController>() where TRenderController : RenderController, new()
    {
        ResourcePack.AddRenderController<TRenderController>();
        return this;
    }

    /// <summary>
    /// Adds a render controller to the pack.
    /// </summary>
    public Pack AddRenderController(Type tRenderController)
    {
        ResourcePack.AddRenderController(tRenderController);
        return this;
    }

    /// <summary>
    /// Adds a pre-built render controller instance to the pack.
    /// </summary>
    public Pack AddRenderController(RenderController controller)
    {
        ResourcePack.AddRenderController(controller);
        return this;
    }

    /// <summary>
    /// Registers a sound definition written to <c>rp/sounds/sound_definitions.json</c>.
    /// The <paramref name="soundId"/> is the event name used by gameplay - a plain string,
    /// not an <see cref="Identifier"/>. Entries with a <see cref="Sound.SourcePath"/> are
    /// copied into the resource pack at <see cref="Sound.Name"/> (nested dirs under <c>sounds/</c>).
    /// </summary>
    /// <param name="soundId">Sound event id (e.g. <c>example.toot</c> or <c>ambient.basalt_deltas.loop</c>).</param>
    /// <param name="sounds">One or more sound file entries for this event.</param>
    /// <param name="category">Volume slider category (<c>ambient</c>, <c>hostile</c>, <c>music</c>, ...).</param>
    /// <param name="maxDistance">Distance beyond which the sound can no longer be heard.</param>
    /// <param name="minDistance">Distance at which attenuation begins.</param>
    public Pack RegisterSoundDefinition(
        string soundId,
        Sound[] sounds,
        string? category = null,
        float? maxDistance = null,
        float? minDistance = null)
    {
        ResourcePack.RegisterSoundDefinition(soundId, sounds, category, maxDistance, minDistance);
        return this;
    }

    /// <summary>
    /// Adds a function to the <see cref="BehaviourPack"/>
    /// </summary>
    /// <param name="identifier">The name of the function in game</param>
    /// <param name="sourceFile">The function source file</param>
    /// <param name="service">Whether to run this function every tick</param>
    public Pack AddFunction(string identifier, string sourceFile, bool service = false)
    {
        BehaviourPack.AddFunction(identifier, sourceFile, service);
        return this;
    }

    /// <summary>
    /// Registers a Script API service that runs every tick. The source file contains the tick
    /// handler body; ingot wraps it in <c>system.runInterval</c> and writes the result to
    /// <c>bp/scripts/services/</c>, imported from <c>scripts/main.js</c>.
    /// </summary>
    /// <param name="sourceFile">Path to the JavaScript service tick body.</param>
    /// <param name="name">Optional output file name. Defaults to the source file name.</param>
    /// <param name="intervalTicks">Ticks between each run of the service body. Defaults to 1 (every tick).</param>
    public Pack AddService(string sourceFile, string? name = null, int intervalTicks = 1)
    {
        if (intervalTicks < 1)
            throw new ArgumentOutOfRangeException(nameof(intervalTicks), intervalTicks, "service interval must be at least 1 tick");

        string fileName = name ?? Path.GetFileName(sourceFile);
        _services.Add(new ScriptServiceRegistration(sourceFile, $"scripts/services/{fileName}", intervalTicks));
        return this;
    }

    /// <summary>
    /// Compiles the pack to a Minecraft-importable <c>.mcaddon</c> archive.
    /// Any existing file at <paramref name="outputPath"/> is deleted first.
    /// The pack is built in a temporary directory, zipped with behaviour and resource
    /// folders at the archive root, then the temporary files are deleted.
    /// </summary>
    /// <param name="outputPath">Path to the <c>.mcaddon</c> file to create</param>
    /// <param name="verbose">Whether to print info logs to the console</param>
    /// <param name="cache">Whether to use or generate a <c>.ingot</c> cache file next to the output</param>
    public void CompileMcaddon(string outputPath, bool verbose = true, bool cache = true)
    {
        string resolvedOutputPath = Path.GetFullPath(outputPath);
        string outputDir = Path.GetDirectoryName(resolvedOutputPath)!;
        string tempDir = Path.Combine(Path.GetTempPath(), "ingot", Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDir);

        try
        {
            string cachePath = Path.Combine(outputDir, ".ingot");
            if (cache && File.Exists(cachePath))
                File.Copy(cachePath, Path.Combine(tempDir, ".ingot"), overwrite: true);

            Compile(tempDir, verbose, cache);

            Directory.CreateDirectory(outputDir);

            if (cache && File.Exists(Path.Combine(tempDir, ".ingot")))
                File.Copy(Path.Combine(tempDir, ".ingot"), cachePath, overwrite: true);

            string logPath = Path.Combine(tempDir, "ingot.log");
            if (verbose && File.Exists(logPath))
                File.Copy(logPath, Path.Combine(outputDir, "ingot.log"), overwrite: true);

            McaddonWriter.Write(resolvedOutputPath, tempDir, Name);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    /// <summary>
    /// Compiles both <see cref="BehaviourPack"/> and <see cref="ResourcePack"/> and generates pack manifests.
    /// Any existing <c>bp/</c> and <c>rp/</c> subfolders under <paramref name="outputDir"/> are deleted first.
    /// </summary>
    /// <param name="outputDir">Output directory to place the behaviour pack and resource pack</param>
    /// <param name="verbose">Whether to print info logs to the console</param>
    /// <param name="cache">Whether to use or generate a cache file</param>
    public void Compile(string outputDir, bool verbose = true, bool cache = true)
    {
        string resolved = Path.GetFullPath(outputDir);
        CompileTo(
            resolved,
            Path.Combine(resolved, "bp"),
            Path.Combine(resolved, "rp"),
            verbose,
            cache);
    }

    /// <summary>
    /// Compiles the pack directly into a Minecraft Bedrock <c>com.mojang</c> directory for local development.
    /// Any existing <c>development_behavior_packs/{Name} BP/</c> and <c>development_resource_packs/{Name} RP/</c>
    /// folders are deleted first.
    /// </summary>
    /// <param name="comMojangPath">Path to the <c>com.mojang</c> folder (for example the MCPelauncher Flatpak games directory)</param>
    /// <param name="verbose">Whether to print info logs to the console</param>
    /// <param name="cache">Whether to use or generate a <c>.ingot</c> cache file in the <paramref name="comMojangPath"/> directory</param>
    public void CompileComMojang(string comMojangPath, bool verbose = true, bool cache = true)
    {
        string root = Path.GetFullPath(comMojangPath);
        CompileTo(
            root,
            Path.Combine(root, "development_behavior_packs", $"{Name} BP"),
            Path.Combine(root, "development_resource_packs", $"{Name} RP"),
            verbose,
            cache);
    }

    private static void DeleteCompileOutputDirectory(string dir)
    {
        if (Directory.Exists(dir))
            Directory.Delete(dir, recursive: true);
    }

    private void CompileTo(string cacheDir, string behaviourPackDir, string resourcePackDir, bool verbose, bool cache)
    {
        DeleteCompileOutputDirectory(behaviourPackDir);
        DeleteCompileOutputDirectory(resourcePackDir);

        // Fancy header/chart only for verbose compiles. Always-on CanvasImage broke unit
        // tests: test hosts often report console width/height as -1, which crashes ImageSharp.
        if (verbose)
            IngotCommon.WriteHeader();
        
        Stopwatch timer = Stopwatch.StartNew();

        CompilerState.Reset();
        CompilerState.Push(Name);
        CompilerState.ShowInfoLogs = verbose;
        CompilerState.CurrentPack = this;
        ScriptRegistry.Clear();
        RequiresScriptModule = false;

        if (cache && CompilerState.Cache is null && File.Exists(Path.Combine(cacheDir, ".ingot")))
        {
            string json = File.ReadAllText(Path.Combine(cacheDir, ".ingot"));
            CompilerState.Cache = JsonConvert.DeserializeObject<IngotCache>(json);

            BehaviourPack.Uuid = CompilerState.Cache.Value.BehaviourUuid;
            ResourcePack.Uuid = CompilerState.Cache.Value.ResourceUuid;
            
            CompilerState.Info("loaded .ingot cache, using overrides");
        }
        
        CompilerState.Info("pack compilation started");

        CompilerState.Info("compiling bp...");
        BehaviourPack.Compile(behaviourPackDir);
        CompilerState.Info($"compiled bp");

        RequiresScriptModule = ScriptCompiler.Compile(this, behaviourPackDir);

        CompilerState.Info("compiling rp...");
        ResourcePack.Compile(resourcePackDir);
        CompilerState.Info($"compiled rp");

        ManifestWriter.WriteBehaviourPackManifest(this, Path.Combine(behaviourPackDir, "manifest.json"));
        CompilerState.Info("compiled bp manifest");

        ManifestWriter.WriteResourcePackManifest(this, Path.Combine(resourcePackDir, "manifest.json"));
        CompilerState.Info("compiled rp manifest");

        if (PackIcon is not null)
        {
            File.Copy(PackIcon, Path.Combine(behaviourPackDir, "pack_icon.png"), overwrite: true);
            File.Copy(PackIcon, Path.Combine(resourcePackDir, "pack_icon.png"), overwrite: true);
        }

        timer.Stop();

        if (verbose)
        {
            File.WriteAllText(Path.Combine(cacheDir, "ingot.log"), string.Join('\n', CompilerState.GetLogs()));
            AnsiConsole.MarkupLine($"[{IngotCommon.PrimaryColor.ToMarkup()}]ingot compilation log available at[/] [{IngotCommon.SecondaryColor.ToMarkup()}]{Markup.Escape(Path.Combine(cacheDir, "ingot.log"))}[/]");
            AnsiConsole.MarkupLine($"[{IngotCommon.PrimaryColor.ToMarkup()}]pack compiled in[/] [{IngotCommon.SecondaryColor.ToMarkup()}]{timer.ElapsedMilliseconds}ms[/]");

            // technically blocks are permutations, just default ones
            int blockPermCount = BehaviourPack.Blocks.Count;
            foreach (Block b in BehaviourPack.Blocks)
                blockPermCount += b.Permutations.Count;

            BreakdownChart chart = new BreakdownChart()
                .AddItem("entities", BehaviourPack.Entities.Count, Color.Green)
                .AddItem("render controllers", ResourcePack.RenderControllers.Count, Color.Red)
                .AddItem("blocks", BehaviourPack.Blocks.Count, Color.Blue)
                .AddItem("block permutations", blockPermCount, Color.Orange1)
                .AddItem("items", BehaviourPack.Items.Count, Color.Yellow)
                .AddItem("loot tables", BehaviourPack.LootTables.Count, Color.Red)
                .AddItem("recipes", BehaviourPack.Recipes.Count, Color.Purple)
                .AddItem("functions", BehaviourPack.Functions.Count, Color.White)
                .AddItem("scripts", ScriptRegistry.Entries.Count, Color.Aqua)
                .AddItem("services", Services.Count, Color.Violet)
                .Width(Math.Clamp(AnsiConsole.Profile.Width > 0 ? AnsiConsole.Profile.Width : 80, 40, 80));

            AnsiConsole.Write(chart);
        }
        
        if (cache && File.Exists(Path.Combine(cacheDir, ".ingot")) == false)
        {
            IngotCache ingotCache = new()
            {
                BehaviourUuid = BehaviourPack.Uuid,
                ResourceUuid = ResourcePack.Uuid,
                Items = BehaviourPack.Items.Select(i => i.Identifier.ToString()).ToArray(),
                Blocks = BehaviourPack.Blocks.Select(i => i.Identifier.ToString()).ToArray(),
                Entities = BehaviourPack.Entities.Select(i => i.Identifier.ToString()).ToArray()
            };

            string json = JsonConvert.SerializeObject(ingotCache, Formatting.Indented);
            File.WriteAllText(Path.Combine(cacheDir, ".ingot"), json);
            
            CompilerState.Info("generated .ingot cache");
        }

        CompilerState.ShowInfoLogs = false;
        CompilerState.CurrentPack = null;
        CompilerState.Pop();
    }

}