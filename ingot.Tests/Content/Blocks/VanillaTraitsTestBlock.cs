using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Block.BlockTraits;
using ingot.Core.Common;

using Version = ingot.Core.Common.Version;

namespace ingot.Tests.Content.Blocks;

/// <summary>
/// Exercise several vanilla description traits with explicit enabled_states.
/// </summary>
internal class VanillaTraitsTestBlock : Block
{
    public override Identifier Identifier => new("test:vanilla_traits_block");

    public override Version FormatVersion => new("1.26.0");

    public override IVanillaBlockTrait[] BlockTraits =>
    [
        new PlacementDirectionVanillaBlockTrait
        {
            EnabledStates = [new("minecraft:facing_direction")],
            YRotationOffset = 180,
        },
        new PlacementPositionVanillaBlockTrait
        {
            EnabledStates = [new("minecraft:vertical_half")],
        },
        new ConnectionVanillaBlockTrait(),
    ];

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("vanilla_traits_block", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
    };
}

/// <summary>
/// Multi-block only; also used for format-version failure tests via instance compile.
/// </summary>
internal class MultiBlockVanillaTraitsTestBlock : Block
{
    public override Identifier Identifier => new("test:multi_block_traits");

    public override Version FormatVersion => new("1.26.0");

    public override IVanillaBlockTrait[] BlockTraits =>
    [
        new MultiBlockVanillaBlockTrait
        {
            Direction = "up",
            Parts = 3,
        },
    ];

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("multi_block_traits", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
    };
}

/// <summary>
/// Uses a trait that requires 1.26.0 while declaring an older format version.
/// </summary>
internal class StaleFormatVanillaTraitsTestBlock : Block
{
    public override Identifier Identifier => new("test:stale_format_traits");

    public override Version FormatVersion => new("1.21.90");

    public override IVanillaBlockTrait[] BlockTraits =>
    [
        new ConnectionVanillaBlockTrait(),
    ];

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("stale_format_traits", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
    };
}
