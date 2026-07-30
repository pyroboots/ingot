using System.Reflection;

using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Entity;
using ingot.Core.Behaviour.Item;
using Newtonsoft.Json;
using Version = ingot.Core.Common.Version;

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

            if (GetAttribute<IngotExcludeAttribute>(property, iface, concreteType) is not null)
            {
                CompilerState.Pop();
                continue;
            }

            object? value = null;
            if (GetAttribute<IngotOverrideAttribute>(property, iface, concreteType) is not null)
            {
                IngotOverrideAttribute overrideAttributeAttr = GetAttribute<IngotOverrideAttribute>(property, iface, concreteType)!;
                value = overrideAttributeAttr.OverrideValue;
            }
            else
            {
                MethodInfo? getter = property.GetGetMethod();
                if (getter == null)
                    throw new ArgumentException($"property {property.Name} has no getter");
                
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
            
            ValidateProperty(value, property, iface, concreteType);

            CompilerState.Pop();

            properties.Add(new TraitProperty(
                name: property.Name,
                value: value!
            ));
        }

        return properties;
    }
    
    private static T? GetAttribute<T>(PropertyInfo interfaceProperty, Type iface, Type concreteType) where T : Attribute
    {
        if (interfaceProperty.GetCustomAttribute<T>(inherit: true) is not null)
            return interfaceProperty.GetCustomAttribute<T>(inherit: true);

        // implicit public implementation with the same name
        PropertyInfo? publicImpl = concreteType.GetProperty(
            interfaceProperty.Name,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (publicImpl is null)
        {
            // walk base types for public implementations
            for (Type? t = concreteType; t is not null && t != typeof(object); t = t.BaseType)
            {
                publicImpl = t.GetProperty(
                    interfaceProperty.Name,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (publicImpl is not null)
                    break;
            }
        }

        if (publicImpl?.GetCustomAttribute<T>(inherit: true) is not null)
            return publicImpl?.GetCustomAttribute<T>(inherit: true);

        // explicit interface implementation: map interface getter -> target method, then its declaring property
        if (!iface.IsInterface || !iface.IsAssignableFrom(concreteType))
            return null;

        InterfaceMapping map = concreteType.GetInterfaceMap(iface);
        MethodInfo? interfaceGetter = interfaceProperty.GetGetMethod();
        if (interfaceGetter is null)
            return null;

        for (int i = 0; i < map.InterfaceMethods.Length; i++)
        {
            if (map.InterfaceMethods[i] != interfaceGetter)
                continue;

            MethodInfo target = map.TargetMethods[i];
            if (target.GetCustomAttribute<T>(inherit: true) is not null)
                return target.GetCustomAttribute<T>(inherit: true);

            // attributes on the explicit property itself (via accessor metadata token / declaring type scan)
            foreach (PropertyInfo prop in target.DeclaringType!.GetProperties(
                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (prop.GetGetMethod(nonPublic: true) == target
                    && prop.GetCustomAttribute<T>(inherit: true) is not null)
                    return prop.GetCustomAttribute<T>(inherit: true);
            }

            break;
        }

        return null;
    }
    
    private static T[]? GetAttributes<T>(PropertyInfo interfaceProperty, Type iface, Type concreteType) where T : Attribute
    {
        if (interfaceProperty.GetCustomAttributes<T>(inherit: true).Any())
            return interfaceProperty.GetCustomAttributes<T>(inherit: true).ToArray();

        // implicit public implementation with the same name
        PropertyInfo? publicImpl = concreteType.GetProperty(
            interfaceProperty.Name,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (publicImpl is null)
        {
            // walk base types for public implementations
            for (Type? t = concreteType; t is not null && t != typeof(object); t = t.BaseType)
            {
                publicImpl = t.GetProperty(
                    interfaceProperty.Name,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (publicImpl is not null)
                    break;
            }
        }

        if (publicImpl?.GetCustomAttributes<T>(inherit: true) is not null && (publicImpl?.GetCustomAttributes<T>(inherit: true)!).Any())
            return publicImpl?.GetCustomAttributes<T>(inherit: true).ToArray();

        // explicit interface implementation: map interface getter -> target method, then its declaring property
        if (!iface.IsInterface || !iface.IsAssignableFrom(concreteType))
            return null;

        InterfaceMapping map = concreteType.GetInterfaceMap(iface);
        MethodInfo? interfaceGetter = interfaceProperty.GetGetMethod();
        if (interfaceGetter is null)
            return null;

        for (int i = 0; i < map.InterfaceMethods.Length; i++)
        {
            if (map.InterfaceMethods[i] != interfaceGetter)
                continue;

            MethodInfo target = map.TargetMethods[i];
            if (target.GetCustomAttributes<T>(inherit: true).Any())
                return target.GetCustomAttributes<T>(inherit: true).ToArray();

            // attributes on the explicit property itself (via accessor metadata token / declaring type scan)
            foreach (PropertyInfo prop in target.DeclaringType!.GetProperties(
                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (prop.GetGetMethod(nonPublic: true) == target
                    && prop.GetCustomAttributes<T>(inherit: true).Any())
                    return prop.GetCustomAttributes<T>(inherit: true).ToArray();
            }

            break;
        }

        return null;
    }

    private static void ValidateProperty(object? value, PropertyInfo interfaceProperty, Type iface, Type concreteType)
    {
        if (GetAttributes<IngotValueConstraintAttribute>(interfaceProperty, iface, concreteType) is null) return;
        
        IngotValueConstraintAttribute[] constraints = GetAttributes<IngotValueConstraintAttribute>(interfaceProperty, iface, concreteType)!;
        IngotValueWarningAttribute[] warnings = GetAttributes<IngotValueWarningAttribute>(interfaceProperty, iface, concreteType) == null ? [] : GetAttributes<IngotValueWarningAttribute>(interfaceProperty, iface, concreteType)!;

        Exception? validate(IngotValueConstraintAttribute.Operator op, object[] targets)
        {
            if (op == IngotValueConstraintAttribute.Operator.NotEqual)
            {
                foreach (object target in targets)
                    if (value == target)
                        return new ArgumentException($"value ({value}) must not equal {target}");
            }
            else if (op == IngotValueConstraintAttribute.Operator.GreaterThan)
            {
                if (value is not int or float)
                    throw new ArgumentException($"value ({value}) type must be a number");
                
                float valueAsNum = (float)value;
                float[] targetsAsNums = targets.Select((v) =>
                {
                    if (v is not int or float)
                        throw new ArgumentException($"valid ({value}) value type must be a number");
                    return (float)v;
                }).ToArray();
                
                // do the inverse to throw
                foreach (float target in targetsAsNums) if (valueAsNum < target)
                    return new ArgumentException($"value ({value}) must be greater than {target}");
            }
            else if (op == IngotValueConstraintAttribute.Operator.LessThan)
            {
                if (value is not int or float)
                    throw new ArgumentException($"value ({value}) type must be a number");
                
                float valueAsNum = (float)value;
                float[] targetsAsNums = targets.Select((v) =>
                {
                    if (v is not int or float)
                        throw new ArgumentException($"valid ({value}) value type must be a number");
                    return (float)v;
                }).ToArray();
                
                // do the inverse to throw
                foreach (float target in targetsAsNums) if (valueAsNum > target) 
                    return new ArgumentException($"value ({value}) must be less than {target}");
            }
            else if (op == IngotValueConstraintAttribute.Operator.OneOf)
            {
                if (targets.Contains(value) == false)
                    return new ArgumentException($"value ({value}) must be one of: {string.Join(' ', targets.Select((i) => $"'{i}'"))}");
            }
            else if (op == IngotValueConstraintAttribute.Operator.MinVer)
            {
                if (targets[0] is not Version)
                    return new ArgumentException($"target must be version");
                Version targetFmtVer = (Version)targets[0];
                Version currentFmtVer;
                object inst = Activator.CreateInstance(concreteType)!;
                if (inst is Item)
                {
                    Item item = (inst as Item)!;
                    currentFmtVer = item.FormatVersion;
                }
                else if (inst is Block)
                {
                    Block entity = (inst as Block)!;
                    currentFmtVer = entity.FormatVersion;
                }
                else if (inst is Entity)
                {
                    Block block = (inst as Block)!;
                    currentFmtVer = block.FormatVersion;
                }
                else return new Exception($"{concreteType.Name} is not supported");
                
                if (targetFmtVer > currentFmtVer)
                    return new Exception($"{interfaceProperty.Name} requires minimum format version of {targetFmtVer}");
            }

            return null;
        }
        
        foreach (IngotValueConstraintAttribute constraint in constraints)
        {
            IngotValueConstraintAttribute.Operator op = constraint.Operation;
            object[] targets = constraint.Values;
            
            Exception? ex = validate(op, targets);
            if (ex is not null) throw ex;
        }
        
        foreach (IngotValueWarningAttribute warning in warnings)
        {
            IngotValueConstraintAttribute.Operator op = warning.Operation;
            object[] targets = warning.Values;
            
            Exception? ex = validate(op, targets);
            JsonTextWriter? dummy = null;
            if (ex is not null) CompilerState.Warn(ref dummy, warning.Warning.Replace("{x}", value!.ToString()));
        }
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