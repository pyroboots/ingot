using ingot.Common;
using Newtonsoft.Json;
using static ingot.JsonHelper;

namespace ingot.Types;

public class Filter : ICompileableFragment
{
    public required string Test;
    public required string Subject;
    public required string Value;

    public Filter(string test, string subject, string value)
    {
        Test = test;
        Subject = subject;
        Value = value;
    }
    
    public void Compile(ref JsonTextWriter writer)
    {
        writer.WriteStartObject();
            Property(ref writer, "test", Test);
            Property(ref writer, "subject", Subject);
            Property(ref writer, "value", Value);
        writer.WriteEndObject();
    }
}