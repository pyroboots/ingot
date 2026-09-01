using Newtonsoft.Json;

namespace ingot.Core.Common
{
    /// <summary>
    /// Represents a random value between float a and float b
    /// </summary>
    public class FloatRange : ICompilableFragment
    {
        /// <summary>
        /// Minimum value
        /// </summary>
        public required float RangeMin;
        /// <summary>
        /// Maximum value
        /// </summary>
        public required float RangeMax;
    
        /// <inheritdoc/>
        public void Compile(ref JsonWriter writer)
        {
            JsonHelper json = new(ref writer);
            json.Object("", () =>
            {
                json.Property(Formatting.PascalToSnakeCase(nameof(RangeMin)), RangeMin);
                json.Property(Formatting.PascalToSnakeCase(nameof(RangeMax)), RangeMax);
            });
        }
    }
}