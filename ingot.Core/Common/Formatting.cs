using System.Text;

namespace ingot.Core.Common;

public class Formatting
{
    public static string SnakeToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) 
            return input;

        string[] words = input.Split('_', StringSplitOptions.RemoveEmptyEntries);
        StringBuilder result = new StringBuilder();

        foreach (string word in words)
        {
            if (word.Length > 0)
                result.Append(char.ToUpperInvariant(word[0])).Append(word[1..].ToLowerInvariant());
        }

        return result.ToString();
    }
    
    public static string PascalToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        StringBuilder sb = new();
        sb.Append(char.ToLowerInvariant(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            char current = input[i];

            if (char.IsUpper(current))
            {
                if (sb.Length > 0 && 
                    (char.IsLower(input[i-1]) || 
                     (i + 1 < input.Length && char.IsLower(input[i+1]))))
                {
                    sb.Append('_');
                }
                sb.Append(char.ToLowerInvariant(current));
            }
            else
            {
                sb.Append(current);
            }
        }

        return sb.ToString();
    }
}