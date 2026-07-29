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
        /// <summary>Block component traits.</summary>
        Block,
        /// <summary>Entity component traits.</summary>
        Entity,
        /// <summary>Item component traits.</summary>
        Item
    }

    /// <summary>
    /// Gets all traits of type <typeparamref name="T"/>
    /// </summary>
    /// <param name="constraint"><see cref="TraitType"/> to reflect</param>
    /// <typeparam name="T">Content class to reflect</typeparam>
    public static List<Trait> GetTraits<T>(TraitType constraint) where T : new() =>
        GetTraits(new T(), constraint);

    /// <summary>
    /// Gets all traits of type <paramref name="t"/> by constructing a fresh instance.
    /// Prefer <see cref="GetTraits(object, TraitType)"/> when compiling a pre-configured instance.
    /// </summary>
    /// <param name="t">Content class to reflect</param>
    /// <param name="constraint"><see cref="TraitType"/> to reflect</param>
    public static List<Trait> GetTraits(Type t, TraitType constraint)
    {
        object instance = Activator.CreateInstance(t)
                          ?? throw new ArgumentException($"failed to construct instance of {t.FullName}");
        return GetTraits(instance, constraint);
    }

    /// <summary>
    /// Gets all traits implemented by <paramref name="instance"/>, reading property values from that instance.
    /// </summary>
    /// <param name="instance">Content instance to reflect</param>
    /// <param name="constraint"><see cref="TraitType"/> to reflect</param>
    public static List<Trait> GetTraits(object instance, TraitType constraint)
    {
        ArgumentNullException.ThrowIfNull(instance);

        JsonTextWriter? dummyWriter = null;
        Type t = instance.GetType();
        List<Trait> traits = new();

        foreach (Type iface in t.GetInterfaces())
        {
            TraitAttribute? traitAttr = iface.GetCustomAttribute<TraitAttribute>();
            if (traitAttr == null)
                continue;

            CompilerState.Push(traitAttr.Identifier.ToString());
            if (traitAttr.Constraint != constraint)
                throw new ArgumentException(
                    $"mismatching trait types (expected: {nameof(TraitType)}.{constraint}, got: {nameof(TraitType)}.{traitAttr.Constraint})");

            Trait trait = new(traitAttr.Identifier, iface);
            trait.Properties.AddRange(ReflectTraitProperties(iface, instance, ref dummyWriter));
            traits.Add(trait);

            CompilerState.Pop();
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
        CompilerState.Push("TraitSystem");

        JsonTextWriter? dummyWriter = null;
        Type objectType = typeof(TObject);
        Type interfaceType = typeof(TTrait);

        TObject inst = Activator.CreateInstance<TObject>();

        TraitAttribute? traitAttr = interfaceType.GetCustomAttribute<TraitAttribute>();
        if (traitAttr == null)
            throw new ArgumentException($"type {interfaceType.Name} is not a trait");

        if (traitAttr.Constraint != constraint)
            throw new ArgumentException(
                $"mismatching trait types (expected: {nameof(TraitType)}.{constraint}, got: {nameof(TraitType)}.{traitAttr.Constraint})");

        if (!interfaceType.IsAssignableFrom(objectType))
            throw new ArgumentException($"{objectType.Name} does not implement trait interface {interfaceType.Name}");

        Trait trait = new(traitAttr.Identifier, interfaceType);
        trait.Properties.AddRange(ReflectTraitProperties(interfaceType, inst!, ref dummyWriter));

        CompilerState.Pop();
        return trait;
    }

    private static List<TraitProperty> ReflectTraitProperties(
        Type iface,
        object instance,
        ref JsonTextWriter? dummyWriter)
    {
        List<TraitProperty> properties = new();
        Type concreteType = instance.GetType();

        foreach (PropertyInfo property in iface.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            CompilerState.Push(property.Name);

            TraitPropertyAttribute? propertyAttr = property.GetCustomAttribute<TraitPropertyAttribute>();
            if (propertyAttr == null)
            {
                CompilerState.Pop();
                continue;
            }

            if (IsIngotExcluded(property, iface, concreteType))
            {
                CompilerState.Pop();
                continue;
            }

            MethodInfo? getter = property.GetGetMethod();
            if (getter == null)
                throw new ArgumentException($"property {property.Name} has no getter");

            object? value = null;
            if (property.GetCustomAttribute<IngotTypeOverrideAttribute>() is not null)
            {
                IngotTypeOverrideAttribute overrideAttributeAttr = property.GetCustomAttribute<IngotTypeOverrideAttribute>()!;
                value = overrideAttributeAttr.OverrideValue;
            }
            else
            {
                try
                {
                    value = getter.Invoke(instance, null);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"failed to get value for property {property.Name}: {ex.Message}");
                }
            }

            if (value == null || (value is string str && string.IsNullOrEmpty(str)))
            {
                CompilerState.Pop();
                continue;
            }

            CompilerState.Pop();

            properties.Add(new TraitProperty(
                name: property.Name,
                value: value!
            ));
        }

        return properties;
    }

    /// <summary>
    /// Whether a trait property should be omitted due to <see cref="IngotExcludeAttribute"/>
    /// on the interface member and/or the concrete implementation.
    /// </summary>
    private static bool IsIngotExcluded(PropertyInfo interfaceProperty, Type iface, Type concreteType)
    {
        if (interfaceProperty.GetCustomAttribute<IngotExcludeAttribute>(inherit: true) is not null)
            return true;

        // Implicit public implementation with the same name
        PropertyInfo? publicImpl = concreteType.GetProperty(
            interfaceProperty.Name,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (publicImpl is null)
        {
            // Walk base types for public implementations
            for (Type? t = concreteType; t is not null && t != typeof(object); t = t.BaseType)
            {
                publicImpl = t.GetProperty(
                    interfaceProperty.Name,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (publicImpl is not null)
                    break;
            }
        }

        if (publicImpl?.GetCustomAttribute<IngotExcludeAttribute>(inherit: true) is not null)
            return true;

        // Explicit interface implementation: map interface getter -> target method, then its declaring property
        if (!iface.IsInterface || !iface.IsAssignableFrom(concreteType))
            return false;

        InterfaceMapping map = concreteType.GetInterfaceMap(iface);
        MethodInfo? interfaceGetter = interfaceProperty.GetGetMethod();
        if (interfaceGetter is null)
            return false;

        for (int i = 0; i < map.InterfaceMethods.Length; i++)
        {
            if (map.InterfaceMethods[i] != interfaceGetter)
                continue;

            MethodInfo target = map.TargetMethods[i];
            if (target.GetCustomAttribute<IngotExcludeAttribute>(inherit: true) is not null)
                return true;

            // Attributes on the explicit property itself (via accessor metadata token / declaring type scan)
            foreach (PropertyInfo prop in target.DeclaringType!.GetProperties(
                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (prop.GetGetMethod(nonPublic: true) == target
                    && prop.GetCustomAttribute<IngotExcludeAttribute>(inherit: true) is not null)
                    return true;
            }

            break;
        }

        return false;
    }
    
    /// <summary>
    /// Returns all properties and fields decorated with the specified attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The type of attribute to search for</typeparam>
    /// <param name="type">The type to inspect</param>
    /// <returns>Dictionary: Key = member name, Value = (Attribute instance, MemberInfo)</returns>
    public static Dictionary<string, (TAttribute Attribute, MemberInfo Member)> GetAttributedMembers<TAttribute>(Type type) where TAttribute : Attribute
    {
        if (type == null) 
            throw new ArgumentNullException(nameof(type));

        Dictionary<string, (TAttribute Attribute, MemberInfo Member)> members = new();

        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                   BindingFlags.Instance | BindingFlags.Static;

        // props
        var properties = type.GetProperties(flags)
            .Select(p => (Member: p, Attr: p.GetCustomAttribute<TAttribute>(inherit: true)))
            .Where(x => x.Attr != null);

        foreach (var item in properties)
            members[item.Member.Name] = (item.Attr, item.Member)!;

        // fields
        var fields = type.GetFields(flags)
            .Select(f => (Member: f, Attr: f.GetCustomAttribute<TAttribute>(inherit: true)))
            .Where(x => x.Attr != null);

        foreach (var item in fields)
            // skip backing fields by default
            if (!item.Member.Name.Contains(">k__BackingField"))
                members[item.Member.Name] = (item.Attr, item.Member)!;

        return members;
    }
}