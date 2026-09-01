using ingot.Core.Common;

using Newtonsoft.Json;

namespace ingot.Core.Resource;

internal class ClientBiomeDefinitionsJsonConverter : JsonConverter<ClientBiomeDefinitions>
{
    public override void WriteJson(JsonWriter writer, ClientBiomeDefinitions? value, JsonSerializer serializer)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));

        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Object("biomes", () =>
            {
                foreach (ClientBiomeDefinitions.ClientBiomeDefinition def in value.Biomes)
                    serializer.Serialize(writer, def);
            });
        });
    }

    public override ClientBiomeDefinitions? ReadJson(JsonReader reader, Type objectType, ClientBiomeDefinitions? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new InvalidOperationException();
    }
}

/// <summary>
/// Represents <c>biomes_client.json</c> in the resource pack
/// </summary>
[JsonConverter(typeof(ClientBiomeDefinitionsJsonConverter))]
public class ClientBiomeDefinitions
{
    private class ClientBiomeDefinitionJsonConverter : JsonConverter<ClientBiomeDefinition>
    {
        public override void WriteJson(JsonWriter writer, ClientBiomeDefinition value, JsonSerializer serializer)
        {
            JsonHelper json = new(ref writer);
            json.Object(value.Identifier.Value.ToString()!, () =>
            {
                if (value.WaterSurfaceColor is not null)
                {
                    if (value.WaterSurfaceColor.StartsWith("#") == false)
                        throw new ArgumentException("water surface color must begin with # to denote hex color");
                    if (value.WaterSurfaceColor.Length != 7)
                        throw new ArgumentException("water surface color must be 7 characters (1 hashtag followed by the 6 char hex code)");
                    json.Property("water_surface_color", value.WaterSurfaceColor);
                }

                if (value.WaterSurfaceTransparency is not null)
                {
                    if (value.WaterSurfaceTransparency.Value > 1 || value.WaterSurfaceTransparency.Value < 0)
                        throw new ArgumentOutOfRangeException(nameof(value.WaterSurfaceTransparency));
                    json.Property("water_surface_transparency", value.WaterSurfaceTransparency);
                }

                if (value.FogIdentifier is not null)
                {
                    if (value.FogIdentifier.ToString().Contains("fog_") == false)
                        CompilerState.Warn("fog identifiers should follow naming convention namespace:fog_biome");
                    json.Property("fog_identifier", value.FogIdentifier);
                }
                json.Property("remove_all_prior_fog", value.RemoveAllPriorFog);
                json.Property("inherit_from_prior_fog", value.InheritFromPriorFog);
            });
        }

        public override ClientBiomeDefinition ReadJson(JsonReader reader, Type objectType, ClientBiomeDefinition existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Represents a client definition for a biome in <c>biomes_client.json</c>
    /// </summary>
    [JsonConverter(typeof(ClientBiomeDefinitionJsonConverter))]
    public struct ClientBiomeDefinition
    {
        /// <summary>
        /// Biome identifier this definition is associated with
        /// </summary>
        public required Either<string, Identifier> Identifier;

        /// <summary>
        /// Specifies the surface color hex code for water bodies within the biome (e.g., <c>#44AFF5</c>).
        /// </summary>
        public string? WaterSurfaceColor;
        /// <summary>
        /// Controls the opacity/transparency layer of the water surface ranging from 0.0 (fully clear) to 1.0 (fully opaque).
        /// </summary>
        public float? WaterSurfaceTransparency;
        /// <summary>
        /// Namespaced ID mapping to the specific fog definition JSON to render in the biome (e.g., <c>minecraft:fog_plains</c>).
        /// </summary>
        public Identifier? FogIdentifier;
        /// <summary>
        /// Determines whether previous active fog layers are cleared upon entering the biome.
        /// </summary>
        public bool? RemoveAllPriorFog;
        /// <summary>
        /// Determines whether the biome retains and merges fog settings from the previously entered biome.
        /// </summary>
        public bool? InheritFromPriorFog;
    }

    /// <summary>
    /// List of <see cref="ClientBiomeDefinition"/> to be written to <c>biomes_client.json</c> in the resource pack
    /// </summary>
    public readonly List<ClientBiomeDefinition> Biomes = new();

    /// <summary>
    /// Adds a biome definition to <see cref="Biomes"/>
    /// </summary>
    /// <param name="definition">Client biome definition to add</param>
    public void Add(ClientBiomeDefinition definition) => Biomes.Add(definition);
}
