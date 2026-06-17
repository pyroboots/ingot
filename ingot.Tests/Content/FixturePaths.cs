namespace ingot.Tests.Content;

internal static class FixturePaths
{
    public static string Resolve(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
}