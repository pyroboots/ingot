using System.Reflection;

namespace ingot.Core.TraitSystem;

public class Item
{
    private static IEnumerable<MethodInfo> GetMethods(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        List<MethodInfo> result = new();

        // get all declared methods on the class
        MethodInfo[] declaredMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (MethodInfo method in declaredMethods)
        {
            Console.WriteLine(method.Name);

            foreach (Type iface in type.GetInterfaces()
                         .Where(i => i.GetCustomAttribute<TraitAttribute>() != null))
            {
                Console.WriteLine(iface.GetMethod(method.Name)?.GetCustomAttribute<TraitPropertyAttribute>()?.Path);
            }
            
            result.Add(method);
        }

        // get methods not overriden from interface
        foreach (Type iface in type.GetInterfaces().Where(i => i.GetCustomAttribute<TraitAttribute>() != null))
        {
            InterfaceMapping map = type.GetInterfaceMap(iface);

            for (int i = 0; i < map.InterfaceMethods.Length; i++)
            {
                var interfaceMethod = map.InterfaceMethods[i];
                var targetMethod = map.TargetMethods[i];

                // if implemented in interface
                if (targetMethod.DeclaringType == iface && interfaceMethod.GetCustomAttribute<TraitPropertyAttribute>() != null)
                {
                    result.Add(interfaceMethod);
                }
            }
        }

        return result.DistinctBy(m => m.Name); // just in case any duplicates
    }
    
    public static string Compile<TItem>()
    {
        Type item = typeof(TItem);
        TItem instance = Activator.CreateInstance<TItem>();

        foreach (MethodInfo mi in GetMethods(item))
        {
            Console.WriteLine($"n: {mi.Name}");
            Console.WriteLine($"v: {mi.Invoke(instance, null)}");
            Console.WriteLine($"a: {mi.GetCustomAttribute<TraitPropertyAttribute>()?.Path}");
        }

        return "";
    }
}