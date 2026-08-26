namespace ingot.Core.Scripting;

/// <summary>
/// Pulls leading ESM <c>import</c> statements out of a handler body so they can be
/// written at the top of a generated script instead of inside a function.
/// </summary>
internal static class ScriptImportHoist
{
    /// <summary>
    /// Splits <paramref name="source"/> into leading <c>import</c> statements and the
    /// remaining handler body. Line comments and blank lines before/between imports
    /// are skipped. Dynamic <c>import()</c> is left in the body.
    /// </summary>
    public static (IReadOnlyList<string> Imports, string Body) Split(string source)
    {
        if (string.IsNullOrEmpty(source))
            return ([], source);

        List<string> imports = [];
        int i = 0;
        int bodyStart = 0;

        while (i < source.Length)
        {
            SkipTrivia(source, ref i);
            if (i >= source.Length)
                break;

            if (!IsStaticImport(source, i))
                break;

            int semi = source.IndexOf(';', i);
            if (semi < 0)
                break;

            imports.Add(source[i..(semi + 1)].Trim());
            i = semi + 1;
            bodyStart = i;
        }

        string body = source[bodyStart..];
        return (imports, body);
    }

    private static void SkipTrivia(string source, ref int i)
    {
        while (i < source.Length)
        {
            char c = source[i];
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (c == '/' && i + 1 < source.Length)
            {
                char next = source[i + 1];
                if (next == '/')
                {
                    int newline = source.IndexOf('\n', i + 2);
                    i = newline < 0 ? source.Length : newline + 1;
                    continue;
                }

                if (next == '*')
                {
                    int end = source.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    i = end < 0 ? source.Length : end + 2;
                    continue;
                }
            }

            break;
        }
    }

    private static bool IsStaticImport(string source, int i)
    {
        const string keyword = "import";
        if (i + keyword.Length > source.Length)
            return false;
        if (!source.AsSpan(i).StartsWith(keyword, StringComparison.Ordinal))
            return false;

        int after = i + keyword.Length;
        if (after >= source.Length)
            return false;

        char c = source[after];
        return char.IsWhiteSpace(c) || c is '{' or '"' or '\'' or '*';
    }
}
