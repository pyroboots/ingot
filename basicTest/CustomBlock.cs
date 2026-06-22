using ingot.Core.Behaviour.Block;
using ingot.Core.Common;

namespace basicTest;

public class CustomBlock : Block
{
    public override Identifier Identifier => new("test", "custom_block");
    public override MaterialInstances MaterialInstances => new() { All = new("dirt") };

    public override string? DisplayName => "Custom Block";
}