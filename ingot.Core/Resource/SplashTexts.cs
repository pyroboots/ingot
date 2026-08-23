using Newtonsoft.Json;

namespace ingot.Core.Resource;

/// <summary>
/// Represents <c>splashes.json</c> in the resource pack.
/// </summary>
public class SplashTexts
{
    /// <summary>
    /// Whether this file may be merged with lower-priority resource packs.
    /// </summary>
    [JsonProperty("canMerge")]
    public bool CanMerge = false;

    /// <summary>
    /// Splash texts shown on the title screen.
    /// </summary>
    [JsonProperty("splashes")]
    public List<string> Splashes = [];
}
