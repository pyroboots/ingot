using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

using Newtonsoft.Json;

namespace ingot.Core.Resource;

internal class GameEventSoundBindingsJsonConverter : JsonConverter<GameEventSoundBindings>
{
    public override void WriteJson(JsonWriter writer, GameEventSoundBindings? value, JsonSerializer serializer)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));

        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            if (value.BlockSounds.Count > 0)
            {
                json.Object("block_sounds", () =>
                {
                    foreach (GameEventSoundBindings.BlockGameEventSoundBindings bind in value.BlockSounds.Values)
                        serializer.Serialize(writer, bind);
                });
            }

            if (value.EntitySoundsDefaults is not null || value.EntitySounds.Count > 0)
            {
                json.Object("entity_sounds", () =>
                {
                    if (value.EntitySoundsDefaults is not null)
                    {
                        GameEventSoundBindings.EntityGameEventSoundBindings defaults = value.EntitySoundsDefaults.Value with
                        {
                            Identifier = "defaults",
                        };
                        serializer.Serialize(writer, defaults);
                    }

                    if (value.EntitySounds.Count > 0)
                    {
                        json.Object("entities", () =>
                        {
                            foreach (GameEventSoundBindings.EntityGameEventSoundBindings bind in value.EntitySounds.Values)
                                serializer.Serialize(writer, bind);
                        });
                    }
                });
            }

            if (value.IndividualEventSounds.Count > 0)
            {
                json.Object("individual_event_sounds", () =>
                {
                    json.Object("events", () =>
                    {
                        foreach (GameEventSoundBindings.SoundBinding bind in value.IndividualEventSounds.Values)
                            serializer.Serialize(writer, bind);
                    });
                });
            }
        });
    }

    public override GameEventSoundBindings? ReadJson(JsonReader reader, Type objectType, GameEventSoundBindings? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        throw new InvalidOperationException();
    }
}

/// <summary>
/// Represents <c>sounds.json</c> in the resource pack
/// </summary>
[JsonConverter(typeof(GameEventSoundBindingsJsonConverter))]
public class GameEventSoundBindings
{
    private class SoundBindingJsonConverter : JsonConverter<SoundBinding>
    {
        public override void WriteJson(JsonWriter writer, SoundBinding value, JsonSerializer serializer)
        {
            JsonHelper json = new(ref writer);
            json.Object(value.Identifier, () =>
            {
                json.Property("sound", value.Sound);
                json.Property("volume", value.Volume);
                json.Property("pitch", value.Pitch);
            });
        }

        public override SoundBinding ReadJson(JsonReader reader, Type objectType, SoundBinding existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Represents a bind for a sound to a game event
    /// </summary>
    [JsonConverter(typeof(SoundBindingJsonConverter))]
    public struct SoundBinding
    {
        /// <summary>
        /// Block or entity identifier associated with this sound binding
        /// </summary>
        public required string Identifier;

        /// <summary>
        /// The sound ID registered in <c>sounds_definitions.json</c>
        /// </summary>
        public required string Sound
        {
            get;
            set
            {
                if (CompilerState.CurrentPack.ResourcePack.SoundDefinitionIds.Contains(value) == false)
                    CompilerState.Warn($"sound id {value} not present in sound definitions - ignore this warning if using a vanilla sound id");
                field = value;
            }
        }

        /// <summary>
        /// How loud the sound is
        /// </summary>
        public float? Volume;

        /// <summary>
        /// Constant float pitch or a range to choose randomly between min (idx 0) and max (idx 1)
        /// </summary>
        public Either<float[], float>? Pitch
        {
            get;
            set
            {
                if (value is null)
                {
                    field = value;
                    return;
                }

                if (value.Type == typeof(float[]) && ((value.Value as float[])!).Length != 2)
                    throw new ArgumentException("range item must have only 2 floats (min and max)");
                field = value;
            }
        }
    }

    private static void WriteEvent(
        JsonHelper json,
        JsonSerializer serializer,
        string eventName,
        Either<SoundBinding, string>? binding)
    {
        if (binding is null)
            return;

        if (binding.Type == typeof(string))
        {
            json.Property(eventName, binding.Value);
            return;
        }

        SoundBinding sound = ((SoundBinding)binding.Value) with { Identifier = eventName };
        serializer.Serialize(json.Writer, sound);
    }

    private class BlockGameEventSoundBindingJsonConverter : JsonConverter<BlockGameEventSoundBindings>
    {
        public override void WriteJson(JsonWriter writer, BlockGameEventSoundBindings value, JsonSerializer serializer)
        {
            JsonHelper json = new(ref writer);
            json.Object(value.Identifier, () =>
            {
                json.Object("events", () =>
                {
                    json.Property("default", "");

                    WriteEvent(json, serializer, "break", value.Break);
                    WriteEvent(json, serializer, "hit", value.Hit);
                    WriteEvent(json, serializer, "item.use.on", value.ItemUseOn);
                    WriteEvent(json, serializer, "place", value.Place);
                    WriteEvent(json, serializer, "power.off", value.PowerOff);
                    WriteEvent(json, serializer, "power.on", value.PowerOn);

                    if (value.ExtraSoundBindings is not null)
                    {
                        foreach ((string eventName, Either<SoundBinding, string> binding) in value.ExtraSoundBindings)
                            WriteEvent(json, serializer, eventName, binding);
                    }
                });

                json.Property("volume", value.Volume);
                json.Property("pitch", value.Pitch);
            });
        }

        public override BlockGameEventSoundBindings ReadJson(JsonReader reader, Type objectType, BlockGameEventSoundBindings existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Contains hit, step, and break sounds for blocks
    /// </summary>
    [JsonConverter(typeof(BlockGameEventSoundBindingJsonConverter))]
    public struct BlockGameEventSoundBindings
    {
        /// <summary>
        /// Block identifier associated with these bindings
        /// </summary>
        public required Identifier Identifier;

        private static Either<SoundBinding, string>? OverrideSoundBindingId(Either<SoundBinding, string>? binding, string id)
        {
            if (binding is null) return null;
            if (binding.Type == typeof(SoundBinding))
                return ((SoundBinding)binding.Value) with { Identifier = id };
            return binding;
        }

        /// <summary>
        /// Sound played when the block is broken
        /// </summary>
        public Either<SoundBinding, string>? Break
        {
            get => OverrideSoundBindingId(field, "break");
            set;
        }

        /// <summary>
        /// Sound played when hit or while being broken
        /// </summary>
        public Either<SoundBinding, string>? Hit
        {
            get => OverrideSoundBindingId(field, "hit");
            set;
        }

        /// <summary>
        /// Sound played when an item is used on this block
        /// </summary>
        public Either<SoundBinding, string>? ItemUseOn
        {
            get => OverrideSoundBindingId(field, "item.use.on");
            set;
        }

        /// <summary>
        /// Sound played when this block is placed
        /// </summary>
        public Either<SoundBinding, string>? Place
        {
            get => OverrideSoundBindingId(field, "place");
            set;
        }

        /// <summary>
        /// Unknown. Defaults to the lever clicking sound
        /// </summary>
        public Either<SoundBinding, string>? PowerOff
        {
            get => OverrideSoundBindingId(field, "power.off");
            set;
        }
        /// <summary>
        /// Unknown. Defaults to the lever clicking sound
        /// </summary>
        public Either<SoundBinding, string>? PowerOn
        {
            get => OverrideSoundBindingId(field, "power.on");
            set;
        }

        /// <summary>
        /// Extra sound bindings for game events outside of the shortcutted common ones
        /// </summary>
        public Dictionary<string, Either<SoundBinding, string>>? ExtraSoundBindings;

        /// <summary>
        /// How loud the sound is
        /// </summary>
        public float? Volume;

        /// <summary>
        /// Pitch of the sound
        /// </summary>
        public float? Pitch;
    }

    private class EntityGameEventSoundBindingJsonConverter : JsonConverter<EntityGameEventSoundBindings>
    {
        public override void WriteJson(JsonWriter writer, EntityGameEventSoundBindings value, JsonSerializer serializer)
        {
            JsonHelper json = new(ref writer);
            string key = value.Identifier.Namespace == "minecraft" && value.Identifier.Name == "defaults"
                ? "defaults"
                : value.Identifier;
            json.Object(key, () =>
            {
                json.Object("events", () =>
                {
                    WriteEvent(json, serializer, "ambient", value.Ambient);
                    WriteEvent(json, serializer, "attack", value.Attack);
                    WriteEvent(json, serializer, "death", value.Death);
                    WriteEvent(json, serializer, "fall.big", value.FallBig);
                    WriteEvent(json, serializer, "fall.small", value.FallSmall);
                    WriteEvent(json, serializer, "hurt", value.Hurt);
                    WriteEvent(json, serializer, "step", value.Step);
                    WriteEvent(json, serializer, "splash", value.Splash);
                    WriteEvent(json, serializer, "shoot", value.Shoot);

                    if (value.ExtraSoundBindings is not null)
                    {
                        foreach ((string eventName, Either<SoundBinding, string> binding) in value.ExtraSoundBindings)
                            WriteEvent(json, serializer, eventName, binding);
                    }
                });

                json.Property("volume", value.Volume);
                json.Property("pitch", value.Pitch);
            });
        }

        public override EntityGameEventSoundBindings ReadJson(JsonReader reader, Type objectType, EntityGameEventSoundBindings existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Contains death, ambient, hurt, etc. sounds for entities
    /// </summary>
    [JsonConverter(typeof(EntityGameEventSoundBindingJsonConverter))]
    public struct EntityGameEventSoundBindings
    {
        /// <summary>
        /// Entity identifier associated with these bindings
        /// </summary>
        public required Identifier Identifier;

        private static Either<SoundBinding, string>? OverrideSoundBindingId(Either<SoundBinding, string>? binding, string id)
        {
            if (binding is null) return null;
            if (binding.Type == typeof(SoundBinding))
                return ((SoundBinding)binding.Value) with { Identifier = id };
            return binding;
        }

        /// <summary>
        /// Played randomly, such as grunts, clucks, or ghast noises
        /// </summary>
        public Either<SoundBinding, string>? Ambient
        {
            get => OverrideSoundBindingId(field, "ambient");
            set;
        }

        /// <summary>
        /// For melee attacking
        /// </summary>
        public Either<SoundBinding, string>? Attack
        {
            get => OverrideSoundBindingId(field, "attack");
            set;
        }

        /// <summary>
        /// Played when the entity dies
        /// </summary>
        public Either<SoundBinding, string>? Death
        {
            get => OverrideSoundBindingId(field, "death");
            set;
        }

        /// <summary>
        /// For hitting the ground from a high height
        /// </summary>
        public Either<SoundBinding, string>? FallBig
        {
            get => OverrideSoundBindingId(field, "fall.big");
            set;
        }

        /// <summary>
        /// For hitting the ground from a low height
        /// </summary>
        public Either<SoundBinding, string>? FallSmall
        {
            get => OverrideSoundBindingId(field, "fall.small");
            set;
        }

        /// <summary>
        /// Played when damaged
        /// </summary>
        public Either<SoundBinding, string>? Hurt
        {
            get => OverrideSoundBindingId(field, "hurt");
            set;
        }

        /// <summary>
        /// For shooting projectiles
        /// </summary>
        public Either<SoundBinding, string>? Shoot
        {
            get => OverrideSoundBindingId(field, "shoot");
            set;
        }

        /// <summary>
        /// For splashing in the water
        /// </summary>
        public Either<SoundBinding, string>? Splash
        {
            get => OverrideSoundBindingId(field, "splash");
            set;
        }

        /// <summary>
        /// Played when the entity moves along the ground
        /// </summary>
        public Either<SoundBinding, string>? Step
        {
            get => OverrideSoundBindingId(field, "step");
            set;
        }

        /// <summary>
        /// Extra sound bindings for game events outside of the shortcutted common ones
        /// </summary>
        public Dictionary<string, Either<SoundBinding, string>>? ExtraSoundBindings;

        /// <summary>
        /// How loud the sound is
        /// </summary>
        public float? Volume;

        /// <summary>
        /// Pitch of the sound. A single float, or a two-element <c>[min, max]</c> range.
        /// </summary>
        public Either<float[], float>? Pitch;
    }

    /// <summary>
    /// List of bindings for block game events
    /// </summary>
    public Dictionary<Identifier, BlockGameEventSoundBindings> BlockSounds = new();
    /// <summary>
    /// Default sound bindings for entity game events when not overriden
    /// </summary>
    public EntityGameEventSoundBindings? EntitySoundsDefaults = null;
    /// <summary>
    /// List of bindings for entity game events
    /// </summary>
    public Dictionary<Identifier, EntityGameEventSoundBindings> EntitySounds = new();
    /// <summary>
    /// List of bindings for miscellaneous game events like beacon activation, chest-close, or explode
    /// </summary>
    public Dictionary<Identifier, SoundBinding> IndividualEventSounds = new();

    /// <summary>
    /// Adds sound bindings for the block with identifier <paramref name="id"/>
    /// </summary>
    /// <param name="id">The block identifier</param>
    /// <param name="bindings">Bindings to add</param>
    public void BindBlockSounds(Identifier id, BlockGameEventSoundBindings bindings)
    {
        bindings.Identifier = id;
        BlockSounds[id] = bindings;
    }

    /// <summary>
    /// Adds sound bindings for the entity with identifier  <paramref name="id"/>
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <param name="bindings">Bindings to add</param>
    public void BindEntitySounds(Identifier id, EntityGameEventSoundBindings bindings)
    {
        bindings.Identifier = id;
        EntitySounds[id] = bindings;
    }

    internal void BindClientEntitySounds(ClientEntity entity)
    {
        ClientEntitySounds? sounds = entity.EntitySounds;
        if (sounds is null || sounds.Events.Count == 0)
            return;
        if (EntitySounds.ContainsKey(entity.Identifier))
            return;

        EntityGameEventSoundBindings bindings = new()
        {
            Identifier = entity.Identifier,
            Volume = sounds.Volume,
            Pitch = sounds.Pitch is { Length: 1 } ? sounds.Pitch[0] : sounds.Pitch,
        };

        Dictionary<string, Either<SoundBinding, string>> extras = new();
        foreach ((string eventName, Either<string, Dictionary<string, object>> value) in sounds.Events)
        {
            Either<SoundBinding, string> binding = ConvertClientEvent(eventName, value);
            switch (eventName)
            {
                case "ambient": bindings.Ambient = binding; break;
                case "attack": bindings.Attack = binding; break;
                case "death": bindings.Death = binding; break;
                case "fall.big": bindings.FallBig = binding; break;
                case "fall.small": bindings.FallSmall = binding; break;
                case "hurt": bindings.Hurt = binding; break;
                case "shoot": bindings.Shoot = binding; break;
                case "splash": bindings.Splash = binding; break;
                case "step": bindings.Step = binding; break;
                default: extras[eventName] = binding; break;
            }
        }

        if (extras.Count > 0)
            bindings.ExtraSoundBindings = extras;

        EntitySounds[entity.Identifier] = bindings;
    }

    private static Either<SoundBinding, string> ConvertClientEvent(
        string eventName,
        Either<string, Dictionary<string, object>> value)
    {
        if (value.Type == typeof(string))
            return (string)value.Value;

        Dictionary<string, object> dict = (Dictionary<string, object>)value.Value;
        return new SoundBinding
        {
            Identifier = eventName,
            Sound = dict.TryGetValue("sound", out object? sound) ? sound.ToString() ?? eventName : eventName,
            Volume = dict.TryGetValue("volume", out object? volume) ? Convert.ToSingle(volume) : null,
            Pitch = dict.TryGetValue("pitch", out object? pitch) ? ConvertPitch(pitch) : null,
        };
    }

    private static Either<float[], float>? ConvertPitch(object pitch) =>
        pitch switch
        {
            float f => f,
            double d => (float)d,
            float[] range => range,
            _ => null,
        };
}
