using ingot.Core.Common;

using Newtonsoft.Json;

using Formatting = Newtonsoft.Json.Formatting;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Behaviour.Entity;

/// <summary>
/// Resource-pack render controller. Maps client-entity short-names
/// (<c>Geometry.*</c>, <c>Material.*</c>, <c>Texture.*</c>) into what is drawn in-game.
/// </summary>
public abstract class RenderController : IConcreteCompilable<RenderController>
{
    /// <summary>
    /// Full render controller identifier, e.g. <c>controller.render.my_entity</c>.
    /// </summary>
    public abstract string ControllerId { get; }

    /// <summary>
    /// Filename (without extension) written under <c>rp/render_controllers/</c>.
    /// Defaults to the portion after <c>controller.render.</c>, or the full id.
    /// </summary>
    public virtual string FileName
    {
        get
        {
            const string prefix = "controller.render.";
            string id = ControllerId;
            if (id.StartsWith(prefix, StringComparison.Ordinal))
                return id[prefix.Length..];
            return id.Replace(':', '_');
        }
    }

    /// <summary>
    /// Format version of the render controller file.
    /// </summary>
    public virtual Version FormatVersion => new("1.10.0");

    /// <summary>
    /// Geometry reference, e.g. <c>Geometry.default</c> or <c>Array.geo[q.variant]</c>.
    /// </summary>
    public virtual string Geometry => "Geometry.default";

    /// <summary>
    /// Material map entries. Each dictionary is one materials array element mapping
    /// bone patterns (e.g. <c>*</c>) to material references (e.g. <c>Material.default</c>).
    /// </summary>
    public virtual IReadOnlyList<IReadOnlyDictionary<string, string>> Materials { get; } =
        [new Dictionary<string, string> { ["*"] = "Material.default" }];

    /// <summary>
    /// Texture layers, bottom-to-top. Entries are short-name references such as
    /// <c>Texture.default</c> or array indexes like <c>Array.top[q.variant]</c>.
    /// </summary>
    public virtual string[] Textures { get; } = ["Texture.default"];

    /// <summary>
    /// Optional <c>arrays.textures</c> definitions: array name → list of texture references.
    /// Keys should typically start with <c>Array.</c> (e.g. <c>Array.top</c>).
    /// </summary>
    public virtual Dictionary<string, string[]>? TextureArrays => null;

    /// <summary>
    /// Optional <c>arrays.geometries</c> definitions: array name → list of geometry references.
    /// </summary>
    public virtual Dictionary<string, string[]>? GeometryArrays => null;

    /// <summary>
    /// Optional <c>arrays.materials</c> definitions: array name → list of material references.
    /// </summary>
    public virtual Dictionary<string, string[]>? MaterialArrays => null;

    /// <summary>
    /// Optional part visibility map (bone pattern → Molang expression).
    /// </summary>
    public virtual Dictionary<string, string>? PartVisibility => null;

    /// <summary>
    /// Optional overlay colour Molang expressions (RGBA components).
    /// </summary>
    public virtual string[]? Color => null;

    /// <summary>
    /// Whether the hurt colour overlay is applied.
    /// </summary>
    public virtual bool? IsHurtColor => null;

    /// <summary>
    /// Whether lighting is ignored when rendering.
    /// </summary>
    public virtual bool? IgnoreLighting => null;

    /// <summary>
    /// Whether the entity should rebuild geometry when needing to re-render.
    /// </summary>
    public virtual bool? RebuildAnimationMatrices => null;

    /// <summary>
    /// Creates a simple render controller that always uses the <c>default</c> short-names.
    /// </summary>
    /// <param name="controllerId">Full controller id, e.g. <c>controller.render.my_entity</c>.</param>
    public static RenderController CreateSimple(string controllerId) =>
        new SimpleRenderController(controllerId);

    /// <inheritdoc/>
    public static string Compile(Type tType)
    {
        RenderController inst = (Activator.CreateInstance(tType) as RenderController)!;
        return CompileFromInstance(inst);
    }

    /// <inheritdoc/>
    public static string Compile<TConcreteType>() where TConcreteType : RenderController, new() =>
        Compile(typeof(TConcreteType));

    /// <inheritdoc/>
    public static string CompileFromInstance(RenderController inst)
    {
        CompilerState.Push(inst.ControllerId);

        StringWriter sw = new();
        JsonWriter w = new JsonTextWriter(sw)
        {
            Formatting = Formatting.Indented,
            Indentation = 4,
        };

        JsonHelper json = new(ref w);

        w.WriteStartObject();
        json.Property("format_version", inst.FormatVersion.ToString());
        json.Object("render_controllers", () =>
        {
            json.Object(inst.ControllerId, () =>
            {
                bool hasTextureArrays = inst.TextureArrays is { Count: > 0 };
                bool hasGeometryArrays = inst.GeometryArrays is { Count: > 0 };
                bool hasMaterialArrays = inst.MaterialArrays is { Count: > 0 };

                if (hasTextureArrays || hasGeometryArrays || hasMaterialArrays)
                {
                    json.Object("arrays", () =>
                    {
                        if (hasMaterialArrays)
                            WriteArrayGroup(json, w, "materials", inst.MaterialArrays!);
                        if (hasGeometryArrays)
                            WriteArrayGroup(json, w, "geometries", inst.GeometryArrays!);
                        if (hasTextureArrays)
                            WriteArrayGroup(json, w, "textures", inst.TextureArrays!);
                    });
                }

                json.Property("geometry", inst.Geometry);

                if (inst.Materials is { Count: > 0 })
                {
                    json.Array("materials", () =>
                    {
                        foreach (IReadOnlyDictionary<string, string> entry in inst.Materials)
                        {
                            w.WriteStartObject();
                            foreach (var kvp in entry)
                            {
                                w.WritePropertyName(kvp.Key);
                                w.WriteValue(kvp.Value);
                            }
                            w.WriteEndObject();
                        }
                    });
                }

                if (inst.Textures is { Length: > 0 })
                {
                    json.Array("textures", () =>
                    {
                        foreach (string texture in inst.Textures)
                            w.WriteValue(texture);
                    });
                }

                if (inst.PartVisibility is { Count: > 0 })
                {
                    json.Array("part_visibility", () =>
                    {
                        foreach (var kvp in inst.PartVisibility)
                        {
                            w.WriteStartObject();
                            w.WritePropertyName(kvp.Key);
                            w.WriteValue(kvp.Value);
                            w.WriteEndObject();
                        }
                    });
                }

                if (inst.Color is { Length: > 0 })
                {
                    json.Array("color", () =>
                    {
                        foreach (string component in inst.Color)
                            w.WriteValue(component);
                    });
                }

                json.Property("is_hurt_color", inst.IsHurtColor);
                json.Property("ignore_lighting", inst.IgnoreLighting);
                json.Property("rebuild_animation_matrices", inst.RebuildAnimationMatrices);
            });
        });
        w.WriteEndObject();

        CompilerState.Pop();
        return sw.ToString();
    }

    private static void WriteArrayGroup(
        JsonHelper json,
        JsonWriter w,
        string groupName,
        Dictionary<string, string[]> arrays)
    {
        json.Object(groupName, () =>
        {
            foreach (var kvp in arrays)
            {
                json.Array(kvp.Key, () =>
                {
                    foreach (string item in kvp.Value)
                        w.WriteValue(item);
                });
            }
        });
    }

    private sealed class SimpleRenderController(string controllerId) : RenderController
    {
        public override string ControllerId { get; } = controllerId;
    }
}
