using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Entity;
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;

namespace ingot.Core.Resource;

/// <summary>
/// Represents a Minecraft geometry
/// </summary>
/// <typeparam name="TParent">Parent class of the geometry. Determines the type of geometry during registration</typeparam>
public class GeometryReference<TParent> where TParent : class, IIdentifiable, ITraitable, IConcreteCompilable<TParent>, new()
{
    private readonly string _id;
    
    /// <summary>
    /// Implicitly registers and references geometry
    /// </summary>
    /// <param name="path">Path to source geometry</param>
    /// <param name="id">Key for the geometry. if null, one will be automatically generated in the format <c>namespace_name_filename</c></param>
    /// <exception cref="InvalidOperationException">geometry registration only valid during pack compilation</exception>
    /// <exception cref="ArgumentException">TParent must be Entity, Item, or Block</exception>
    public GeometryReference(string path, string? id = null)
    {
        Pack pack = CompilerState.CurrentPack 
                    ?? throw new InvalidOperationException("geometry registration only valid during pack compilation");

        Identifier identifier = new TParent().Identifier;
        _id = id ?? $"{identifier.Namespace}_{identifier.Name}_{Path.GetFileNameWithoutExtension(path)}";
        
        if (pack.ResourcePack.GeometrySources.ContainsKey(_id))
            return;
        
        if (typeof(TParent).IsAssignableTo(typeof(Entity)))
            pack.ResourcePack.AddGeometry(_id, path, modelsSubdir: "entity");
        else if (typeof(TParent).IsAssignableTo(typeof(Block)))
            pack.ResourcePack.AddGeometry(_id, path, modelsSubdir: "blocks");
        else
            throw new ArgumentException("TParent must be Entity or Block");
        
        CompilerState.Info($"implicitly registered geometry {_id}");
    }

    /// <summary/>
    public static implicit operator string(GeometryReference<TParent> geo) => geo._id;
    /// <summary/>
    public static implicit operator GeometryReference(GeometryReference<TParent> geo) => new(typeof(TParent), geo._id);
}

/// <summary/>
public class GeometryReference(Type parent, string reference)
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