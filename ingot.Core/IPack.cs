using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Entity;
using ingot.Core.Behaviour.Item;
using ingot.Core.Behaviour.Loot;
using ingot.Core.Behaviour.Recipe;
using ingot.Core.Scripting;

using Version = ingot.Core.Common.Version;

namespace ingot.Core;

public interface IPack
{
    public string BehaviourUuid { get; }
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

    public Type[] Items => [];
    public Type[] Blocks => [];
    public Type[] Entities => [];

    public Type[] Recipes => [];
    public Type[] LootTables => [];

    public record SoundDefinition(string SoundId, Sound[] Sounds, string Category = "neutral");
    public SoundDefinition[] SoundDefinitions => [];

    public record McFunction(string Identifier, string SourceFile, bool Service = false);
    public McFunction[] Functions => [];
    
    public record ScriptService(string SourceFile, string? Name = null, int Interval = 1);
    public ScriptService[] Services => [];
    public record ScriptEvent(string EventId, ScriptHandler Handler, string? Name = null);
    public ScriptEvent[] Events => [];
    
    /// <summary>
    /// Whether to initialise the behaviour pack with Script API capabilities
    /// </summary>
    public bool ScriptsEnabled => false;
    /// <summary>
    /// The entry point of the Script API to be loaded when the world is
    /// </summary>
    public string ScriptEntry => "scripts/main.js";
    
    private static void ValidateHomogenous<TExpected>(Type[] array, string name, Action<Type> regFunc)
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

    public static Pack GetPack<TPack>() where TPack : IPack, new()
    {
        IPack iPack = new TPack();
        
        Pack pack = Pack.Create(iPack.BehaviourUuid, iPack.Name, iPack.Description, iPack.ResourceUuid, iPack.PackVersion, iPack.PackVersion);
        pack.Authors = iPack.Authors;
        pack.PackIcon = iPack.PackIcon;
        pack.MinEngineVersion = iPack.MinEngineVersion;

        pack.ScriptsEnabled = iPack.ScriptsEnabled;
        pack.ScriptEntry = iPack.ScriptEntry;

        ValidateHomogenous<Item>(iPack.Items, nameof(Item), t => pack.AddItem(t));
        ValidateHomogenous<Block>(iPack.Blocks, nameof(Block), t => pack.AddBlock(t));
        ValidateHomogenous<Entity>(iPack.Entities, nameof(Entity), t => pack.AddEntity(t));
        ValidateHomogenous<IRecipe>(iPack.Recipes, "Recipe", t => pack.AddRecipe(t));
        ValidateHomogenous<LootTable>(iPack.LootTables, nameof(LootTable), t => pack.AddLootTable(t));

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
}