using System.Reflection;
using Newtonsoft.Json;

namespace ingot.Core.TraitSystem;

/// <summary>
/// Handles trait reflection
/// </summary>
public static class TraitSystem
{
    /// <summary>
    /// Type of trait
    /// </summary>
    public enum TraitType
    {
        Block,
        Entity,
        Item
    }

    /// <summary>
    /// Gets all traits of type <typeparamref name="T"/>
    /// </summary>
    /// <param name="constraint"><see cref="TraitType"/> to reflect</param>
    /// <typeparam name="T">Content class to reflect</typeparam>
    public static List<Trait> GetTraits<T>(TraitType constraint) => GetTraits(typeof(T), constraint);
    /// <summary>
    /// Gets all traits of type <paramref name="t"/>
    /// </summary>
    /// <param name="t"></param>
    /// <param name="constraint"><see cref="Type"/> of content class to reflect</param>
    public static List<Trait> GetTraits(Type t, TraitType constraint)
    {
        JsonTextWriter? dummyWriter = null;
        Type type = t;
        object instance = Activator.CreateInstance(t)!;
        List<Trait> traits = new();
    
        // get all trait interfaces
        foreach (Type iface in type.GetInterfaces())
        {
            TraitAttribute? traitAttr = iface.GetCustomAttribute<TraitAttribute>();
            // skip if not trait
            if (traitAttr == null)
                continue;
            
            CompileTimeLogging.Push(traitAttr.Identifier.ToString());
            if (traitAttr.Constraint != constraint)
            {
                CompileTimeLogging.Warn(ref dummyWriter, 
                    $"mismatching trait types (expected: {nameof(TraitType)}.{constraint}, got: {nameof(TraitType)}.{traitAttr.Constraint}), omitting from compiled json");
                continue;
            }
            
            Trait trait = new(traitAttr.Identifier, iface);
            traits.Add(trait);
    
            // get all properties on the interface including inherited
            foreach (PropertyInfo property in iface.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                CompileTimeLogging.Push(property.Name);
                
                TraitPropertyAttribute? propertyAttr = property.GetCustomAttribute<TraitPropertyAttribute>();
                if (propertyAttr == null)
                    continue;
    
                // since were using properties now instead of methods,
                // we get the getter method of the property
                MethodInfo? getter = property.GetGetMethod();
                if (getter == null)
                {
                    CompileTimeLogging.Warn(ref dummyWriter, $"property {property.Name} has no getter, omitting");
                    continue;
                }
    
                object? value = null;
                try
                {
                    // get the value by invoking the getter
                    value = getter.Invoke(instance, null);
                }
                catch (Exception ex)
                {
                    CompileTimeLogging.Warn(ref dummyWriter, $"failed to get value for property {property.Name}: {ex.Message}");
                }
    
                if (value == null || (value is string str && string.IsNullOrEmpty(str)))
                    CompileTimeLogging.Warn(ref dummyWriter, "value is null or empty, entry omitted from compiled json");
    
                CompileTimeLogging.Pop();
    
                TraitProperty traitProperty = new TraitProperty(
                    path: propertyAttr.Path,
                    name: property.Name,           // use prop name instead of method name
                    value: value!
                );
    
                trait.Properties.Add(traitProperty);
            }
            
            CompileTimeLogging.Pop();
        }
    
        return traits;
    }
    
    /// <summary>
    /// Gets the implemented <typeparamref name="TTrait"/> of <typeparamref name="TObject"/>
    /// </summary>
    /// <param name="constraint"><see cref="TraitType"/> to reflect</param>
    /// <typeparam name="TObject">Content class to reflect</typeparam>
    /// <typeparam name="TTrait">Trait interface to reflect in <typeparamref name="TObject"/></typeparam>
    public static Trait GetTrait<TObject, TTrait>(TraitType constraint) where TTrait : class
    {
        CompileTimeLogging.Push("TraitSystem");
        
        JsonTextWriter? dummyWriter = null;
        Type objectType = typeof(TObject);
        Type interfaceType = typeof(TTrait);
    
        // inst to read vals
        TObject inst = Activator.CreateInstance<TObject>();
    
        // is it actually a trait?
        TraitAttribute? traitAttr = interfaceType.GetCustomAttribute<TraitAttribute>();
        if (traitAttr == null)
        {
            string err = $"type {interfaceType.Name} is not a trait";
            CompileTimeLogging.Warn(ref dummyWriter, err);
            throw new ArgumentException(err);
        }
    
        if (traitAttr.Constraint != constraint)
        {
            string err =
                $"mismatching trait types (expected: {nameof(TraitType)}.{constraint}, got: {nameof(TraitType)}.{traitAttr.Constraint})";
            CompileTimeLogging.Warn(ref dummyWriter, err);
            throw new ArgumentException(err);
        }
        
        // it should, but if it doesnt
        if (!interfaceType.IsAssignableFrom(objectType))
        {
            string err = $"{objectType.Name} does not implement trait interface {interfaceType.Name}";
            CompileTimeLogging.Warn(ref dummyWriter, err);
            throw new ArgumentException(err);
        }
    
        Trait trait = new(traitAttr.Identifier, interfaceType);
    
        // get props
        foreach (PropertyInfo property in interfaceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            CompileTimeLogging.Push(property.Name);
            
            TraitPropertyAttribute? propertyAttr = property.GetCustomAttribute<TraitPropertyAttribute>();
            if (propertyAttr == null)
            {
                CompileTimeLogging.Pop();
                continue;
            }
    
            MethodInfo? getter = property.GetGetMethod();
            if (getter == null)
            {
                CompileTimeLogging.Warn(ref dummyWriter, $"property {property.Name} has no getter, omitting");
                CompileTimeLogging.Pop();
                continue;
            }
    
            object? value = null;
            try
            {
                value = getter.Invoke(inst, null);
            }
            catch (Exception ex)
            {
                CompileTimeLogging.Warn(ref dummyWriter, 
                    $"failed to get value for property {property.Name}: {ex.Message}");
            }
    
            if (value == null || (value is string str && string.IsNullOrEmpty(str)))
                CompileTimeLogging.Warn(ref dummyWriter, $"value for {property.Name} is null or empty");
    
            CompileTimeLogging.Pop();
    
            TraitProperty traitProperty = new TraitProperty(
                path: propertyAttr.Path,
                name: property.Name,
                value: value!
            );
    
            trait.Properties.Add(traitProperty);
        }
    
        CompileTimeLogging.Pop();
        return trait;
    }
}