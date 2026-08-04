namespace ingot.Generators;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        string output = args.Length > 0
            ? args[0]
            : Path.Combine(AppContext.BaseDirectory, "out");

        string? token = Environment.GetEnvironmentVariable("gh_api_token")
                        ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        if (string.IsNullOrWhiteSpace(token))
        {
            Console.Error.WriteLine("set gh_api_token (or GITHUB_TOKEN) to a github personal access token");
            Environment.ExitCode = 1;
            return;
        }

        await TraitGeneratorV2.GenerateItemTraits(output, token);
    }
}
