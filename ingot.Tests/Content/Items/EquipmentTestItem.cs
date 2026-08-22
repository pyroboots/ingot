using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.Resource;

namespace ingot.Tests.Content.Items;

internal class EquipmentTestItem : Item
{
    public override Identifier Identifier => new("test:equipment_item");
    public override string Texture =>
        new TextureReference<EquipmentTestItem>(FixturePaths.Resolve("test_item.png"), "equipment_item");
    public override string DisplayName => "Equipment Item";
    public override Enums.CatalogueCategory Category => Enums.CatalogueCategory.Equipment;
    public override string? Group => "itemGroup.name.sword";
    public override int MaxStackSize => 1;
}