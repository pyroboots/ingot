using ingot.Common;
using Newtonsoft.Json;

namespace ingot.Types;

public class FilterGroup : ICompileableFragment
{
    public Filter[] AllOf = [];
    public Filter[] AnyOf = [];
    
    public void Compile(ref JsonTextWriter writer)
    {
        writer.WriteStartObject();
            writer.WritePropertyName("all_of");
            writer.WriteStartArray();
                foreach (var filter in AllOf)
                    filter.Compile(ref writer);
            writer.WriteEndArray();
            writer.WritePropertyName("any_of");
            writer.WriteStartArray();
                foreach (var filter in AnyOf)
                    filter.Compile(ref writer);
            writer.WriteEndArray();
        writer.WriteEndObject();
    }
}