using Newtonsoft.Json;

namespace ingot.Core.Common;

/// <summary>
/// Internal use interface make a class compilable to JSON
/// </summary>
public interface ICompileableFragment
{
    /// <summary>
    /// Compiles this class to JSON
    /// </summary>
    /// <param name="writer">JSON source stream to write to</param>
    public void Compile(ref JsonTextWriter writer);
}