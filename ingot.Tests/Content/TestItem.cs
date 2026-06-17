using ingot.Core.Behaviour;
using ingot.Core.Common;

namespace ingot.Tests.Content;

internal class TestItem : Item
{
    public override Identifier Identifier => new("test:test_item");
    public override string Texture => "test_item";
    public override string? TexturePath => FixturePaths.Resolve("test_item.png");
}