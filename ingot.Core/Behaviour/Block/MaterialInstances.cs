using ingot.Core.Common;

using Newtonsoft.Json;

using static ingot.Core.Common.JsonHelper;

using Formatting = ingot.Core.Common.Formatting;

namespace ingot.Core.Behaviour.Block;

/// <summary>
/// Texture and material configuration for the face of a <see cref="Block"/>
/// </summary>
public struct MaterialInstance : ICompilableFragment
{
    /// <summary>
    /// Creates a material instance with the given texture key.
    /// </summary>
    /// <param name="texture">Texture key referenced in the resource pack.</param>
    public MaterialInstance(string texture) => Texture = texture;

    /// <summary>
    /// Creates a material instance with the given texture key and optional source PNG path.
    /// </summary>
    /// <param name="texture">Texture key referenced in the resource pack.</param>
    /// <param name="sourcePath">Optional path to the source PNG for auto-registration during compile.</param>
    public MaterialInstance(string texture, string? sourcePath)
    {
        Texture = texture;
        SourcePath = sourcePath;
    }

    /// <summary>
    /// Creates a material instance with texture key, render method, and optional source PNG path.
    /// </summary>
    /// <param name="texture">Texture key referenced in the resource pack.</param>
    /// <param name="method">How the texture is rendered on this face.</param>
    /// <param name="sourcePath">Optional path to the source PNG for auto-registration during compile.</param>
    public MaterialInstance(string texture, RenderMethods method, string? sourcePath = null)
    {
        Texture = texture;
        RenderMethod = method;
        SourcePath = sourcePath;
    }

    /// <summary>
    /// How a <see cref="MaterialInstance"/> texture is blended or alpha-tested at runtime.
    /// </summary>
    public enum RenderMethods
    {
        /// <summary>Fully solid, no transparency.</summary>
        Opaque,
        /// <summary>Rendered on both sides of the face.</summary>
        DoubleSided,
        /// <summary>Full alpha blending.</summary>
        Blend,
        /// <summary>Cutout transparency (classic alpha test).</summary>
        AlphaTest,
        /// <summary>Alpha test on a single-sided face.</summary>
        AlphaTestSingleSided,
        /// <summary>Blended rendering that transitions to opaque.</summary>
        BlendToOpaque,
        /// <summary>Alpha-tested rendering that transitions to opaque.</summary>
        AlphaTestToOpaque,
        /// <summary>Single-sided alpha test that transitions to opaque.</summary>
        AlphaTestSingleSidedToOpaque,
    }

    /// <summary>
    /// Biome or foliage tinting applied to a <see cref="MaterialInstance"/> texture.
    /// </summary>
    public enum TintMethods
    {
        /// <summary>No tinting applied.</summary>
        None,
        /// <summary>Default foliage tint for the biome.</summary>
        DefaultFoliage,
        /// <summary>Birch foliage tint.</summary>
        BirchFoliage,
        /// <summary>Evergreen foliage tint.</summary>
        EvergreenFoliage,
        /// <summary>Dry foliage tint.</summary>
        DryFoliage,
        /// <summary>Grass tint for the biome.</summary>
        Grass,
        /// <summary>Water tint for the biome.</summary>
        Water
    }

    /// <summary>
    /// Strength of ambient occlusion on this face.
    /// </summary>
    public float? AmbientOcclusion = null;
    /// <summary>
    /// Whether the face is dimmed when not facing a light source.
    /// </summary>
    public bool? FaceDimming = null;
    /// <summary>
    /// Whether the texture is rotated randomly per block (useful for grass, etc.).
    /// </summary>
    public bool? Isotropic = null;
    /// <summary>
    /// How the texture is rendered on this face.
    /// </summary>
    public RenderMethods RenderMethod = RenderMethods.AlphaTest;
    /// <summary>
    /// Texture key referenced in the resource pack (not a file path).
    /// </summary>
    public string Texture;
    /// <summary>
    /// Optional path to the source PNG for this texture. When set, ingot auto-registers the texture
    /// in the resource pack during compilation unless it was already added manually.
    /// </summary>
    public string? SourcePath = null;
    /// <summary>
    /// Color tinting method applied to this face.
    /// </summary>
    public TintMethods TintMethod = TintMethods.None;

    /// <inheritdoc/>
    public void Compile(ref JsonWriter writer)
    {
        // lambda cannot access struct members
        var ao = AmbientOcclusion;
        var fd = FaceDimming;
        var iso = Isotropic;
        var rm = Formatting.PascalToSnakeCase(Enum.GetName(RenderMethod)!);
        var tex = Texture;
        var tm = Formatting.PascalToSnakeCase(Enum.GetName(TintMethod)!);

        JsonHelper json = new(ref writer);

        json.Object("", () =>
        {
            json.Property("ambient_occlusion", ao);
            json.Property("face_dimming", fd);
            json.Property("isotropic", iso);
            json.Property("render_method", rm);
            json.Property("texture", tex);
            if (tm != "none") json.Property("tint_method", tm);
        });
    }
}

/// <summary>
/// Texture and material configuration for the faces of a <see cref="Block"/>
/// </summary>
public struct MaterialInstances : ICompilableFragment
{
    /// <summary>
    /// Creates an empty <see cref="MaterialInstances"/> with no faces configured.
    /// </summary>
    public MaterialInstances() { }

    /// <summary>
    /// Creates <see cref="MaterialInstances"/> where every face uses the same <paramref name="all"/> configuration.
    /// </summary>
    /// <param name="all">Material applied to all faces via the <c>*</c> wildcard.</param>
    public MaterialInstances(MaterialInstance all) => All = all;

    /// <summary>
    /// Material applied to all faces via the <c>*</c> wildcard.
    /// </summary>
    public MaterialInstance? All = null;
    /// <summary>
    /// Material applied to the top face.
    /// </summary>
    public MaterialInstance? Up = null;
    /// <summary>
    /// Material applied to the bottom face.
    /// </summary>
    public MaterialInstance? Down = null;
    /// <summary>
    /// Material applied to the east face.
    /// </summary>
    public MaterialInstance? East = null;
    /// <summary>
    /// Material applied to the west face.
    /// </summary>
    public MaterialInstance? West = null;
    /// <summary>
    /// Material applied to the north face.
    /// </summary>
    public MaterialInstance? North = null;
    /// <summary>
    /// Material applied to the south face.
    /// </summary>
    public MaterialInstance? South = null;

    /// <summary>
    /// Returns each distinct texture key used by a face, along with its optional source PNG path.
    /// </summary>
    public IEnumerable<(string Key, string? SourcePath)> EnumerateTextures()
    {
        HashSet<string> seen = new();
        foreach (MaterialInstance? face in new[] { All, Up, Down, East, West, North, South })
        {
            if (face is null)
                continue;

            MaterialInstance instance = face.Value;
            if (string.IsNullOrWhiteSpace(instance.Texture) || !seen.Add(instance.Texture))
                continue;

            yield return (instance.Texture, instance.SourcePath);
        }
    }

    /// <summary>
    /// Compiles <see cref="MaterialInstances"/> to JSON
    /// </summary>
    /// <param name="writer">JSON source stream to write to</param>
    public void Compile(ref JsonWriter writer)
    {
        // lambda cannot access struct members
        var a = All;
        var u = Up;
        var d = Down;
        var e = East;
        var west = West;
        var n = North;
        var s = South;

        JsonHelper json = new(ref writer);

        json.Object("minecraft:material_instances", () =>
        {
            if (a is not null)
            {
                json.Writer.WritePropertyName("*");
                a.Value.Compile(ref json.Writer);
            }
            if (u is not null)
            {
                json.Writer.WritePropertyName("up");
                u.Value.Compile(ref json.Writer);
            }
            if (d is not null)
            {
                json.Writer.WritePropertyName("down");
                d.Value.Compile(ref json.Writer);
            }
            if (e is not null)
            {
                json.Writer.WritePropertyName("east");
                e.Value.Compile(ref json.Writer);
            }
            if (west is not null)
            {
                json.Writer.WritePropertyName("west");
                west.Value.Compile(ref json.Writer);
            }
            if (n is not null)
            {
                json.Writer.WritePropertyName("north");
                n.Value.Compile(ref json.Writer);
            }
            if (s is not null)
            {
                json.Writer.WritePropertyName("south");
                s.Value.Compile(ref json.Writer);
            }
        });
    }
}