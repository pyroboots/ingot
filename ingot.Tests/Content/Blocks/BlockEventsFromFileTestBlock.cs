using ingot.Core.Behaviour.Block;
using ingot.Core.Common;
using ingot.Core.Scripting;
using ingot.Tests.Support;

namespace ingot.Tests.Content.Blocks;

internal class BlockEventsFromFileTestBlock : Block
{
    public override Identifier Identifier => new("test:events_from_file_block");

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance("events_from_file_block", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
    };

    public override BlockEvents? BlockEvents => new()
    {
        OnPlaceEvent = ScriptHandler.FromFile(FixturePaths.Resolve("scripts/on_place.js")),
    };
}