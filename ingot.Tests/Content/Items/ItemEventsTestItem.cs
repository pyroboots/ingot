using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.Resource;

namespace ingot.Tests.Content.Items;

internal class ItemEventsTestItem : Item
{
    public override Identifier Identifier => new("test:events_item");
    public override string Texture =>
        new TextureReference<ItemEventsTestItem>(FixturePaths.Resolve("test_item.png"), "events_item");

    public override ItemEvents? ItemEvents => new()
    {
        UseEvent = "event.source.sendMessage('used item');"
    };
}