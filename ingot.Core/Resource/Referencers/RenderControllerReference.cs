using ingot.Core.Behaviour.Entity;

namespace ingot.Core.Resource.Referencers;

/// <summary>
/// Represents an entity render controller
/// </summary>
/// <typeparam name="TRenderController">Render controller to reference</typeparam>
public class RenderControllerReference<TRenderController> where TRenderController : RenderController, new()
{
    private readonly string _id;
    
    /// <summary>
    /// Implicitly registers and references a <see cref="RenderController"/>
    /// </summary>
    /// <exception cref="InvalidOperationException">render controller registration only valid during pack compilation</exception>
    public RenderControllerReference()
    {
        Pack pack = CompilerState.CurrentPack 
                    ?? throw new InvalidOperationException("render controller registration only valid during pack compilation");
        
        _id = new TRenderController().ControllerId;

        if (pack.ResourcePack.RegisteredRenderControllerIds.Contains(_id) == false)
        {
            CompilerState.Info($"implicitly registered render controller {_id}");
            pack.ResourcePack.AddRenderController<TRenderController>();
        }
    }

    /// <summary/>
    public static implicit operator string(RenderControllerReference<TRenderController> rc) => rc._id;
    /// <summary/>
    public static implicit operator RenderControllerReference(RenderControllerReference<TRenderController> rc) => new(typeof(TRenderController), rc._id);
}

/// <summary/>
public class RenderControllerReference(Type parent, string reference)
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