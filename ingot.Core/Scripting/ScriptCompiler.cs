using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Item;

using Newtonsoft.Json;

using Version = ingot.Core.Common.Version;

namespace ingot.Core.Scripting;

/// <summary>
/// Dedicated Script API compilation pass that runs after behaviour-pack JSON is written.
/// </summary>
internal static class ScriptCompiler
{
    /// <summary>
    /// Collects generated scripts from pack content and writes all script files plus <c>main.js</c>.
    /// </summary>
    /// <returns>Whether the behaviour pack requires a script module in the manifest.</returns>
    public static bool Compile(Pack pack, string behaviourPackDir)
    {
        JsonTextWriter? writer = null;
        bool hasEventBindings = CollectEventScripts(pack, ref writer);

        bool hasServices = pack.Services.Count > 0;
        bool hasScriptEvents = pack.ScriptEvents.Count > 0;

        if (!pack.ScriptsEnabled)
        {
            if (hasEventBindings)
                CompilerState.Warn(ref writer, "content defines Script API events but ScriptsEnabled is false");

            if (hasServices)
                CompilerState.Warn(ref writer, "services are registered but ScriptsEnabled is false; services will not be compiled");

            if (hasScriptEvents)
                CompilerState.Warn(ref writer, "script events are registered but ScriptsEnabled is false; script events will not be compiled");

            return false;
        }

        foreach (ScriptServiceRegistration service in pack.Services)
            pack.ScriptRegistry.RegisterService(service.SourceFile, service.RelativePath, service.IntervalTicks);

        foreach (ScriptEventRegistration scriptEvent in pack.ScriptEvents)
        {
            string body = scriptEvent.Handler.ResolveBody();
            string code = ScriptEventGenerator.Generate(scriptEvent.EventId, body);
            pack.ScriptRegistry.RegisterGenerated(scriptEvent.RelativePath, code);
            CompilerState.Info($"registered script event {scriptEvent.EventId}");
        }

        if (!pack.ScriptRegistry.HasEntries)
            return false;

        EnsureDefaultScriptModules(pack);
        WriteScripts(pack, behaviourPackDir);
        return true;
    }

    /// <summary>
    /// Guarantees <c>@minecraft/server</c> is listed even if the author replaced
    /// <see cref="Pack.ScriptApiModules"/> with only optional modules like server-ui.
    /// </summary>
    private static void EnsureDefaultScriptModules(Pack pack)
    {
        if (!pack.ScriptApiModules.ContainsKey("@minecraft/server"))
            pack.ScriptApiModules["@minecraft/server"] = new(2, 8, 0);
    }

    private static bool CollectEventScripts(Pack pack, ref JsonTextWriter? writer)
    {
        bool hasEventBindings = false;

        foreach (Block block in pack.BehaviourPack.Blocks)
        {
            if (block.BlockEvents is not { HasEvents: true } events)
                continue;

            hasEventBindings = true;
            Type blockType = block.GetType();
            ScriptEventValidator.ValidateBlock(blockType, events, ref writer);

            if (!pack.ScriptsEnabled)
                continue;

            (string _, string code) = ScriptEventsGenerator.Generate(block.Identifier, events);
            pack.ScriptRegistry.RegisterGenerated(events.GetScriptPath(block.Identifier), code);
            CompilerState.Info($"registered block event script for {block.Identifier}");
        }

        foreach (Item item in pack.BehaviourPack.Items)
        {
            if (item.ItemEvents is not { HasEvents: true } events)
                continue;

            hasEventBindings = true;
            Type itemType = item.GetType();
            ScriptEventValidator.ValidateItem(itemType, events, ref writer);

            if (!pack.ScriptsEnabled)
                continue;

            (string _, string code) = ScriptEventsGenerator.Generate(item.Identifier, events);
            pack.ScriptRegistry.RegisterGenerated(events.GetScriptPath(item.Identifier), code);
            CompilerState.Info($"registered item event script for {item.Identifier}");
        }

        return hasEventBindings;
    }

    private static void WriteScripts(Pack pack, string behaviourPackDir)
    {
        foreach (ScriptEntry entry in pack.ScriptRegistry.Entries)
        {
            string path = Path.Combine(behaviourPackDir, entry.RelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            string content = entry.Kind switch
            {
                ScriptEntryKind.Generated => entry.GeneratedContent!,
                ScriptEntryKind.Service => ScriptServiceGenerator.Generate(
                    File.ReadAllText(entry.SourceFilePath!),
                    entry.ServiceIntervalTicks),
                _ => throw new InvalidOperationException($"unknown script entry kind: {entry.Kind}"),
            };

            File.WriteAllText(path, content);
            CompilerState.Info($"wrote {entry.Kind.ToString().ToLower()} script {entry.RelativePath}");
        }

        string entryPath = Path.Combine(behaviourPackDir, pack.ScriptEntry);
        Directory.CreateDirectory(Path.GetDirectoryName(entryPath)!);

        using StringWriter sw = new();
        sw.WriteLine("// autogenerated by ingot");
        foreach (KeyValuePair<string, Version> module in pack.ScriptApiModules)
        {
            if (module.Key == "@minecraft/server")
                sw.WriteLine("import {world, system} from \"@minecraft/server\";");
            else sw.WriteLine($"import \"{module.Key}\";");
        }

        foreach (ScriptEntry entry in pack.ScriptRegistry.Entries)
        {
            string importPath = "./" + entry.RelativePath["scripts/".Length..];
            sw.WriteLine($"import \"{importPath}\";");
        }

        if (pack.ScriptEntryBody is not null)
        {
            sw.WriteLine();
            sw.WriteLine(pack.ScriptEntryBody.Value.ResolveBody());
        }
        
        File.WriteAllText(entryPath, sw.ToString());
        CompilerState.Info($"wrote script entry {pack.ScriptEntry}");
    }
}