using System.ComponentModel;

using ingot.Core.Common;

using Newtonsoft.Json;

using Formatting = ingot.Core.Common.Formatting;

namespace ingot.Core.Behaviour.Entity;

/// <summary>
/// Allow data to be stored on entities without needing the use of components in the server side of the entity, similar to block states
/// </summary>
public interface IEntityProperty : ICompilableFragment
{
    /// <summary>
    /// Type of entity property
    /// </summary>
    public string Type { get; }
    
    /// <summary>
    /// Whether this property is accessbile from the client
    /// </summary>
    public bool ClientSync { get; set; }
}

/// <summary>
/// Entity property that can store a boolean state: true or false
/// </summary>
public class BooleanEntityProperty : IEntityProperty
{
    /// <inheritdoc/>
    public string Type => "bool";

    /// <summary>
    /// Default value of the property
    /// </summary>
    public required bool Default;

    /// <inheritdoc/>
    public bool ClientSync { get; set; } = true;

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Property(Formatting.PascalToSnakeCase(nameof(Type)), Type);
            json.Property(Formatting.PascalToSnakeCase(nameof(Default)), Default);
            json.Property(Formatting.PascalToSnakeCase(nameof(ClientSync)), ClientSync);
        });
    }
}

/// <summary>
/// Entity property that can store an enumeration of possible string values
/// </summary>
public class EnumEntityProperty : IEntityProperty
{
    /// <inheritdoc/>
    public string Type => "enum";

    /// <summary>
    /// Possible enumeration values
    /// </summary>
    public required string[] Values;

    /// <summary>
    /// Default value of the property
    /// </summary>
    public required string Default;
    
    /// <inheritdoc/>
    public bool ClientSync { get; set; } = true;

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        if (Values.Contains(Default) == false)
            throw new InvalidEnumArgumentException($"value {Default} is not present in enum ({string.Join(',', Values)})");
        
        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Property(Formatting.PascalToSnakeCase(nameof(Type)), Type);
            json.Property(Formatting.PascalToSnakeCase(nameof(Values)), Values);
            json.Property(Formatting.PascalToSnakeCase(nameof(Default)), Default);
            json.Property(Formatting.PascalToSnakeCase(nameof(ClientSync)), ClientSync);
        });
    }
}

/// <summary>
/// Entity property that can store a float in a range
/// </summary>
public class FloatEntityProperty : IEntityProperty
{
    /// <inheritdoc/>
    public string Type => "float";

    /// <summary>
    /// Minimum value of the property
    /// </summary>
    public required float Min;

    /// <summary>
    /// Maximum value of the property
    /// </summary>
    public required float Max;
    
    /// <summary>
    /// Default value of the property
    /// </summary>
    public required float Default;
    
    /// <inheritdoc/>
    public bool ClientSync { get; set; } = true;

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        if (Default > Max || Default < Min)
            throw new ArgumentOutOfRangeException($"value {Default} must between the range {Max} and {Min}");
        
        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Property(Formatting.PascalToSnakeCase(nameof(Type)), Type);
            json.Property("range", new[] {Min, Max});
            json.Property(Formatting.PascalToSnakeCase(nameof(Default)), Default);
            json.Property(Formatting.PascalToSnakeCase(nameof(ClientSync)), ClientSync);
        });
    }
}

/// <summary>
/// Entity property that can store an integer in a range
/// </summary>
public class IntEntityProperty : IEntityProperty
{
    /// <inheritdoc/>
    public string Type => "int";

    /// <summary>
    /// Minimum value of the property
    /// </summary>
    public required int Min;

    /// <summary>
    /// Maximum value of the property
    /// </summary>
    public required int Max;
    
    /// <summary>
    /// Default value of the property
    /// </summary>
    public required int Default;
    
    /// <inheritdoc/>
    public bool ClientSync { get; set; } = true;

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        if (Default > Max || Default < Min)
            throw new ArgumentOutOfRangeException($"value {Default} must between the range {Max} and {Min}");
        
        JsonHelper json = new(ref writer);
        json.Object("", () =>
        {
            json.Property(Formatting.PascalToSnakeCase(nameof(Type)), Type);
            json.Property("range", new[] {Min, Max});
            json.Property(Formatting.PascalToSnakeCase(nameof(Default)), Default);
            json.Property(Formatting.PascalToSnakeCase(nameof(ClientSync)), ClientSync);
        });
    }
}