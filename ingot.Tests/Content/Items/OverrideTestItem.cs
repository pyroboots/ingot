using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;

namespace ingot.Tests.Content.Items;

internal class OverrideTestItem : Item
{
    public override Identifier Identifier => new("test:override_item");
    public override string Texture => "override_item";
    public override string? TexturePath => FixturePaths.Resolve("auto.png");
}