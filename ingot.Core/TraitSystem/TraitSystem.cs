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
    public static List<Trait> GetTraits<T>(TraitType constraint) => GetTraits(typeof(T), constraint);
    /// <summary>
    /// Gets all traits of type <paramref name="t"/>
    /// </summary>
    /// <param name="t"></param>
    /// <param name="constraint"><see cref="Type"/> of content class to reflect</param>
    public static List<Trait> GetTraits(Type t, TraitType constraint)
    {
        JsonTextWriter? dummyWriter = null;
        object instance = Activator.CreateInstance(t)!;
        List<Trait> traits = new();

        foreach (Type iface in t.GetInterfaces())
        {
            TraitAttribute? traitAttr = iface.GetCustomAttribute<TraitAttribute>();
            if (traitAttr == null)
                continue;

            CompilerState.Push(traitAttr.Identifier.ToString());
            if (traitAttr.Constraint != constraint)
            {
                CompilerState.Warn(ref dummyWriter,
                    $"mismatching trait types (expected: {nameof(TraitType)}.{constraint}, got: {nameof(TraitType)}.{traitAttr.Constraint}), omitting from compiled json");
                CompilerState.Pop();
                continue;
            }

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
        {
            string err = $"type {interfaceType.Name} is not a trait";
            CompilerState.Warn(ref dummyWriter, err);
            CompilerState.Pop();
            throw new ArgumentException(err);
        }

        if (traitAttr.Constraint != constraint)
        {
            string err =
                $"mismatching trait types (expected: {nameof(TraitType)}.{constraint}, got: {nameof(TraitType)}.{traitAttr.Constraint})";
            CompilerState.Warn(ref dummyWriter, err);
            CompilerState.Pop();
            throw new ArgumentException(err);
        }

        if (!interfaceType.IsAssignableFrom(objectType))
        {
            string err = $"{objectType.Name} does not implement trait interface {interfaceType.Name}";
            CompilerState.Warn(ref dummyWriter, err);
            CompilerState.Pop();
            throw new ArgumentException(err);
        }

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

        foreach (PropertyInfo property in iface.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            CompilerState.Push(property.Name);

            TraitPropertyAttribute? propertyAttr = property.GetCustomAttribute<TraitPropertyAttribute>();
            if (propertyAttr == null)
            {
                CompilerState.Pop();
                continue;
            }

            MethodInfo? getter = property.GetGetMethod();
            if (getter == null)
            {
                CompilerState.Warn(ref dummyWriter, $"property {property.Name} has no getter, omitting");
                CompilerState.Pop();
                continue;
            }

            object? value = null;
            try
            {
                value = getter.Invoke(instance, null);
            }
            catch (Exception ex)
            {
                CompilerState.Warn(ref dummyWriter, $"failed to get value for property {property.Name}: {ex.Message}");
            }

            if (value == null || (value is string str && string.IsNullOrEmpty(str)))
                CompilerState.Warn(ref dummyWriter, "value is null or empty, entry omitted from compiled json");

            CompilerState.Pop();

            properties.Add(new TraitProperty(
                path: propertyAttr.Path,
                name: property.Name,
                value: value!
            ));
        }

        return properties;
    }
}