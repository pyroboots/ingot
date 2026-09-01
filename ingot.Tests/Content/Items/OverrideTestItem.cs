using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.Resource;
using ingot.Core.Resource.Referencers;

namespace ingot.Tests.Content.Items;

internal class OverrideTestItem : Item
{
    public override Identifier Identifier => new("test:override_item");
    public override string Texture =>
        new TextureReference<OverrideTestItem>(FixturePaths.Resolve("auto.png"), "override_item");
}