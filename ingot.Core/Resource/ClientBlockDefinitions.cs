using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

using Newtonsoft.Json;

namespace ingot.Core.Resource;

internal class ClientBlockDefinitionsJsonConverter : JsonConverter<ClientBlockDefinitions>
{
    public override void WriteJson(JsonWriter writer, ClientBlockDefinitions? value, JsonSerializer serializer)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));

        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Property("format_version", "1.21.40");
            foreach (ClientBlockDefinitions.ClientBlockDefinition def in value.Blocks)
                serializer.Serialize(writer, def);
        });
    }

    public override ClientBlockDefinitions? ReadJson(JsonReader reader, Type objectType, ClientBlockDefinitions? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        throw new InvalidOperationException();
    }
}

/// <summary>
/// Represents <c>blocks.json</c> in the resource pack
/// </summary>
[JsonConverter(typeof(ClientBlockDefinitionsJsonConverter))]
public class ClientBlockDefinitions
{
    private class ClientBlockDefinitionJsonConverter : JsonConverter<ClientBlockDefinition>
    {
        private static void EnsureValid(IEnumerable<string> list)
        {
            string[] valid = ["up", "down", "north", "east", "south", "west"];
            if (list.Any(i => valid.Contains(i) == false))
                throw new ArgumentException("keys must only be " + string.Join(" or ", valid));
        }

        public override void WriteJson(JsonWriter writer, ClientBlockDefinition value, JsonSerializer serializer)
        {
            JsonHelper json = new(ref writer);
            json.Object(value.Identifier, () =>
            {
                json.Property("sound", value.Sound);

                if (value.Textures is not null)
                {
                    if (value.Textures.Type == typeof(Dictionary<string, string>))
                        EnsureValid(((value.Textures.Value as Dictionary<string, string>)!).Keys);
                    json.Property("textures", value.Textures.Value);
                }
                if (value.CarriedTextures is not null)
                {
                    if (value.CarriedTextures.Type == typeof(Dictionary<string, string>))
                        EnsureValid(((value.CarriedTextures.Value as Dictionary<string, string>)!).Keys);
                    json.Property("carried_textures", value.CarriedTextures.Value);
                }
                if (value.Isotropic is not null)
                {
                    if (value.Isotropic.Type == typeof(Dictionary<string, bool>))
                        EnsureValid(((value.Isotropic.Value as Dictionary<string, bool>)!).Keys);
                    json.Property("isotropic", value.Isotropic.Value);
                }

                json.Property("ambient_occlusion_exponent", value.AmbientOcclusionExponent);
            });
        }

        public override ClientBlockDefinition ReadJson(JsonReader reader, Type objectType, ClientBlockDefinition existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Represents a block client definition in <c>blocks.json</c>
    /// </summary>
    [JsonConverter(typeof(ClientBlockDefinitionJsonConverter))]
    public struct ClientBlockDefinition
    {
        /// <summary>
        /// Identifier of the block this definition is associated with
        /// </summary>
        public required Identifier Identifier;
        /// <summary>
        /// Used to determine general block sounds, such as the mining sound, step on sound, breaking sound, and placement sound
        /// </summary>
        public string? Sound;
        /// <summary>
        /// Defines the textures to use for each face or for all
        /// </summary>
        [Obsolete("use block material instances instead")]
        public Either<Dictionary<string, string>, string>? Textures;
        /// <summary>
        /// Defines the texture when holding as an item
        /// </summary>
        public Either<Dictionary<string, string>, string>? CarriedTextures;
        /// <summary>
        /// Determines whether individual or all sides are isotropic or not
        /// </summary>
        public Either<Dictionary<string, bool>, bool>? Isotropic;
        /// <summary>
        /// How much AO this block should have
        /// </summary>
        public float? AmbientOcclusionExponent;
    }

    /// <summary>
    /// List of <see cref="ClientBlockDefinition"/> to be written to <c>blocks.json</c> in the resource pack
    /// </summary>
    public readonly List<ClientBlockDefinition> Blocks = new();

    /// <summary>
    /// Adds a client block definition to <see cref="Blocks"/>
    /// </summary>
    /// <param name="definition">Client block definition to add</param>
    public void Add(ClientBlockDefinition definition) => Blocks.Add(definition);

    internal void SeedFromPack(Pack pack)
    {
        foreach (Block block in pack.BehaviourPack.Blocks)
        {
            if (block.ResourceTexture is null && block.Sound is null)
                continue;
            if (Blocks.Any(existing => existing.Identifier == block.Identifier))
                continue;

#pragma warning disable CS0618 // ResourceTexture is the blocks.json texture shortcut
            ClientBlockDefinition definition = new()
            {
                Identifier = block.Identifier,
                Sound = block.Sound,
            };
            if (block.ResourceTexture is not null)
                definition.Textures = block.ResourceTexture;
            Add(definition);
#pragma warning restore CS0618
        }
    }
}
