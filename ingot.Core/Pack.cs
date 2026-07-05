using System.Diagnostics;

using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Loot;
using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;
using ingot.Core.Scripting;

using Newtonsoft.Json;

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
    /// Minimum game version required to run this pack
    /// </summary>
    public Version MinEngineVersion = new(1, 20, 0);
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
    /// </summary>
    public Pack AddEntity<TEntity>() where TEntity : Entity, new()
    {
        BehaviourPack.AddEntity<TEntity>();
        return this;
    }

    /// <summary>
    /// Adds an entity to the pack.
    /// </summary>
    public Pack AddEntity(Type tEntity)
    {
        BehaviourPack.AddEntity(tEntity);
        return this;
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
    /// Registers a block geometry file (<c>.geo.json</c>) that will be copied into the resource pack under
    /// <c>models/blocks/</c>. The <paramref name="identifier"/> must match the geometry referenced from
    /// behaviour-side <c>minecraft:geometry</c>.
    /// </summary>
    public Pack AddGeometry(string identifier, string sourceGeoJsonPath, string? rpName = null)
    {
        ResourcePack.AddGeometry(identifier, sourceGeoJsonPath, rpName);
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
            string iconName = Path.GetFileName(PackIcon);
            File.Copy(PackIcon, Path.Combine(behaviourPackDir, iconName), overwrite: true);
            File.Copy(PackIcon, Path.Combine(resourcePackDir, iconName), overwrite: true);
        }

        timer.Stop();

        if (verbose)
        {
            File.WriteAllText(Path.Combine(cacheDir, "ingot.log"), string.Join('\n', CompilerState.GetLogs()));
            Console.WriteLine();
            CompilerState.Info($"pack compiled in {timer.ElapsedMilliseconds}ms");
            CompilerState.Info($"ingot compilation log available at {Path.Combine(cacheDir, "ingot.log")}");
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