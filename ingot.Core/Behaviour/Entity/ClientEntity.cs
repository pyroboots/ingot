using System.Reflection;

using ingot.Core.Common;

using Newtonsoft.Json;

using Formatting = Newtonsoft.Json.Formatting;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Behaviour.Entity;

/// <summary>
/// Attribute to tag a material short-name on a <see cref="ClientEntity"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ClientEntityMaterialAttribute : Attribute
{
    /// <summary>
    /// Short-name of the material. Defaults to the member name lowercased when null.
    /// </summary>
    public readonly string? MaterialId;

    /// <summary>
    /// Constructs the attribute.
    /// </summary>
    /// <param name="materialId">Short-name of the material.</param>
    public ClientEntityMaterialAttribute(string? materialId = null) => MaterialId = materialId;
}

/// <summary>
/// Attribute to tag a texture short-name on a <see cref="ClientEntity"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ClientEntityTextureAttribute : Attribute
{
    /// <summary>
    /// Short-name of the texture. Defaults to the member name lowercased when null.
    /// </summary>
    public readonly string? TextureId;

    /// <summary>
    /// Constructs the attribute.
    /// </summary>
    /// <param name="textureId">Short-name of the texture.</param>
    public ClientEntityTextureAttribute(string? textureId = null) => TextureId = textureId;
}

/// <summary>
/// Attribute to tag a geometry short-name on a <see cref="ClientEntity"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ClientEntityGeometryAttribute : Attribute
{
    /// <summary>
    /// Short-name of the geometry. Defaults to the member name lowercased when null.
    /// </summary>
    public readonly string? GeometryId;

    /// <summary>
    /// Constructs the attribute.
    /// </summary>
    /// <param name="geometryId">Short-name of the geometry.</param>
    public ClientEntityGeometryAttribute(string? geometryId = null) => GeometryId = geometryId;
}

/// <summary>
/// Coloured or textured spawn egg written into <c>minecraft:client_entity</c> description.
/// </summary>
public class ClientEntitySpawnEgg
{
    /// <summary>
    /// Base colour of a vanilla-style spawn egg (e.g. <c>#db7500</c>).
    /// </summary>
    public string? BaseColor { get; init; }

    /// <summary>
    /// Overlay colour of a vanilla-style spawn egg (e.g. <c>#242222</c>).
    /// </summary>
    public string? OverlayColor { get; init; }

    /// <summary>
    /// Item texture short-name (from <c>item_texture.json</c>) used as the spawn egg icon.
    /// </summary>
    public string? Texture { get; init; }

    /// <summary>
    /// Optional texture index when the item texture is an atlas.
    /// </summary>
    public int? TextureIndex { get; init; }
}

/// <summary>
/// Entity sound mapping written to <c>rp/sounds.json</c> under <c>entity_sounds.entities</c>.
/// Maps gameplay sound events (ambient, hurt, death, step, milk, ...) to sound definition names.
/// </summary>
public class ClientEntitySounds
{
    /// <summary>
    /// Base volume for entity sounds.
    /// </summary>
    public float Volume { get; init; } = 1f;

    /// <summary>
    /// Pitch range as <c>[min, max]</c>, or a single pitch value via a one-element array.
    /// </summary>
    public float[]? Pitch { get; init; }

    /// <summary>
    /// Event name → sound definition (e.g. <c>hurt</c> → <c>mob.cow.hurt</c>).
    /// Values may be a sound name string or a richer object (volume/pitch/sound).
    /// </summary>
    public required Dictionary<string, object> Events { get; init; }

    /// <summary>
    /// Maps common gameplay events to vanilla <c>mob.{name}.*</c> sound definitions
    /// (say / hurt / death / step, plus optional milk).
    /// </summary>
    /// <param name="vanillaMobName">Vanilla mob folder name, e.g. <c>cow</c>, <c>pig</c>.</param>
    /// <param name="includeMilk">When true, adds <c>milk</c> → <c>mob.{name}.milk</c>.</param>
    /// <param name="pitch">Optional pitch range (default 0.8-1.2).</param>
    public static ClientEntitySounds FromVanilla(
        string vanillaMobName,
        bool includeMilk = false,
        float[]? pitch = null)
    {
        string mob = vanillaMobName.Trim().ToLowerInvariant();
        Dictionary<string, object> events = new()
        {
            ["ambient"] = $"mob.{mob}.say",
            ["hurt"] = $"mob.{mob}.hurt",
            ["death"] = $"mob.{mob}.death",
            ["step"] = $"mob.{mob}.step",
        };
        if (includeMilk)
            events["milk"] = $"mob.{mob}.milk";

        return new ClientEntitySounds
        {
            Volume = 1f,
            Pitch = pitch ?? [0.8f, 1.2f],
            Events = events,
        };
    }
}

/// <summary>
/// Molang scripts section of a client entity description.
/// </summary>
public class ClientEntityScripts
{
    /// <summary>
    /// Runs when the entity is first initialized / loaded.
    /// </summary>
    public string[]? Initialize { get; init; }

    /// <summary>
    /// Runs every frame before animations.
    /// </summary>
    public string[]? PreAnimation { get; init; }

    /// <summary>
    /// Animation / animation-controller short-names to play each frame.
    /// Entries may be a short-name string, or a single-entry dictionary mapping a short-name to a Molang blend value.
    /// </summary>
    public object[]? Animate { get; init; }

    /// <summary>
    /// Uniform model scale (Molang or number as string).
    /// </summary>
    public string? Scale { get; init; }

    /// <summary>
    /// X-axis model scale.
    /// </summary>
    public string? ScaleX { get; init; }

    /// <summary>
    /// Y-axis model scale.
    /// </summary>
    public string? ScaleY { get; init; }

    /// <summary>
    /// Z-axis model scale.
    /// </summary>
    public string? ScaleZ { get; init; }
}

/// <summary>
/// Resource-pack client entity definition (<c>minecraft:client_entity</c>).
/// Defines short-names for materials, textures, and geometry that render controllers resolve.
/// </summary>
public abstract class ClientEntity : IConcreteCompilable<ClientEntity>, IIdentifiable
{
    /// <inheritdoc/>
    public abstract Identifier Identifier { get; }

    /// <summary>
    /// Format version written to the client entity JSON.
    /// </summary>
    public virtual Version FormatVersion => new("1.10.0");

    /// <summary>
    /// Default material short-name value (maps short-name <c>default</c>).
    /// </summary>
    [ClientEntityMaterial("default")]
    public virtual string DefaultMaterial => "entity";

    /// <summary>
    /// Default texture path (maps short-name <c>default</c>).
    /// Typically a path such as <c>textures/entity/my_mob</c>.
    /// </summary>
    [ClientEntityTexture("default")]
    public abstract string DefaultTexture { get; }

    /// <summary>
    /// Optional path to a source PNG for <see cref="DefaultTexture"/>.
    /// When set, the texture is auto-registered into the resource pack under <c>textures/entity/</c>.
    /// </summary>
    public virtual string? DefaultTexturePath => null;

    /// <summary>
    /// Default geometry identifier (maps short-name <c>default</c>).
    /// </summary>
    [ClientEntityGeometry("default")]
    public virtual string DefaultGeometry => $"geometry.{Identifier.Name}";

    /// <summary>
    /// Render controller identifiers applied to this entity.
    /// Defaults to a per-entity controller that is auto-emitted when not registered explicitly.
    /// </summary>
    public virtual string[] RenderControllers => [$"controller.render.{Identifier.Name}"];

    /// <summary>
    /// When <see langword="true"/>, a simple default render controller is written for any
    /// <c>controller.render.*</c> id listed in <see cref="RenderControllers"/> that was not
    /// registered via <see cref="ResourcePack.AddRenderController{T}"/>.
    /// </summary>
    public virtual bool EmitDefaultRenderController => true;

    /// <summary>
    /// Animation short-name definitions (animation / animation controller identifiers).
    /// </summary>
    public virtual Dictionary<string, string>? Animations => null;

    /// <summary>
    /// Molang scripts (<c>initialize</c>, <c>pre_animation</c>, <c>animate</c>, <c>scale</c>, ...).
    /// </summary>
    public virtual ClientEntityScripts? Scripts => null;

    /// <summary>
    /// Sound effect short-name definitions (for animations).
    /// </summary>
    public virtual Dictionary<string, string>? SoundEffects => null;

    /// <summary>
    /// Gameplay entity sounds written to <c>rp/sounds.json</c> (<c>entity_sounds.entities</c>).
    /// Without this, custom entities only get generic damage sounds.
    /// </summary>
    public virtual ClientEntitySounds? EntitySounds => null;

    /// <summary>
    /// Particle effect short-name definitions.
    /// </summary>
    public virtual Dictionary<string, string>? ParticleEffects => null;

    /// <summary>
    /// Optional spawn egg appearance.
    /// </summary>
    public virtual ClientEntitySpawnEgg? SpawnEgg => null;

    /// <summary>
    /// Whether attachables (held items, etc.) can be attached to this entity.
    /// </summary>
    public virtual bool? EnableAttachables => null;

    /// <summary>
    /// When <see langword="true"/>, armor is worn but not rendered.
    /// </summary>
    public virtual bool? HideArmor => null;

    /// <summary>
    /// Optional <c>min_engine_version</c> for the client entity description
    /// (required for player persona skins to stay less than 1.13.0).
    /// </summary>
    public virtual string? MinEngineVersion => null;

    /// <summary>
    /// Extra texture short-name -> path entries merged into the client entity
    /// <c>textures</c> map (in addition to attributed members / <see cref="DefaultTexture"/>).
    /// Useful when many block textures must be listed for a render-controller array.
    /// </summary>
    public virtual Dictionary<string, string>? ExtraTextures => null;

    /// <inheritdoc/>
    public static string Compile(Type tType)
    {
        ClientEntity inst = (Activator.CreateInstance(tType) as ClientEntity)!;
        return CompileFromInstance(inst);
    }

    /// <inheritdoc/>
    public static string Compile<TConcreteType>() where TConcreteType : ClientEntity, new() =>
        Compile(typeof(TConcreteType));

    /// <inheritdoc/>
    public static string CompileFromInstance(ClientEntity inst)
    {
        Type tType = inst.GetType();
        
        CompilerState.Push(inst.Identifier.ToString());

        StringWriter sw = new();
        JsonTextWriter w = new(sw)
        {
            Formatting = Formatting.Indented,
            Indentation = 4,
        };

        JsonHelper json = new(ref w);

        Dictionary<string, string> materials = CollectShortNames<ClientEntityMaterialAttribute>(
            tType, inst, attr => attr.MaterialId);
        Dictionary<string, string> textures = CollectShortNames<ClientEntityTextureAttribute>(
            tType, inst, attr => attr.TextureId);
        Dictionary<string, string> geometries = CollectShortNames<ClientEntityGeometryAttribute>(
            tType, inst, attr => attr.GeometryId);

        // Ensure the three required short-names exist even if attributes are not inherited on overrides.
        materials["default"] = inst.DefaultMaterial;
        textures["default"] = inst.DefaultTexture;
        geometries["default"] = inst.DefaultGeometry;

        if (inst.ExtraTextures is { Count: > 0 })
        {
            foreach (var (shortName, path) in inst.ExtraTextures)
            {
                if (string.IsNullOrWhiteSpace(shortName) || string.IsNullOrWhiteSpace(path))
                    continue;
                textures[shortName] = path;
            }
        }

        TryRegisterDefaultTexture(inst, ref w);

        w.WriteStartObject();
        json.Property("format_version", inst.FormatVersion.ToString());
        json.Object("minecraft:client_entity", () =>
        {
            json.Object("description", () =>
            {
                json.Property("identifier", inst.Identifier);
                json.Property("min_engine_version", inst.MinEngineVersion);

                WriteStringMap(json, "materials", materials);
                WriteStringMap(json, "textures", textures);
                WriteStringMap(json, "geometry", geometries);

                if (inst.RenderControllers is { Length: > 0 })
                {
                    json.Array("render_controllers", () =>
                    {
                        foreach (string controller in inst.RenderControllers)
                            w.WriteValue(controller);
                    });
                }

                if (inst.Animations is { Count: > 0 })
                    WriteStringMap(json, "animations", inst.Animations);

                if (inst.Scripts is not null)
                    WriteScripts(json, w, inst.Scripts);

                if (inst.SoundEffects is { Count: > 0 })
                    WriteStringMap(json, "sound_effects", inst.SoundEffects);

                if (inst.ParticleEffects is { Count: > 0 })
                    WriteStringMap(json, "particle_effects", inst.ParticleEffects);

                if (inst.SpawnEgg is not null)
                    WriteSpawnEgg(json, inst.SpawnEgg);

                json.Property("enable_attachables", inst.EnableAttachables);
                json.Property("hide_armor", inst.HideArmor);
            });
        });
        w.WriteEndObject();

        CompilerState.Pop();
        return sw.ToString();
    }

    private static void TryRegisterDefaultTexture(ClientEntity inst, ref JsonTextWriter w)
    {
        if (CompilerState.CurrentPack is null || string.IsNullOrWhiteSpace(inst.DefaultTexturePath))
            return;

        string texturePath = inst.DefaultTexture;
        if (string.IsNullOrWhiteSpace(texturePath))
            return;

        // textures/entity/foo/bar -> foo/bar (path under textures/entity)
        string relative = texturePath
            .Replace('\\', '/')
            .TrimStart('/');
        const string prefix = "textures/entity/";
        if (relative.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            relative = relative[prefix.Length..];

        if (string.IsNullOrWhiteSpace(relative))
            return;

        JsonTextWriter? warnWriter = w;
        ResourcePack rp = CompilerState.CurrentPack.ResourcePack;
        if (rp.TryAddEntityTexture(relative, inst.DefaultTexturePath))
            CompilerState.Info($"auto-registered entity texture '{relative}'");
        else
            CompilerState.Warn(ref warnWriter, $"entity texture '{relative}' was not auto-registered because it is already defined on the resource pack");
    }

    private static Dictionary<string, string> CollectShortNames<TAttribute>(
        Type tType,
        ClientEntity inst,
        Func<TAttribute, string?> getId) where TAttribute : Attribute
    {
        Dictionary<string, string> result = new();

        foreach (var entry in TraitSystem.TraitSystem.GetAttributedMembers<TAttribute>(tType))
        {
            TAttribute attr = entry.Value.Attribute;
            string key = getId(attr) ?? ToShortName(entry.Key);
            string value = ReadMemberString(inst, entry.Value.Member);

            if (string.IsNullOrEmpty(value))
            {
                JsonTextWriter? dummy = null;
                CompilerState.Warn(ref dummy, $"client entity short-name '{key}' has an empty value (member {entry.Key})");
            }

            if (!result.TryAdd(key, value))
            {
                JsonTextWriter? dummy = null;
                CompilerState.Warn(ref dummy, $"duplicate client entity short-name '{key}' from member {entry.Key}; keeping first value");
            }
        }

        return result;
    }

    private static string ReadMemberString(object inst, MemberInfo member)
    {
        object? raw = member switch
        {
            PropertyInfo prop => prop.GetValue(prop.GetMethod?.IsStatic == true ? null : inst),
            FieldInfo field => field.GetValue(field.IsStatic ? null : inst),
            _ => null,
        };
        return raw?.ToString() ?? string.Empty;
    }

    private static string ToShortName(string memberName)
    {
        if (string.IsNullOrEmpty(memberName))
            return memberName;

        // DefaultMaterial / DefaultTexture / DefaultGeometry → default when prefixed
        if (memberName.StartsWith("Default", StringComparison.Ordinal) && memberName.Length > "Default".Length)
            return "default";

        return char.ToLowerInvariant(memberName[0]) + memberName[1..];
    }

    private static void WriteStringMap(JsonHelper json, string key, Dictionary<string, string> map)
    {
        if (map.Count == 0)
            return;

        json.Object(key, () =>
        {
            foreach (var kvp in map)
                json.Property(kvp.Key, kvp.Value);
        });
    }

    private static void WriteScripts(JsonHelper json, JsonTextWriter w, ClientEntityScripts scripts)
    {
        json.Object("scripts", () =>
        {
            if (scripts.Initialize is { Length: > 0 })
            {
                json.Array("initialize", () =>
                {
                    foreach (string line in scripts.Initialize)
                        w.WriteValue(line);
                });
            }

            if (scripts.PreAnimation is { Length: > 0 })
            {
                json.Array("pre_animation", () =>
                {
                    foreach (string line in scripts.PreAnimation)
                        w.WriteValue(line);
                });
            }

            if (scripts.Animate is { Length: > 0 })
            {
                json.Array("animate", () =>
                {
                    foreach (object entry in scripts.Animate)
                    {
                        if (entry is string shortName)
                        {
                            w.WriteValue(shortName);
                        }
                        else if (entry is Dictionary<string, string> blend)
                        {
                            w.WriteStartObject();
                            foreach (var kvp in blend)
                            {
                                w.WritePropertyName(kvp.Key);
                                w.WriteValue(kvp.Value);
                            }
                            w.WriteEndObject();
                        }
                        else if (entry is IReadOnlyDictionary<string, string> blendRo)
                        {
                            w.WriteStartObject();
                            foreach (var kvp in blendRo)
                            {
                                w.WritePropertyName(kvp.Key);
                                w.WriteValue(kvp.Value);
                            }
                            w.WriteEndObject();
                        }
                        else
                        {
                            JsonSerializer.CreateDefault().Serialize(w, entry);
                        }
                    }
                });
            }

            json.Property("scale", scripts.Scale);
            json.Property("scaleX", scripts.ScaleX);
            json.Property("scaleY", scripts.ScaleY);
            json.Property("scaleZ", scripts.ScaleZ);
        });
    }

    private static void WriteSpawnEgg(JsonHelper json, ClientEntitySpawnEgg egg)
    {
        json.Object("spawn_egg", () =>
        {
            json.Property("base_color", egg.BaseColor);
            json.Property("overlay_color", egg.OverlayColor);
            json.Property("texture", egg.Texture);
            if (egg.TextureIndex is not null)
                json.Property("texture_index", egg.TextureIndex);
        });
    }
}

/// <summary>
/// Client entity whose identifier is taken from the behaviour-side parent <typeparamref name="TParent"/>.
/// </summary>
/// <typeparam name="TParent">Behaviour entity this client definition visualises.</typeparam>
public abstract class ClientEntity<TParent> : ClientEntity where TParent : Entity, new()
{
    private static TParent ParentInst => new();

    /// <inheritdoc/>
    public override Identifier Identifier => ParentInst.Identifier;
}
