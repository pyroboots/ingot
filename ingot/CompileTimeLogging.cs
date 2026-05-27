using Newtonsoft.Json;

namespace ingot;

public static class CompileTimeLogging
{
    private static Stack<string> _traceStack = new(["pack"]);
    private static List<string> _logs = new();
    public static void Push(string trace) => _traceStack.Push(trace);
    public static void Pop() => _traceStack.Pop();

    private static string _getTrace() => string.Join('/', _traceStack.ToArray().Reverse());

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
}