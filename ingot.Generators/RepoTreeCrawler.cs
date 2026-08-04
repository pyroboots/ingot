using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace ingot.Generators;

public class RepoTreeCrawler
{
    private record Tree(string sha, string url, TreeLeaf[] tree, bool truncated);
    private record TreeLeaf(string path, string mode, string type, string sha, string url, int size = -1);
    private record Blob(string sha, string node_id, int size, string url, string content, string encoding);

    public static async Task<string> GetTree(
        string repoOwner,
        string repoName,
        string path,
        string baseSha = "main",
        string? token = null)
    {
        using HttpClient http = CreateClient(token);

        string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        string treeUrl = $"https://api.github.com/repos/{repoOwner}/{repoName}/git/trees/{baseSha}";
        Tree tree = JsonConvert.DeserializeObject<Tree>(await http.GetStringAsync(treeUrl))!;

        foreach (string dir in segments)
        {
            TreeLeaf? target = tree.tree.FirstOrDefault(leaf =>
                leaf.type == "tree" && leaf.path == dir);

            if (target is null)
                throw new InvalidOperationException($"directory '{dir}' not found in path '{path}'");

            treeUrl = target.url;
            tree = JsonConvert.DeserializeObject<Tree>(await http.GetStringAsync(treeUrl))!;
        }

        return await http.GetStringAsync($"{treeUrl}?recursive=1");
    }

    public static async Task<string[]> GetFileContents(string treeJson, string? token = null)
    {
        (string Path, string Content)[] withPaths = await GetFileContentsWithPaths(treeJson, token);
        return withPaths.Select(x => x.Content).ToArray();
    }

    /// <summary>
    /// Fetches blob contents for every file leaf in <paramref name="treeJson"/>, preserving tree paths.
    /// </summary>
    public static async Task<(string Path, string Content)[]> GetFileContentsWithPaths(
        string treeJson,
        string? token = null)
    {
        Tree tree = JsonConvert.DeserializeObject<Tree>(treeJson)!;

        TreeLeaf[] fileLeaves = tree.tree
            .Where(leaf => leaf.type == "blob")
            .ToArray();

        using HttpClient http = CreateClient(token);

        var results = new (string Path, string Content)[fileLeaves.Length];
        ParallelOptions options = new() { MaxDegreeOfParallelism = 6 };

        await Parallel.ForEachAsync(
            fileLeaves.Select((leaf, i) => (leaf, i)),
            options,
            async (item, ct) =>
            {
                (TreeLeaf leaf, int index) = item;

                string blobJson = await http.GetStringAsync(leaf.url, ct);
                Blob blob = JsonConvert.DeserializeObject<Blob>(blobJson)!;

                if (blob.encoding != "base64")
                    throw new InvalidOperationException($"unexpected encoding {blob.encoding}");

                // GitHub inserts newlines (and occasionally other whitespace) in base64
                string clean = blob.content
                    .Replace("\n", "")
                    .Replace("\r", "")
                    .Replace(" ", "");

                byte[] bytes = Convert.FromBase64String(clean);
                results[index] = (leaf.path, Encoding.UTF8.GetString(bytes));
            });

        return results;
    }

    private static HttpClient CreateClient(string? token)
    {
        HttpClient http = new();
        http.DefaultRequestHeaders.Add("User-Agent", "ingot");
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        if (!string.IsNullOrWhiteSpace(token))
        {
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return http;
    }
}