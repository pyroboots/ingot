using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Entity;
using ingot.Core.Behaviour.Item;
using ingot.Core.Behaviour.Loot;
using ingot.Core.Behaviour.Recipe;
using ingot.Core.Scripting;

using Version = ingot.Core.Common.Version;

namespace ingot.Core;

/// <summary>
/// Interface to implement the properties of a pack
/// </summary>
public interface IPack
{
    /// <summary>
    /// Allows extra configuration of the underlying <see cref="Pack"/> for properties
    /// that <see cref="IPack"/> does not yet expose
    /// </summary>
    public virtual Pack Configure(Pack pack) => pack;
    
    /// <summary>
    /// UUID for the BP
    /// </summary>
    public string BehaviourUuid { get; }
    /// <summary>
    /// UUID for the RP
    /// </summary>
    public string ResourceUuid { get; }
    
    /// <summary>
    /// Name of the pack that shows up in Minecraft
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Short description of the pack
    /// </summary>
    public string Description { get; }
    /// <summary>
    /// List of authors who helped with the development of the pack
    /// </summary>
    public string[] Authors => [];
    /// <summary>
    /// Icon for the behaviour pack and resource pack in the game
    /// </summary>
    public string? PackIcon => null;
    /// <summary>
    /// Version of the pack. Recommended to increment with each pack build
    /// </summary>
    public Version PackVersion => new(1, 0, 0);
    /// <summary>
    /// Minimum game version required to run this pack.
    /// Defaults to <c>1.21.90</c> to match the default block/item format version
    /// (required for Custom Components V2).
    /// </summary>
    public Version MinEngineVersion => new(1, 21, 90);

    /// <summary>
    /// Homogeneous type array of <see cref="Item"/>s
    /// </summary>
    public Type[] Items => [];
    /// <summary>
    /// Homogeneous type array of <see cref="Block"/>s
    /// </summary>
    public Type[] Blocks => [];
    /// <summary>
    /// Homogeneous type array of <see cref="Entity"/>s
    /// </summary>
    public Type[] Entities => [];

    /// <summary>
    /// Homogeneous type array of <see cref="IRecipe"/>s
    /// </summary>
    public Type[] Recipes => [];
    /// <summary>
    /// Homogeneous type array of <see cref="LootTable"/>s
    /// </summary>
    public Type[] LootTables => [];

    /// <summary>
    /// Represents a sound definition in <c>sound_definitions.json</c>
    /// </summary>
    /// <param name="SoundId">ID of the sound</param>
    /// <param name="Sounds">Array of sounds this definition can play</param>
    /// <param name="Category">Sound category</param>
    public record SoundDefinition(string SoundId, Sound[] Sounds, string Category = "neutral");
    /// <summary>
    /// Array of sound definitions to register in <c>sound_definitions.json</c>
    /// </summary>
    public SoundDefinition[] SoundDefinitions => [];

    /// <summary>
    /// Represents a Minecraft function
    /// </summary>
    /// <param name="Identifier">Identifier to use in commands for this function</param>
    /// <param name="SourceFile">Source function file</param>
    /// <param name="Service">Whether to run this function every tick as a service</param>
    public record McFunction(string Identifier, string SourceFile, bool Service = false);
    /// <summary>
    /// Array of functions to register
    /// </summary>
    public McFunction[] Functions => [];
    
    /// <summary>
    /// Represents a Minecraft script service
    /// </summary>
    /// <param name="SourceFile">JS source file for this service</param>
    /// <param name="Name">Identifier of the service</param>
    /// <param name="Interval">Run this service every n ticks</param>
    public record ScriptService(string SourceFile, string? Name = null, int Interval = 1);
    /// <summary>
    /// Array of services to register
    /// </summary>
    public ScriptService[] Services => [];
    /// <summary>
    /// Represents a Minecraft script event
    /// </summary>
    /// <param name="Handler">Handler source for this event</param>
    /// <param name="EventId">Identifier of the event</param>
    /// <param name="Name">Name to qualify this event to fire</param>
    public record ScriptEvent(string EventId, ScriptHandler Handler, string? Name = null);
    /// <summary>
    /// Array of script events to register
    /// </summary>
    public ScriptEvent[] Events => [];
    
    /// <summary>
    /// Whether to initialise the behaviour pack with Script API capabilities
    /// </summary>
    public bool ScriptsEnabled => false;
    /// <summary>
    /// The entry point of the Script API to be loaded when the world is
    /// </summary>
    public string ScriptEntry => "scripts/main.js";
    
    private static void ValidateHomogeneous<TExpected>(Type[] array, string name, Action<Type> regFunc)
    {
        foreach (Type t in array)
        {
            // use IsAssignableTo because JsonEntity is not Entity, but it is 
            // assignable to
            if (t.IsAssignableTo(typeof(TExpected)) == false)
                throw new ArrayTypeMismatchException($"expected {name}s in {name} array");
            regFunc(t);
        }
    }

    /// <summary>
    /// Returns the underlying <see cref="Pack"/> for extra configuration if necessary
    /// </summary>
    public static Pack GetPack<TPack>() where TPack : IPack, new()
    {
        IPack iPack = new TPack();
        
        Pack pack = Pack.Create(iPack.BehaviourUuid, iPack.Name, iPack.Description, iPack.ResourceUuid, iPack.PackVersion, iPack.PackVersion);
        pack.Authors = iPack.Authors;
        pack.PackIcon = iPack.PackIcon;
        pack.MinEngineVersion = iPack.MinEngineVersion;

        pack.ScriptsEnabled = iPack.ScriptsEnabled;
        pack.ScriptEntry = iPack.ScriptEntry;

        ValidateHomogeneous<Item>(iPack.Items, nameof(Item), t => pack.AddItem(t));
        ValidateHomogeneous<Block>(iPack.Blocks, nameof(Block), t => pack.AddBlock(t));
        ValidateHomogeneous<Entity>(iPack.Entities, nameof(Entity), t => pack.AddEntity(t));
        ValidateHomogeneous<IRecipe>(iPack.Recipes, "Recipe", t => pack.AddRecipe(t));
        ValidateHomogeneous<LootTable>(iPack.LootTables, nameof(LootTable), t => pack.AddLootTable(t));

        foreach (SoundDefinition def in iPack.SoundDefinitions)
            pack.RegisterSoundDefinition(def.SoundId, def.Sounds, def.Category);
        
        foreach (McFunction f in iPack.Functions)
            pack.AddFunction(f.Identifier, f.SourceFile, f.Service);
        foreach (ScriptService s in iPack.Services)
            pack.AddService(s.SourceFile, s.Name, s.Interval);
        foreach (ScriptEvent e in iPack.Events)
            pack.AddScriptEvent(e.EventId, e.Handler, e.Name);
        
        return pack;
    }

    public static void CompileToMcaddon<TPack>(string outputPath, bool verbose = true) where TPack : IPack, new()
        => new TPack().Configure(GetPack<TPack>()).CompileMcaddon(outputPath, verbose, false);
    
    public static void CompileComMojang<TPack>(string mojangPath, bool verbose = true) where TPack : IPack, new()
        => new TPack().Configure(GetPack<TPack>()).CompileComMojang(mojangPath, verbose, false);
}