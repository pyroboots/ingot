using System.Diagnostics;
using ingot.Core.Content;
using ingot.Core.Content.Block;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;
using Version = ingot.Core.Common.Version;

namespace ingot.Core;

/// <summary>
/// C# representation of a full pack with behaviour and resources
/// </summary>
public class Pack
{
    public required string Name;
    public required string Description;
    public string? PackIcon = null;
    public Version PackVersion = new(1, 0, 0);
    public Version MinEngineVersion = new(1, 20, 0);
    public string[] Authors = [];
    
    public bool ScriptsEnabled = false;
    public string ScriptEntry = "scripts/main.js";
    public Dictionary<string, Version> ScriptApiModules = new()
    {
        ["@minecraft/server"] = new(2, 8, 0),
    };
    public Version ScriptApiVersion = new(2, 8, 0);

    public required BehaviourPack BehaviourPack;
    public required ResourcePack ResourcePack;
    public bool LinkPacks = true;

    /// <summary>
    /// Compiles both <see cref="BehaviourPack"/> and <see cref="ResourcePack"/> and generates pack manifests
    /// </summary>
    /// <param name="outputDir">Output directory to place the behaviour pack and resource pack</param>
    /// <param name="verbose">Whether to print info logs to the console</param>
    public void Compile(string outputDir, bool verbose = true)
    {
        Stopwatch timer = Stopwatch.StartNew();
        
        CompileTimeLogging.Push(Name);
        CompileTimeLogging.ShowInfoLogs = verbose;
        CompileTimeLogging.Info("pack compilation started");
        
        CompileTimeLogging.Info("compiling bp...");
        BehaviourPack.Compile(Path.Combine(outputDir, "bp"));
        CompileTimeLogging.Info($"compiled bp");
        
        CompileTimeLogging.Info("compiling rp...");
        ResourcePack.Compile(Path.Combine(outputDir, "rp"));
        CompileTimeLogging.Info($"compiled rp");
        
        using (StringWriter sw = new())
        {
            JsonTextWriter w = new(sw);
            w.Formatting = Formatting.Indented;
            w.Indentation = 4;
    
            w.WriteStartObject();
            
            Property(ref w, "format_version", 2);
            Object(ref w, "header", w =>
            {
                Property(ref w, "name", Name);
                Property(ref w, "description", Description);
                Property(ref w, "uuid", BehaviourPack.Uuid);
                Property(ref w, "version", PackVersion.AsArray());
                Property(ref w, "min_engine_version", MinEngineVersion.AsArray());
            });
            
            Array(ref w, "modules", w =>
            {
                Object(ref w, "", w =>
                {
                    Property(ref w, "description", $"{Name} Behaviour");
                    Property(ref w, "type", "data");
                    Property(ref w, "uuid", Guid.NewGuid().ToString());
                    Property(ref w, "version", new Version(1, 0, 0).AsArray());
                });
                if (ScriptsEnabled) Object(ref w, "", w =>
                {
                    Property(ref w, "type", "script");
                    Property(ref w, "language", "javascript");
                    Property(ref w, "uuid", Guid.NewGuid().ToString());
                    Property(ref w, "entry", ScriptEntry);
                    Property(ref w, "version", new Version(1, 0, 0).AsArray());
                });
            });
            
            Array(ref w, "dependencies", w =>
            {
                if (LinkPacks) Object(ref w, "", w =>
                {
                    Property(ref w, "uuid", ResourcePack.Uuid);
                    Property(ref w, "version", ResourcePack.ResourcePackVersion.AsArray());
                });
                if (ScriptsEnabled) foreach (var kvp in ScriptApiModules)
                {
                    Object(ref w, "", w =>
                    {
                        Property(ref w, "module_name", kvp.Key);
                        Property(ref w, "version", kvp.Value.AsArray());
                    });
                }
            });
            
            Object(ref w, "metadata", w =>
            {
                Property(ref w, "authors", Authors);
                Object(ref w, "generated_with", w =>
                {
                    Property(ref w, "ingot", "https://github.com/pyroboots/ingot");
                });
            });
            
            w.WriteEndObject();
            
            File.WriteAllText(Path.Combine(outputDir, "bp", "manifest.json"), sw.ToString());
        }
        CompileTimeLogging.Info("compiled bp manifest");
        
        using (StringWriter sw = new())
        {
            JsonTextWriter w = new(sw);
            w.Formatting = Formatting.Indented;
            w.Indentation = 4;
    
            w.WriteStartObject();
            
            Property(ref w, "format_version", 2);
            Object(ref w, "header", w =>
            {
                Property(ref w, "name", Name);
                Property(ref w, "description", Description);
                Property(ref w, "uuid", ResourcePack.Uuid);
                Property(ref w, "version", PackVersion.AsArray());
                Property(ref w, "min_engine_version", MinEngineVersion.AsArray());
            });
            
            Array(ref w, "modules", w =>
            {
                Object(ref w, "", w =>
                {
                    Property(ref w, "description", $"{Name} Resources");
                    Property(ref w, "type", "resources");
                    Property(ref w, "uuid", Guid.NewGuid().ToString());
                    Property(ref w, "version", new Version(1, 0, 0).AsArray());
                });
            });
            
            Array(ref w, "dependencies", w =>
            {
                if (LinkPacks) Object(ref w, "", w =>
                {
                    Property(ref w, "uuid", BehaviourPack.Uuid);
                    Property(ref w, "version", BehaviourPack.BehaviourPackVersion.AsArray());
                });
            });
            
            Object(ref w, "metadata", w =>
            {
                Property(ref w, "authors", Authors);
                Object(ref w, "generated_with", w =>
                {
                    Property(ref w, "ingot", "https://github.com/pyroboots/ingot");
                });
            });
            
            w.WriteEndObject();
            
            File.WriteAllText(Path.Combine(outputDir, "rp", "manifest.json"), sw.ToString());
        }
        CompileTimeLogging.Info("compiled rp manifest");
        
        if (PackIcon is not null)
        {
            File.Copy(PackIcon, Path.Combine(outputDir, "bp"));
            File.Copy(PackIcon, Path.Combine(outputDir, "rp"));
        }
        
        timer.Stop();

        if (verbose)
        {
            File.WriteAllText(Path.Combine(outputDir, "ingot.log"), string.Join('\n', CompileTimeLogging.GetLogs()));
            Console.WriteLine();
            CompileTimeLogging.Info($"pack compiled in {timer.ElapsedMilliseconds}ms");
            CompileTimeLogging.Info($"ingot compilation log available at {Path.Combine(outputDir, "ingot.log")}");
        }
        
        CompileTimeLogging.ShowInfoLogs = false;
        CompileTimeLogging.Pop();
    }
}