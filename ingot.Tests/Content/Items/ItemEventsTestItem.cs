using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;

namespace ingot.Tests.Content.Items;

internal class ItemEventsTestItem : Item
{
    public override Identifier Identifier => new("test:events_item");
    public override string Texture => "events_item";
    public override string? TexturePath => FixturePaths.Resolve("test_item.png");

    public override ItemEvents? ItemEvents => new()
    {
        UseEvent = "event.source.sendMessage('used item');"
    };
}