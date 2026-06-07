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
        Type type =  typeof(T);
        T instance = Activator.CreateInstance<T>();
        List<Trait> traits = new();

        // find all trait interfaces
        foreach (Type iface in type.GetInterfaces())
        {
            TraitAttribute? traitAttr = iface.GetCustomAttribute<TraitAttribute>();
            // skip if not a trait
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

            InterfaceMapping map = type.GetInterfaceMap(iface);
            for (int i = 0; i < map.InterfaceMethods.Length; i++)
            {
                MethodInfo interfaceMethod = map.InterfaceMethods[i];
                MethodInfo targetMethod = map.TargetMethods[i];

                TraitPropertyAttribute? propertyAttr = interfaceMethod.GetCustomAttribute<TraitPropertyAttribute>();
                if (propertyAttr == null)
                    continue;

                // get the actual method to call
                MethodInfo methodToCall = targetMethod.DeclaringType == iface 
                    ? interfaceMethod 
                    : targetMethod;
                
                CompileTimeLogging.Push(methodToCall.Name);
                object? value;
                value = methodToCall.Invoke(instance, null);
                if (value == null || (value is string && value as string is ""))
                    CompileTimeLogging.Warn(ref dummyWriter, "value is null or empty, entry omitted from compiled json");
                CompileTimeLogging.Pop();

                TraitProperty traitProperty = new TraitProperty(
                    path: propertyAttr.Path,
                    name: interfaceMethod.Name,
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