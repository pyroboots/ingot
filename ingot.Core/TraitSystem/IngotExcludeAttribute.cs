namespace ingot.Core.TraitSystem;

/// <summary>
/// Marks a trait property to be omitted when compiling JSON.
/// Apply on the trait interface (always skip) or on a content class implementation
/// (skip only for that type).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class IngotExcludeAttribute : Attribute
{
}
