using System.Reflection;

using ingot.Core.Common;
using Newtonsoft.Json;

using Formatting = Newtonsoft.Json.Formatting;

namespace ingot.Core.Behaviour.Entity;

/// <summary>
/// Attribute to tag a material in a <see cref="ClientEntity{TParent}"/>
/// </summary>
public class ClientEntityMaterialAttribute : Attribute
{
    /// <summary>
    /// Identifer of the material. Defaults to member name if null
    /// </summary>
    public readonly string? MaterialId;
    /// <summary>
    /// Constructs the attribute
    /// </summary>
    /// <param name="materialId">Identifer of the material</param>
    public ClientEntityMaterialAttribute(string? materialId = null) => MaterialId = materialId;
}

/// <summary>
/// Attribute to tag a texture in a <see cref="ClientEntity{TParent}"/>
/// </summary>
public class ClientEntityTextureAttribute : Attribute
{
    /// <summary>
    /// Identifer of the texture. Defaults to member name if null
    /// </summary>
    public readonly string? TextureId;
    /// <summary>
    /// Constructs the attribute
    /// </summary>
    /// <param name="textureId">Identifer of the texture</param>
    public ClientEntityTextureAttribute(string? textureId = null) => TextureId = textureId;
}

/// <summary>
/// Attribute to tag a model geometry in a <see cref="ClientEntity{TParent}"/>
/// </summary>
public class ClientEntityGeometryAttribute : Attribute
{
    /// <summary>
    /// Identifer of the geometry. Defaults to member name if null
    /// </summary>
    public readonly string? GeometryId;
    /// <summary>
    /// Constructs the attribute
    /// </summary>
    /// <param name="geometryId">Identifer of the geometry</param>
    public ClientEntityGeometryAttribute(string? geometryId = null) => GeometryId = geometryId;
}

/// <summary>
/// Represents the client side resources of <typeparamref name="TParent"/> <see cref="Entity"/>
/// </summary>
/// <typeparam name="TParent">The parent entity that this <see cref="ClientEntity{TParent}"/> resources define</typeparam>
public abstract class ClientEntity<TParent> : IConcreteCompilable<Entity>, IIdentifiable where TParent : Entity, new()
{
    private static TParent ParentInst => new();
    
    /// <summary>
    /// The identifier of the <see cref="Entity"/> this <see cref="ClientEntity{TParent}"/> is parented to
    /// </summary>
    public Identifier Identifier => ParentInst.Identifier;

    /// <summary>
    /// Default material ID
    /// </summary>
    [ClientEntityMaterial("default")]
    public virtual string DefaultMaterial => "entity";
    
    /// <summary>
    /// Default texture ID
    /// </summary>
    [ClientEntityTexture("default")]
    public abstract string DefaultTexture { get; }

    /// <summary>
    /// Default model ID
    /// </summary>
    [ClientEntityGeometry("default")]
    public virtual string DefaultGeometry => $"geometry.{ParentInst.Identifier.Name}";
    
    /// <inheritdoc/>
    public static string Compile(Type tType)
    {
        ClientEntity<TParent> inst = Activator.CreateInstance<ClientEntity<TParent>>();

        CompilerState.Push(inst.Identifier.ToString());

        StringWriter sw = new();
        JsonTextWriter w = new(sw);
        w.Formatting = Formatting.Indented;
        w.Indentation = 4;

        JsonHelper json = new(ref w);

        w.WriteStartObject();

        Dictionary<string, string> materials = new();
        foreach (var i in TraitSystem.TraitSystem.GetAttributedMembers<ClientEntityMaterialAttribute>(tType))
        {
            var attr = i.Value.Attribute;
            string key = i.Key;
            if (attr.MaterialId is not null)
                key = attr.MaterialId;

            string value = "";
            if (i.Value.Member is PropertyInfo prop)
                value = prop.GetValue(ParentInst)?.ToString()!;
            else if (i.Value.Member is FieldInfo field)
                value = field.GetValue(ParentInst)?.ToString()!;

            materials.Add(key, value);
        }
        Dictionary<string, string> textures = new();
        foreach (var i in TraitSystem.TraitSystem.GetAttributedMembers<ClientEntityTextureAttribute>(tType))
        {
            var attr = i.Value.Attribute;
            string key = i.Key;
            if (attr.TextureId is not null)
                key = attr.TextureId;

            string value = "";
            if (i.Value.Member is PropertyInfo prop)
                value = prop.GetValue(ParentInst)?.ToString()!;
            else if (i.Value.Member is FieldInfo field)
                value = field.GetValue(ParentInst)?.ToString()!;

            textures.Add(key, value);
        }
        Dictionary<string, string> models = new();
        foreach (var i in TraitSystem.TraitSystem.GetAttributedMembers<ClientEntityGeometryAttribute>(tType))
        {
            var attr = i.Value.Attribute;
            string key = i.Key;
            if (attr.GeometryId is not null)
                key = attr.GeometryId;

            string value = "";
            if (i.Value.Member is PropertyInfo prop)
                value = prop.GetValue(ParentInst)?.ToString()!;
            else if (i.Value.Member is FieldInfo field)
                value = field.GetValue(ParentInst)?.ToString()!;

            models.Add(key, value);
        }

        return "";
    }
}