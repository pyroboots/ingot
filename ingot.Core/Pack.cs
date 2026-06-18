using System.Diagnostics;
using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Loot;
using ingot.Core.Behaviour.Recipe;
using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;
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
    public bool LinkPacks = true;

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
    public Pack AddBlockTexture(string key, string sourcePngPath)
    {
        ResourcePack.AddBlockTexture(key, sourcePngPath);
        return this;
    }

    /// <summary>
    /// Manually registers an item texture. Takes precedence over behaviour-side auto-registration for the same key.
    /// </summary>
    public Pack AddItemTexture(string key, string sourcePngPath)
    {
        ResourcePack.AddItemTexture(key, sourcePngPath);
        return this;
    }

    /// <summary>
    /// Compiles both <see cref="BehaviourPack"/> and <see cref="ResourcePack"/> and generates pack manifests
    /// </summary>
    /// <param name="outputDir">Output directory to place the behaviour pack and resource pack</param>
    /// <param name="verbose">Whether to print info logs to the console</param>
    public void Compile(string outputDir, bool verbose = true)
    {
        Stopwatch timer = Stopwatch.StartNew();
        
        CompilerState.Push(Name);
        CompilerState.ShowInfoLogs = verbose;
        CompilerState.CurrentPack = this;
        CompilerState.Info("pack compilation started");
        
        CompilerState.Info("compiling bp...");
        BehaviourPack.Compile(Path.Combine(outputDir, "bp"));
        CompilerState.Info($"compiled bp");
        
        CompilerState.Info("compiling rp...");
        ResourcePack.Compile(Path.Combine(outputDir, "rp"));
        CompilerState.Info($"compiled rp");
        
        using (StringWriter sw = new())
        {
            JsonTextWriter w = new(sw);
            w.Formatting = Formatting.Indented;
            w.Indentation = 4;

            JsonHelper json = new(ref w);
            
            w.WriteStartObject();
            
            json.Property("format_version", 2);
            json.Object( "header", () =>
            {
                json.Property("name", Name);
                json.Property("description", Description);
                json.Property("uuid", BehaviourPack.Uuid);
                json.Property("version", PackVersion.AsArray());
                json.Property("min_engine_version", MinEngineVersion.AsArray());
            });
            
            json.Array("modules", () =>
            {
                json.Object("", () =>
                {
                    json.Property("description", $"{Name} Behaviour");
                    json.Property("type", "data");
                    json.Property("uuid", Guid.NewGuid().ToString());
                    json.Property("version", new Version(1, 0, 0).AsArray());
                });
                if (ScriptsEnabled) json.Object("", () =>
                {
                    json.Property("type", "script");
                    json.Property("language", "javascript");
                    json.Property("uuid", Guid.NewGuid().ToString());
                    json.Property("entry", ScriptEntry);
                    File.Create(Path.Combine(outputDir, "bp", ScriptEntry));
                    json.Property("version", new Version(1, 0, 0).AsArray());
                });
            });
            
            json.Array("dependencies", () =>
            {
                if (LinkPacks) json.Object("", () =>
                {
                    json.Property("uuid", ResourcePack.Uuid);
                    json.Property("version", ResourcePack.ResourcePackVersion.AsArray());
                });
                if (ScriptsEnabled) foreach (var kvp in ScriptApiModules)
                {
                    json.Object("", () =>
                    {
                        json.Property("module_name", kvp.Key);
                        json.Property("version", kvp.Value.AsArray());
                    });
                }
            });
            
            json.Object("metadata", () =>
            {
                json.Property("authors", Authors);
                json.Object("generated_with", () =>
                {
                    json.Property("ingot", "https://github.com/pyroboots/ingot");
                });
            });
            
            w.WriteEndObject();
            
            File.WriteAllText(Path.Combine(outputDir, "bp", "manifest.json"), sw.ToString());
        }
        CompilerState.Info("compiled bp manifest");
        
        using (StringWriter sw = new())
        {
            JsonTextWriter w = new(sw);
            w.Formatting = Formatting.Indented;
            w.Indentation = 4;
    
            JsonHelper json = new(ref w);
            
            w.WriteStartObject();
            
            json.Property("format_version", 2);
            json.Object("header", () =>
            {
                json.Property("name", Name);
                json.Property("description", Description);
                json.Property("uuid", ResourcePack.Uuid);
                json.Property("version", PackVersion.AsArray());
                json.Property("min_engine_version", MinEngineVersion.AsArray());
            });
            
            json.Array("modules", () =>
            {
                json.Object("", () =>
                {
                    json.Property("description", $"{Name} Resources");
                    json.Property("type", "resources");
                    json.Property("uuid", Guid.NewGuid().ToString());
                    json.Property("version", new Version(1, 0, 0).AsArray());
                });
            });
            
            json.Array("dependencies", () =>
            {
                if (LinkPacks) json.Object("", () =>
                {
                    json.Property("uuid", BehaviourPack.Uuid);
                    json.Property("version", BehaviourPack.BehaviourPackVersion.AsArray());
                });
            });
            
            json.Object("metadata", () =>
            {
                json.Property("authors", Authors);
                json.Object("generated_with", () =>
                {
                    json.Property("ingot", "https://github.com/pyroboots/ingot");
                });
            });
            
            w.WriteEndObject();
            
            File.WriteAllText(Path.Combine(outputDir, "rp", "manifest.json"), sw.ToString());
        }
        CompilerState.Info("compiled rp manifest");
        
        if (PackIcon is not null)
        {
            File.Copy(PackIcon, Path.Combine(outputDir, "bp"));
            File.Copy(PackIcon, Path.Combine(outputDir, "rp"));
        }
        
        timer.Stop();

        if (verbose)
        {
            File.WriteAllText(Path.Combine(outputDir, "ingot.log"), string.Join('\n', CompilerState.GetLogs()));
            Console.WriteLine();
            CompilerState.Info($"pack compiled in {timer.ElapsedMilliseconds}ms");
            CompilerState.Info($"ingot compilation log available at {Path.Combine(outputDir, "ingot.log")}");
        }
        
        CompilerState.ShowInfoLogs = false;
        CompilerState.CurrentPack = null;
        CompilerState.Pop();
    }
}