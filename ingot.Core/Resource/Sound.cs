namespace ingot.Core;

/// <summary>
/// A single sound file entry within a resource-pack sound definition
/// (<c>rp/sounds/sound_definitions.json</c>).
/// </summary>
public sealed class Sound
{
    /// <summary>
    /// Path to the sound file relative to the resource pack root, without file extension
    /// (e.g. <c>sounds/ambient/nether/basalt_deltas/basaltground1</c>).
    /// When <see cref="SourcePath"/> is set, the source file is copied to this location under the RP.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Optional path to a source sound file on disk (e.g. <c>.ogg</c>, <c>.fsb</c>, <c>.wav</c>).
    /// When set, the file is copied into the resource pack at <see cref="Name"/> plus the source extension,
    /// creating intermediate directories as needed.
    /// </summary>
    public string? SourcePath { get; init; }

    /// <summary>
    /// Playback volume for this variant. Defaults to <c>1.0</c> when omitted in game.
    /// </summary>
    public float? Volume { get; init; }

    /// <summary>
    /// Relative chance this variant is selected when the sound event plays.
    /// Defaults to <c>1</c> when omitted in game.
    /// </summary>
    public int? Weight { get; init; }

    /// <summary>
    /// Whether the sound is spatialized in 3D. Serialized as <c>is3D</c>.
    /// </summary>
    public bool? Is3D { get; init; }

    /// <summary>
    /// When <see langword="true"/>, the sound is streamed from disk.
    /// Recommended for long audio (music, ambient loops, records).
    /// </summary>
    public bool? Stream { get; init; }

    /// <summary>
    /// Playback pitch for this variant. Defaults to <c>1.0</c> when omitted in game.
    /// </summary>
    public float? Pitch { get; init; }

    /// <summary>
    /// Creates a sound entry with the given resource path and optional playback options.
    /// </summary>
    /// <param name="name">Path under the resource pack root, without extension.</param>
    /// <param name="sourcePath">Optional source audio file to copy into the pack at <paramref name="name"/>.</param>
    /// <param name="volume">Optional playback volume.</param>
    /// <param name="weight">Optional selection weight.</param>
    /// <param name="is3D">Optional 3D spatialization flag.</param>
    /// <param name="stream">Optional stream-from-disk flag.</param>
    /// <param name="pitch">Optional playback pitch.</param>
    public static Sound Create(
        string name,
        string? sourcePath = null,
        float? volume = null,
        int? weight = null,
        bool? is3D = null,
        bool? stream = null,
        float? pitch = null) =>
        new()
        {
            Name = name,
            SourcePath = sourcePath,
            Volume = volume,
            Weight = weight,
            Is3D = is3D,
            Stream = stream,
            Pitch = pitch,
        };
}
