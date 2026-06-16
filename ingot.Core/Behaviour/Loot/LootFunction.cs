using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.Common.JsonHelper;

namespace ingot.Core.Behaviour.Loot;

/// <summary>
/// A loot table function that modifies a dropped item
/// </summary>
public abstract class LootFunction : ICompileableFragment
{
    /// <summary>
    /// Bedrock function name (e.g. <c>set_count</c>)
    /// </summary>
    public abstract string FunctionName { get; }

    /// <summary>
    /// Writes function-specific parameters
    /// </summary>
    protected abstract void CompileParameters(ref JsonTextWriter writer);

    /// <inheritdoc/>
    public void Compile(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);

        writer.WriteStartObject();
        json.Property("function", FunctionName);
        CompileParameters(ref writer);
        writer.WriteEndObject();
    }
}

/// <summary>
/// Sets the quantity of items returned
/// </summary>
public class SetCountFunction : LootFunction
{
    /// <summary>
    /// Item count to return
    /// </summary>
    public required IntRange Count { get; init; }

    /// <inheritdoc/>
    public override string FunctionName => "set_count";

    /// <inheritdoc/>
    protected override void CompileParameters(ref JsonTextWriter writer)
    {
        JsonHelper json = new(ref writer);
        json.Property("count", Count);
    }
}