namespace ingot.Core.Scripting;

/// <summary>
/// A Script API event handler body, either inline or loaded from a file.
/// </summary>
public readonly record struct ScriptHandler
{
    /// <summary>Inline JavaScript handler body.</summary>
    public string? InlineBody { get; init; }

    /// <summary>Absolute or relative path to a file containing the handler body.</summary>
    public string? FilePath { get; init; }

    /// <summary>Whether this handler is configured.</summary>
    public bool IsConfigured => InlineBody is not null || FilePath is not null;

    /// <summary>Creates an inline handler body.</summary>
    public static ScriptHandler Inline(string body) => new() { InlineBody = body };

    /// <summary>Loads a handler body from a file at compile time.</summary>
    public static ScriptHandler FromFile(string path) => new() { FilePath = path };

    /// <summary>Resolves the handler body from inline content or file.</summary>
    public string ResolveBody()
    {
        if (InlineBody is not null)
            return InlineBody;

        if (FilePath is not null)
        {
            string resolved = Path.GetFullPath(FilePath);
            if (!File.Exists(resolved))
                throw new FileNotFoundException($"script handler file not found: {FilePath}", resolved);

            return File.ReadAllText(resolved);
        }

        throw new InvalidOperationException("ScriptHandler has no content configured.");
    }

    /// <summary>Allows assigning raw JavaScript strings to handler properties.</summary>
    public static implicit operator ScriptHandler(string body) => Inline(body);
}