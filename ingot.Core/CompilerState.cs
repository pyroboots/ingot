using Newtonsoft.Json;

namespace ingot.Core;

/// <summary>
/// Internal use class for emitting compile time logs
/// </summary>
public static class CompilerState
{
    private static Stack<string> _traceStack = new(["pack"]);
    private static List<string> _logs = new();
    /// <summary>
    /// Push a new section onto the trace stack
    /// </summary>
    /// <param name="trace">Name of the section to show up in any produced logs or warnings</param>
    public static void Push(string trace) => _traceStack.Push(trace);
    /// <summary>
    /// Ends the previous section
    /// </summary>
    public static void Pop() => _traceStack.Pop();
    /// <summary>
    /// Whether logs produced by <see cref="Info"/> appear in the console
    /// </summary>
    public static bool ShowInfoLogs = false;

    /// <summary>
    /// Current pack being compiled. Useful for <see cref="BehaviourPack"/> to generate content for <see cref="ResourcePack"/> and vice versa
    /// </summary>
    public static Pack? CurrentPack = null; 

    private static string _getTrace() => string.Join('/', _traceStack.ToArray().Reverse());

    /// <summary>
    /// Writes a warning to the console and in the JSON output
    /// </summary>
    /// <param name="w">Used to write the warning in the JSON source</param>
    /// <param name="msg">Message to write</param>
    public static void Warn(ref JsonTextWriter? w, string msg)
    {
        string warning = $"/!\\ [{_getTrace()}] {msg}";
        _logs.Add(warning);
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(warning);
        Console.ResetColor();

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
        string log = $"(i) [{_getTrace()}] {msg}";
        _logs.Add(log);
        
        if (ShowInfoLogs) Console.WriteLine(log);
    }
    
    /// <summary>
    /// Returns logs as a list
    /// </summary>
    public static List<string> GetLogs() => _logs;
}