using ingot.Core.Common;

using Newtonsoft.Json;

using Spectre.Console;

namespace ingot.Core;

/// <summary>
/// Internal use class for emitting compile time logs
/// </summary>
public static class CompilerState
{
    private static readonly AsyncLocal<CompilerContext> Context = new();

    private static CompilerContext Current => Context.Value ??= new();

    /// <summary>
    /// Whether logs produced by <see cref="Info"/> appear in the console
    /// </summary>
    public static bool ShowInfoLogs
    {
        get => Current.ShowInfoLogs;
        set => Current.ShowInfoLogs = value;
    }

    /// <summary>
    /// Current pack being compiled. Useful for <see cref="BehaviourPack"/> to generate content for <see cref="ResourcePack"/> and vice versa
    /// </summary>
    public static Pack? CurrentPack
    {
        get => Current.CurrentPack;
        set => Current.CurrentPack = value;
    }
    
    /// <summary>
    /// Contains cached information of pack contents
    /// </summary>
    public static IngotCache? Cache
    {
        get => Current.Cache;
        set => Current.Cache = value;
    }

    /// <summary>
    /// Clears accumulated logs and resets compile-time state before a new pack compilation.
    /// </summary>
    public static void Reset()
    {
        Context.Value = new CompilerContext();
    }

    /// <summary>
    /// Push a new section onto the trace stack
    /// </summary>
    /// <param name="trace">Name of the section to show up in any produced logs or warnings</param>
    public static void Push(string trace) => Current.TraceStack.Push(trace);

    /// <summary>
    /// Ends the previous section
    /// </summary>
    public static void Pop() => Current.TraceStack.Pop();

    private static string GetTrace() =>
        string.Join('/', Current.TraceStack.ToArray().Reverse());

    /// <summary>
    /// Writes a warning to the console and in the JSON output
    /// </summary>
    /// <param name="w">Used to write the warning in the JSON source</param>
    /// <param name="msg">Message to write</param>
    public static void Warn(ref JsonTextWriter? w, string msg)
    {
        string warning = $"/!\\ [{GetTrace()}] {msg}";
        Current.Logs.Add(warning);
        
        AnsiConsole.MarkupLine($"[{IngotCommon.SecondaryColor.ToMarkup()} bold blink underline]{warning.EscapeMarkup()}[/]");

        if (w is not null)
        {
            w.WriteWhitespace("\n");
            w.WriteComment(msg);
        }
    }

    /// <summary>
    /// Logs information in the compilation process
    /// </summary>
    /// <param name="msg">Info</param>
    public static void Info(string msg)
    {
        string log = $"(i) [{GetTrace()}] {msg}";
        Current.Logs.Add(log);

        if (ShowInfoLogs)
            AnsiConsole.MarkupLine($"[{IngotCommon.PrimaryColor.ToMarkup()} dim]{log.EscapeMarkup()}[/]");
    }

    /// <summary>
    /// Returns logs as a list
    /// </summary>
    public static List<string> GetLogs() => Current.Logs;

    private sealed class CompilerContext
    {
        public Stack<string> TraceStack = new(["pack"]);
        public List<string> Logs = new();
        public bool ShowInfoLogs;
        public Pack? CurrentPack;
        public IngotCache? Cache;
    }
}

/// <summary>
/// Cache of content at compile time
/// </summary>
public struct IngotCache
{
    [JsonProperty("rpUuid")]
    public string ResourceUuid;
    [JsonProperty("bpUuid")]
    public string BehaviourUuid;
    
    [JsonProperty("entities")]
    public string[] Entities;
    [JsonProperty("blocks")]
    public string[] Blocks;
    [JsonProperty("items")]
    public string[] Items;
}