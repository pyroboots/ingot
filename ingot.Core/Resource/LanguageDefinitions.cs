using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Item;

using Newtonsoft.Json;

namespace ingot.Core.Resource;

/// <summary>
/// Language files written under <c>texts/</c> in the resource pack
/// (<c>languages.json</c> and per-locale <c>.lang</c> files).
/// </summary>
public class LanguageDefinitions
{
    /// <summary>
    /// Locales listed in <c>texts/languages.json</c>.
    /// </summary>
    public List<string> Languages { get; } = ["en_US"];

    /// <summary>
    /// Locale → lang-file lines (<c>key=value</c>).
    /// </summary>
    public Dictionary<string, List<string>> Entries { get; } = new()
    {
        ["en_US"] = [],
    };

    /// <summary>
    /// Adds a lang-file line for the given locale.
    /// </summary>
    public void Add(string entry, string locale = "en_US")
    {
        if (!Entries.TryGetValue(locale, out List<string>? lines))
        {
            lines = [];
            Entries[locale] = lines;
        }

        if (!Languages.Contains(locale))
            Languages.Add(locale);

        lines.Add(entry);
    }

    internal void SeedFromPack(Pack pack)
    {
        foreach (Block block in pack.BehaviourPack.Blocks)
        {
            if (block.LangName is not null)
                Add($"tile.{block.Identifier}.name={block.LangName}");
        }

        foreach (Item item in pack.BehaviourPack.Items)
        {
            if (item.DisplayName is not null)
                Add($"item.{item.Identifier}.name={item.DisplayName}");
        }
    }

    internal void Write(string resourcePackDir)
    {
        CompilerState.Push("texts");

        string textsDir = Path.Combine(resourcePackDir, "texts");
        Directory.CreateDirectory(textsDir);

        File.WriteAllText(
            Path.Combine(textsDir, "languages.json"),
            JsonConvert.SerializeObject(Languages, Newtonsoft.Json.Formatting.Indented) + Environment.NewLine);

        foreach (string locale in Languages)
        {
            Entries.TryGetValue(locale, out List<string>? lines);
            lines ??= [];

            string body = lines.Count > 0
                ? string.Join('\n', lines) + '\n'
                : string.Empty;
            File.WriteAllText(Path.Combine(textsDir, $"{locale}.lang"), body);

            CompilerState.Info(
                lines.Count > 0
                    ? $"wrote {locale}.lang with {lines.Count} entries"
                    : $"wrote empty {locale}.lang");
        }

        CompilerState.Pop();
    }
}
