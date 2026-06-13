using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;
using Formatting = ingot.Core.Common.Formatting;

namespace ingot.Core.Content.Block;

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
        
        Object(ref writer, "", w =>
        {
            Property(ref w, "ambient_occlusion", ao);
            Property(ref w, "face_dimming", fd);
            Property(ref w, "isotropic", iso);
            Property(ref w, "render_method", rm);
            Property(ref w, "texture", tex);
            if (tm != "none") Property(ref w, "tint_method", tm);
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
        
        Object(ref writer, "minecraft:material_instances", w =>
        {
            if (a is not null)
            {
                w.WritePropertyName("*");
                a.Value.Compile(ref w);
            }
            if (u is not null)
            {
                w.WritePropertyName("up");
                u.Value.Compile(ref w);
            }
            if (d is not null)
            {
                w.WritePropertyName("down");
                d.Value.Compile(ref w);
            }
            if (e is not null)
            {
                w.WritePropertyName("east");
                e.Value.Compile(ref w);
            }
            if (west is not null)
            {
                w.WritePropertyName("west");
                west.Value.Compile(ref w);
            }
            if (n is not null)
            {
                w.WritePropertyName("north");
                n.Value.Compile(ref w);
            }
            if (s is not null)
            {
                w.WritePropertyName("south");
                s.Value.Compile(ref w);
            }
        });
    }
}