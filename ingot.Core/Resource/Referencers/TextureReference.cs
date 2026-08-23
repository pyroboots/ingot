using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Entity;
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;

namespace ingot.Core.Resource.Referencers;

/// <summary>
/// Represents a Minecraft texture
/// </summary>
/// <typeparam name="TParent">Parent class of the texture. Determines the type of texture during registration</typeparam>
public class TextureReference<TParent> where TParent : class, IIdentifiable, ITraitable, IConcreteCompilable<TParent>, new()
{
    private readonly string _id;
    
    /// <summary>
    /// Implicitly registers and references a texture.
    /// Can be manually overriden with the <see cref="Pack"/> AddTexture method
    /// </summary>
    /// <param name="path">Path to source texture</param>
    /// <param name="id">Key for the texture. if null, one will be automatically generated in the format <c>namespace_name_filename</c></param>
    /// <exception cref="InvalidOperationException">Texture registration only valid during pack compilation</exception>
    /// <exception cref="ArgumentException">TParent must be Entity, Item, or Block</exception>
    public TextureReference(string path, string? id = null)
    {
        Pack pack = CompilerState.CurrentPack 
                    ?? throw new InvalidOperationException("texture registration only valid during pack compilation");

        Identifier identifier = new TParent().Identifier;
        _id = id ?? $"{identifier.Namespace}_{identifier.Name}_{Path.GetFileNameWithoutExtension(path)}";

        bool registered;
        if (typeof(TParent).IsAssignableTo(typeof(Entity)))
            registered = pack.ResourcePack.Textures.TryAddEntityTexture(_id, path);
        else if (typeof(TParent).IsAssignableTo(typeof(Item)))
            registered = pack.ResourcePack.Textures.TryAddItemTexture(_id, path);
        else if (typeof(TParent).IsAssignableTo(typeof(Block)))
            registered = pack.ResourcePack.Textures.TryAddBlockTexture(_id, path);
        else
            throw new ArgumentException("TParent must be Entity, Item, or Block");
        
        if (registered) CompilerState.Info($"implicitly registered texture {_id}");
    }

    /// <summary/>
    public static implicit operator string(TextureReference<TParent> tex) => tex._id;
    /// <summary/>
    public static implicit operator TextureReference(TextureReference<TParent> tex) => new(typeof(TParent), tex._id);
}

/// <summary/>
public class TextureReference(Type parent, string reference)
{
    /// <summary>
    /// Underlying type of the reference
    /// </summary>
    public Type Parent = parent;
    /// <summary>
    /// Implicit reference string of the asset
    /// </summary>
    public string Reference = reference;
}