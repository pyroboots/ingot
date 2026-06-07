using System.Reflection;
using Newtonsoft.Json;

namespace ingot.Core.TraitSystem;

public class TraitSystem
{
    public enum TraitType
    {
        Block,
        Entity,
        Item
    }
    
    public static List<Trait> GetTraits<T>(TraitType constraint)
{
    CompileTimeLogging.Push("TraitSystem");
    
    JsonTextWriter? dummyWriter = null;
    Type type = typeof(T);
    T instance = Activator.CreateInstance<T>();
    List<Trait> traits = new();

    // get all trait interfaces
    foreach (Type iface in type.GetInterfaces())
    {
        TraitAttribute? traitAttr = iface.GetCustomAttribute<TraitAttribute>();
        // skip if not trait
        if (traitAttr == null)
            continue;
        
        CompileTimeLogging.Push(traitAttr.Identifier);
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
    CompileTimeLogging.Pop();

    return traits;
}
}