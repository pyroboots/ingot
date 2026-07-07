using ingot.Core.Behaviour.Loot;
using ingot.Core.Common;

using Newtonsoft.Json;

namespace ingot.Tests.Loot;

public class RandomBlockStateJsonContainsBlockStateKeyTest
{
    [Fact]
    public void Compile_RandomBlockStateJsonContainsBlockStateKey()
    {
        RandomBlockState function = new()
        {
            Values = new IntRange(0, 1),
            BlockState = "test:variant",
        };

        using StringWriter writer = new();
        JsonTextWriter jsonWriter = new(writer);
        function.Compile(ref jsonWriter);

        Assert.Contains("\"block_state\"", writer.ToString());
        Assert.DoesNotContain("\"blocK_state\"", writer.ToString());
    }
}