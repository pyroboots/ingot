using System.Collections.ObjectModel;

using ingot.Core.Common;

using Newtonsoft.Json;

namespace ingot.Core.Resource;

internal class SoundDefinitionsJsonConverter : JsonConverter<SoundDefinitions>
{
    public override void WriteJson(JsonWriter writer, SoundDefinitions? value, JsonSerializer serializer)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        
        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Property("format_version", "1.20.20");
            json.Object("sound_definitions", () =>
            {
                foreach (SoundDefinitions.SoundDefinition definition in value.Definitions)
                    serializer.Serialize(writer, definition);
            });
        });
    }

    public override SoundDefinitions? ReadJson(JsonReader reader, Type objectType, SoundDefinitions? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        throw new InvalidOperationException();
    }
}

/// <summary>
/// Represents <c>sound_definitions.json</c> in the resource pack
/// </summary>
[JsonConverter(typeof(SoundDefinitionsJsonConverter))]
public class SoundDefinitions
{
    private class SoundDefinitionJsonConverter : JsonConverter<SoundDefinition>
    {
        public override void WriteJson(JsonWriter writer, SoundDefinition value, JsonSerializer serializer)
        {
            JsonHelper json = new(ref writer);
            json.Object(value.Identifier, () =>
            {
                json.Property("category", ingot.Core.Common.Formatting.PascalToSnakeCase(value.Category.ToString()));

                writer.WritePropertyName("max_distance");
                if (value.MaxDistance is null)
                    writer.WriteNull();
                else
                    writer.WriteValue(value.MaxDistance.Value);

                writer.WritePropertyName("min_distance");
                if (value.MinDistance is null)
                    writer.WriteNull();
                else
                    writer.WriteValue(value.MinDistance.Value);

                json.Array("sounds", () =>
                {
                    foreach (SoundDefinition.Sound sound in value.Sounds)
                        serializer.Serialize(writer, sound);
                });
            });
        }

        public override SoundDefinition ReadJson(JsonReader reader, Type objectType, SoundDefinition existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new InvalidOperationException();
        }
    }
    /// <summary>
    /// Represents a sounds definition is <c>sound_definitions.json</c> in the resource pack
    /// </summary>
    /// <param name="identifier"></param>
    [JsonConverter(typeof(SoundDefinitionJsonConverter))]
    public struct SoundDefinition(string identifier)
    {
        /// <summary>
        /// Identifier of the sound definition
        /// </summary>
        public string Identifier = identifier;
        /// <summary>
        /// Sound definition category. Determines what audio settings slider affects this sound
        /// </summary>
        public SoundDefinitionCategory Category = SoundDefinitionCategory.Neutral;

        /// <summary>
        /// The maximum distance in blocks at which this sound can be heard
        /// </summary>
        public int? MaxDistance
        {
            get; set
            {
                if (value != null && Category == SoundDefinitionCategory.Ui)
                    CompilerState.Warn("sound max distance has no effect on UI sfx");
                field = value;
            }
        }
        /// <summary>
        /// The minimum distance in blocks at which this sound can be heard
        /// </summary>
        public int? MinDistance
        {
            get; set
            {
                if (value != null && Category == SoundDefinitionCategory.Ui)
                    CompilerState.Warn("sound min distance has no effect on UI sfx");
                field = value;
            }
        }
        
        /// <summary>
        /// Array of possible sounds to play
        /// </summary>
        public Sound[] Sounds = [];

        internal void EnsureSoundNames()
        {
            for (int i = 0; i < Sounds.Length; i++)
            {
                Sound sound = Sounds[i];
                sound.EnsureName(this);
                Sounds[i] = sound;
            }
        }
        
        /// <summary>
        /// Sound definition category. Determines what audio settings slider affects this sound
        /// </summary>
        public enum SoundDefinitionCategory
        {
            /// <summary>Used for general sounds that do not pertain to a specific category</summary>
            Neutral,
            /// <summary>Used for environment related sounds that are not weather</summary>
            Ambient,
            /// <summary>Used for sounds that are block related</summary>
            Block,
            /// <summary>Used for sounds that are related to a hostile mob</summary>
            Hostile,
            /// <summary>
            /// Used for sounds that are related to in-game music.
            /// For sounds played by music discs, use <see cref="Record"/>
            /// </summary>
            Music,
            /// <summary>Used for sounds that are player related</summary>
            Player,
            /// <summary>Used for sounds that are music from a music disc</summary>
            Record,
            /// <summary>
            /// Used for sounds that are related to user interfaces.
            /// Sounds in this category will ignore range limit
            /// </summary>
            Ui,
            /// <summary>Used for environment related sounds</summary>
            Weather
        }

        /// <summary>
        /// Represents a single sound entry in a sound definition
        /// </summary>
        /// <param name="path">
        /// Path to an audio file on disk (e.g. <c>.ogg</c>, <c>.wav</c>). When set, the file is
        /// copied into the resource pack and <see cref="Name"/> is auto-resolved if omitted.
        /// Pass <see cref="string.Empty"/> with an explicit <paramref name="name"/> for a
        /// reference-only entry (no file copy).
        /// </param>
        /// <param name="name">
        /// Optional location in the resource pack (extensionless, e.g.
        /// <c>sounds/ambient/nether/basalt_deltas/basaltground1</c>). Serialized as <c>name</c>.
        /// When null, derived from <paramref name="path"/> and the definition category.
        /// </param>
        public struct Sound(string path, string? name = null)
        {
            private static readonly ReadOnlyDictionary<SoundDefinitionCategory, string> CategoryPaths =
                new(new Dictionary<SoundDefinitionCategory, string>
                {
                    [SoundDefinitionCategory.Neutral] = "random",
                    [SoundDefinitionCategory.Ambient] = "ambient",
                    [SoundDefinitionCategory.Block] = "block",
                    [SoundDefinitionCategory.Hostile] = "mob",
                    [SoundDefinitionCategory.Music] = "music",
                    [SoundDefinitionCategory.Player] = "mob/player",
                    [SoundDefinitionCategory.Record] = "music/game/records",
                    [SoundDefinitionCategory.Ui] = "ui",
                    [SoundDefinitionCategory.Weather] = "ambient/weather",
                });

            internal void EnsureName(SoundDefinition ctx)
            {
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    Name = NormalizeRpName(Name);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Path))
                    throw new ArgumentException(
                        "sound Path is required when Name is not set", nameof(Path));

                if (!File.Exists(Path))
                {
                    throw new FileNotFoundException(
                        $"could not find sound file to autogenerate name from: {Path}", Path);
                }

                string file = System.IO.Path.GetFileNameWithoutExtension(Path);
                string category = CategoryPaths[ctx.Category];
                Name = NormalizeRpName(System.IO.Path.Combine("sounds", category, file));
            }

            private static string NormalizeRpName(string value)
            {
                string relative = value.Replace('\\', '/').Trim().TrimStart('/');
                string ext = System.IO.Path.GetExtension(relative);
                if (!string.IsNullOrEmpty(ext))
                    relative = relative[..^ext.Length];
                return relative;
            }

            /// <summary>
            /// Creates a sound entry from a disk audio file, optionally overriding the pack path.
            /// </summary>
            public static Sound Create(
                string path,
                string? name = null,
                float volume = 1,
                int? weight = null,
                bool is3D = true,
                bool? stream = null,
                float? pitch = null) =>
                new(path, name)
                {
                    Volume = volume,
                    Weight = weight,
                    Is3D = is3D,
                    Stream = stream,
                    Pitch = pitch,
                };

            /// <summary>
            /// Creates a reference-only sound entry (no disk file copy) with an explicit pack path.
            /// </summary>
            public static Sound Reference(
                string name,
                float volume = 1,
                int? weight = null,
                bool is3D = true,
                bool? stream = null,
                float? pitch = null) =>
                Create(string.Empty, name, volume, weight, is3D, stream, pitch);

            /// <summary>
            /// Path to an audio file on disk. When set, the file is copied into the resource pack
            /// at <see cref="Name"/> (plus the source extension).
            /// </summary>
            [JsonIgnore] public string Path = path;

            /// <summary>
            /// Location in the resource pack without extension. Serialized as <c>name</c>.
            /// Auto-resolved from <see cref="Path"/> and the definition category when null.
            /// </summary>
            [JsonProperty("name")] public string? Name = name;

            /// <summary>
            /// Whether this sound will be spatialized
            /// </summary>
            [JsonProperty("is3D")] public bool Is3D = true;

            /// <summary>
            /// How loud the sound is
            /// </summary>
            [JsonProperty("volume")] public float Volume = 1;

            /// <summary>
            /// In the case of having multiple sounds with varying levels of rarity in a sound definition,
            /// <see cref="Weight"/> determines the rarity. Lower numbers = lower chance of being picked
            /// </summary>
            [JsonProperty("weight")] public int? Weight;

            /// <summary>
            /// When <see langword="true"/>, the sound is streamed from disk.
            /// </summary>
            [JsonProperty("stream")] public bool? Stream;

            /// <summary>
            /// Playback pitch for this variant.
            /// </summary>
            [JsonProperty("pitch")] public float? Pitch;
        }
    }

    /// <summary>
    /// List of sound definitions to write to <c>sound_definitions.json</c>
    /// </summary>
    public List<SoundDefinition> Definitions = new();

    /// <summary>
    /// Sound definition ids registered on this handler.
    /// </summary>
    public IReadOnlyCollection<string> Ids => Definitions.Select(d => d.Identifier).ToArray();

    /// <summary>
    /// Adds a <see cref="SoundDefinition"/> to <see cref="Definitions"/>
    /// </summary>
    /// <param name="definition"><see cref="SoundDefinition"/> to add</param>
    public void Add(SoundDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.Identifier))
            throw new ArgumentException("sound id cannot be empty", nameof(definition));
        if (definition.Sounds is null || definition.Sounds.Length == 0)
            throw new ArgumentException("at least one sound entry is required", nameof(definition));

        definition.EnsureSoundNames();
        int existing = Definitions.FindIndex(d => d.Identifier == definition.Identifier);
        if (existing >= 0)
            Definitions[existing] = definition;
        else
            Definitions.Add(definition);
    }

    /// <summary>
    /// Registers a sound definition from <see cref="SoundDefinition.Sound"/> entries.
    /// </summary>
    public void Add(
        string soundId,
        SoundDefinition.Sound[] sounds,
        SoundDefinition.SoundDefinitionCategory category = SoundDefinition.SoundDefinitionCategory.Neutral,
        float? maxDistance = null,
        float? minDistance = null)
    {
        if (string.IsNullOrWhiteSpace(soundId))
            throw new ArgumentException("sound id cannot be empty", nameof(soundId));
        if (sounds is null || sounds.Length == 0)
            throw new ArgumentException("at least one sound entry is required", nameof(sounds));

        for (int i = 0; i < sounds.Length; i++)
        {
            SoundDefinition.Sound sound = sounds[i];
            bool hasPath = !string.IsNullOrWhiteSpace(sound.Path);
            bool hasName = !string.IsNullOrWhiteSpace(sound.Name);
            if (!hasPath && !hasName)
            {
                throw new ArgumentException(
                    $"sound entry at index {i} requires Path (disk file) or Name (pack path)",
                    nameof(sounds));
            }

            if (sound.Name is not null && string.IsNullOrWhiteSpace(sound.Name))
            {
                throw new ArgumentException(
                    $"sound entry at index {i} has an empty Name", nameof(sounds));
            }
        }

        SoundDefinition definition = new(soundId)
        {
            Sounds = sounds,
            MaxDistance = maxDistance is null ? null : (int)maxDistance.Value,
            MinDistance = minDistance is null ? null : (int)minDistance.Value,
            Category = category
        };

        Add(definition);
    }

    internal IEnumerable<ResourceCopy> EnumerateCopies()
    {
        foreach (SoundDefinition definition in Definitions)
        {
            foreach (SoundDefinition.Sound sound in definition.Sounds)
            {
                if (string.IsNullOrWhiteSpace(sound.Path) || string.IsNullOrWhiteSpace(sound.Name))
                    continue;

                string extension = System.IO.Path.GetExtension(sound.Path);
                if (string.IsNullOrEmpty(extension))
                    extension = ".ogg";

                string relative = sound.Name.Replace('\\', '/').Trim().TrimStart('/');
                string nameExt = System.IO.Path.GetExtension(relative);
                if (!string.IsNullOrEmpty(nameExt) &&
                    nameExt.Equals(extension, StringComparison.OrdinalIgnoreCase))
                {
                    relative = relative[..^nameExt.Length];
                }

                yield return new ResourceCopy(
                    sound.Path,
                    $"{relative}{extension}",
                    sound.Name,
                    "sound file");
            }
        }
    }
}