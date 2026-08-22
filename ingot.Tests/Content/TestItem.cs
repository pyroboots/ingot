using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.Resource;

namespace ingot.Tests.Content;

internal class TestItem : Item
{
    public override Identifier Identifier => new("test:test_item");
    public override string Texture =>
        new TextureReference<TestItem>(FixturePaths.Resolve("test_item.png"), "test_item");
}