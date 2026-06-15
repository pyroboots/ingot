using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;
using Formatting = ingot.Core.Common.Formatting;

namespace ingot.Core.Behaviour.Block;

/// <summary>
/// Texture and material configuration for the face of a <see cref="Block"/>
/// </summary>
public struct MaterialInstance : ICompileableFragment
{
    public MaterialInstance(string texture) => Texture = texture;
    public MaterialInstance(string texture, RenderMethods method)
    {
        Texture = texture;
        RenderMethod = method;
    }
    
    public enum RenderMethods
    {
        Opaque,
        DoubleSided,
        Blend,
        AlphaTest,
        AlphaTestSingleSided,
        BlendToOpaque,
        AlphaTestToOpaque,
        AlphaTestSingleSidedToOpaque,
    }

    public enum TintMethods
    {
        None,
        DefaultFoliage,
        BirchFoliage,
        EvergreenFoliage,
        DryFoliage,
        Grass,
        Water
    }
    
    public float? AmbientOcclusion = null;
    public bool? FaceDimming = null;
    public bool? Isotropic = null;
    public RenderMethods RenderMethod = RenderMethods.AlphaTest;
    public string Texture;
    public TintMethods TintMethod = TintMethods.None;
    
    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
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
public struct MaterialInstances : ICompileableFragment
{
    public MaterialInstances() { }
    public MaterialInstances(MaterialInstance all) => All = all;
    
    public MaterialInstance? All = null;
    public MaterialInstance? Up = null;
    public MaterialInstance? Down = null;
    public MaterialInstance? East = null;
    public MaterialInstance? West = null;
    public MaterialInstance? North = null;
    public MaterialInstance? South = null;
    
    /// <summary>
    /// Compiles <see cref="MaterialInstances"/> to JSON
    /// </summary>
    /// <param name="writer">JSON source stream to write to</param>
    public void Compile(ref JsonTextWriter writer)
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