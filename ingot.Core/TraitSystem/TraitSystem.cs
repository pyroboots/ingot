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

            ValidateTraitFormatVersion(iface, instance);

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

        ValidateTraitFormatVersion(interfaceType, inst!);

        Trait trait = new(traitAttr.Identifier, interfaceType);
        trait.Properties.AddRange(ReflectTraitProperties(interfaceType, inst!, ref dummyWriter));

        CompilerState.Pop();
        return trait;
    }

    private static void ValidateTraitFormatVersion(Type iface, object instance)
    {
        TraitFormatVersionAttribute? req = iface.GetCustomAttribute<TraitFormatVersionAttribute>(inherit: true);
        if (req is null)
            return;

        Version required = req.GetMinimumVersion();
        Version current = GetContentFormatVersion(instance);
        if (current >= required)
            return;

        string traitName = iface.GetCustomAttribute<TraitAttribute>()?.Identifier.ToString() ?? iface.Name;
        throw new ArgumentException(
            $"{traitName} requires minimum format version {required}, but {instance.GetType().Name} has {current}");
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
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="interfaceProperty"></param>
    /// <param name="iface"></param>
    /// <param name="concreteType"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? GetAttribute<T>(PropertyInfo interfaceProperty, Type iface, Type concreteType) where T : Attribute
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

    private static void ValidateProperty(
        object? value,
        PropertyInfo interfaceProperty,
        Type iface,
        Type concreteType)
    {
        TraitPropertyConstraintAttribute[] constraints =
            GetAttributes<TraitPropertyConstraintAttribute>(interfaceProperty, iface, concreteType) ?? [];
        TraitPropertyWarningAttribute[] warnings =
            GetAttributes<TraitPropertyWarningAttribute>(interfaceProperty, iface, concreteType) ?? [];

        if (constraints.Length == 0 && warnings.Length == 0)
            return;

        // Constraints require the operator condition to hold; warnings fire when it holds.
        foreach (TraitPropertyConstraintAttribute constraint in constraints)
        {
            if (OperatorMatches(constraint.Operation, constraint.Values, value, interfaceProperty.Name))
                continue;

            throw BuildConstraintException(constraint.Operation, constraint.Values, value, interfaceProperty.Name);
        }

        foreach (TraitPropertyWarningAttribute warning in warnings)
        {
            if (!OperatorMatches(warning.Operation, warning.Values, value, interfaceProperty.Name))
                continue;

            JsonTextWriter? dummy = null;
            string message = warning.Warning.Replace("{x}", value?.ToString() ?? "null");
            CompilerState.Warn(ref dummy, message);
        }
    }
    
    private static bool OperatorMatches(
        TraitPropertyConstraintAttribute.Constraint op,
        object[] targets,
        object? value,
        string propertyName)
    {
        targets ??= [];

        switch (op)
        {
            case TraitPropertyConstraintAttribute.Constraint.NotEqual:
                foreach (object target in targets)
                {
                    if (ValuesEqual(value, target))
                        return false;
                }
                return true;

            case TraitPropertyConstraintAttribute.Constraint.GreaterThan:
            {
                double valueAsNum = RequireNumber(value, propertyName);
                foreach (object target in targets)
                {
                    double targetAsNum = RequireNumber(target, propertyName, isTarget: true);
                    if (valueAsNum <= targetAsNum)
                        return false;
                }
                return true;
            }

            case TraitPropertyConstraintAttribute.Constraint.LessThan:
            {
                double valueAsNum = RequireNumber(value, propertyName);
                foreach (object target in targets)
                {
                    double targetAsNum = RequireNumber(target, propertyName, isTarget: true);
                    if (valueAsNum >= targetAsNum)
                        return false;
                }
                return true;
            }

            case TraitPropertyConstraintAttribute.Constraint.OneOf:
                foreach (object target in targets)
                {
                    if (ValuesEqual(value, target))
                        return true;
                }
                return false;

            case TraitPropertyConstraintAttribute.Constraint.Type:
            {
                if (value is null)
                    return false;

                foreach (object target in targets)
                {
                    if (target is null)
                        continue;
                    if (ValueMatchesTypeName(value, target.ToString()!))
                        return true;
                }

                return false;
            }

            case TraitPropertyConstraintAttribute.Constraint.Range:
            {
                double valueAsNum = RequireNumber(value, propertyName);
                double min = RequireNumber(targets[0], propertyName, isTarget: true);
                double max = RequireNumber(targets[1], propertyName, isTarget: true);
                
                return (valueAsNum >= min && valueAsNum <= max);
            }

            case TraitPropertyConstraintAttribute.Constraint.GreaterThanEq:
            {
                double valueAsNum = RequireNumber(value, propertyName);
                double targetAsNum = RequireNumber(targets[0], propertyName, isTarget: true);
                return (valueAsNum >= targetAsNum);
            }
            
            case TraitPropertyConstraintAttribute.Constraint.LessThanEq:
            {
                double valueAsNum = RequireNumber(value, propertyName);
                double targetAsNum = RequireNumber(targets[0], propertyName, isTarget: true);
                return (valueAsNum <= targetAsNum);
            }

            default:
                throw new ArgumentException($"{propertyName}: unknown constraint operator {op}");
        }
    }

    private static ArgumentException BuildConstraintException(
        TraitPropertyConstraintAttribute.Constraint op,
        object[] targets,
        object? value,
        string propertyName)
    {
        string runtimeType = value?.GetType().Name ?? "null";
        string message = op switch
        {
            TraitPropertyConstraintAttribute.Constraint.NotEqual =>
                $"{propertyName}: value ({value}) must not equal {string.Join(", ", targets)}",
            TraitPropertyConstraintAttribute.Constraint.GreaterThan =>
                $"{propertyName}: value ({value}) must be greater than {string.Join(", ", targets)}",
            TraitPropertyConstraintAttribute.Constraint.GreaterThanEq =>
                $"{propertyName}: value ({value}) must be greater than or equal to {targets[0]}",
            TraitPropertyConstraintAttribute.Constraint.LessThan =>
                $"{propertyName}: value ({value}) must be less than {string.Join(", ", targets)}",
            TraitPropertyConstraintAttribute.Constraint.LessThanEq =>
                $"{propertyName}: value ({value}) must be less than or equal to {targets[0]}",
            TraitPropertyConstraintAttribute.Constraint.OneOf =>
                $"{propertyName}: value ({value}) must be one of: {string.Join(", ", targets.Select(t => $"'{t}'"))}",
            TraitPropertyConstraintAttribute.Constraint.Range =>
                $"{propertyName}: value ({value}) must be between {targets[0]} and {targets[1]}",
            TraitPropertyConstraintAttribute.Constraint.Type =>
                $"{propertyName}: value type ({runtimeType}) must be one of: {string.Join(", ", targets.Select(t => $"'{t}'"))}",
            _ => $"{propertyName}: constraint {op} failed for value ({value})"
        };
        return new ArgumentException(message);
    }

    /// <summary>
    /// Whether <paramref name="value"/>'s runtime type satisfies a schema/C# type name
    /// (e.g. <c>boolean</c>, <c>string</c>, <c>integer</c>, <c>number</c>).
    /// </summary>
    private static bool ValueMatchesTypeName(object value, string typeName)
    {
        string name = typeName.Trim().ToLowerInvariant();
        Type t = value.GetType();

        return name switch
        {
            "boolean" or "bool" => t == typeof(bool),
            "string" => t == typeof(string),
            "integer" or "int" => IsIntegralType(t),
            // json "number" accepts both floating and integral values
            "number" or "float" or "double" => IsFloatingType(t) || IsIntegralType(t),
            "array" => t.IsArray || (value is System.Collections.IList && value is not string),
            "object" => t.IsClass && t != typeof(string) && !t.IsArray,
            _ => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase)
                 || string.Equals(t.FullName, typeName, StringComparison.OrdinalIgnoreCase)
        };
    }

    private static bool IsIntegralType(Type t) =>
        t == typeof(byte) || t == typeof(sbyte)
        || t == typeof(short) || t == typeof(ushort)
        || t == typeof(int) || t == typeof(uint)
        || t == typeof(long) || t == typeof(ulong);

    private static bool IsFloatingType(Type t) =>
        t == typeof(float) || t == typeof(double) || t == typeof(decimal);

    private static bool ValuesEqual(object? left, object? right)
    {
        if (left is null || right is null)
            return left is null && right is null;

        // Prefer typed equality so boxed numerics and attribute string constants compare correctly.
        if (left is IConvertible && right is IConvertible
            && left is not string && right is not string
            && left is not bool && right is not bool)
        {
            try
            {
                return Convert.ToDouble(left).Equals(Convert.ToDouble(right));
            }
            catch (FormatException) { }
            catch (InvalidCastException) { }
        }

        return left.Equals(right) || string.Equals(left.ToString(), right.ToString(), StringComparison.Ordinal);
    }

    private static double RequireNumber(object? value, string propertyName, bool isTarget = false)
    {
        if (value is null)
            throw new ArgumentException($"{propertyName}: {(isTarget ? "constraint target" : "value")} must be a number");

        if (value is string s)
        {
            if (double.TryParse(s, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double parsed))
                return parsed;
            throw new ArgumentException(
                $"{propertyName}: {(isTarget ? "constraint target" : "value")} '{value}' must be a number");
        }

        try
        {
            return Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
        {
            throw new ArgumentException(
                $"{propertyName}: {(isTarget ? "constraint target" : "value")} '{value}' must be a number", ex);
        }
    }

    private static Version GetContentFormatVersion(object instance) => instance switch
    {
        Item item => item.FormatVersion,
        Block block => block.FormatVersion,
        Entity entity => entity.FormatVersion,
        BlockPermutation permutation => permutation.Parent.FormatVersion,
        _ => throw new ArgumentException(
            $"FormatVersion checks are not supported for content type {instance.GetType().Name} (expected Item, Block, Entity, or BlockPermutation)")
    };
    
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