using ingot.Core.Behaviour;
using ingot.Core.Common;

namespace ingot.Tests.Content.Items;

internal class EquipmentTestItem : Item
{
    public override Identifier Identifier => new("test:equipment_item");
    public override string Texture => "equipment_item";
    public override string? TexturePath => FixturePaths.Resolve("test_item.png");
    public override string DisplayName => "Equipment Item";
    public override CatalogueCategory Category => CatalogueCategory.Equipment;
    public override string? Group => "itemGroup.name.sword";
    public override int MaxStackSize => 1;
}