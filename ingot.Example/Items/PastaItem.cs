using ingot.Core.Behaviour;
using ingot.Core.Common;

namespace ingot.Example.Items;

public class PastaItem : Item
{
    public override Identifier Identifier => new("test:pasta");
    public override string Texture => "pasta";

    public override string DisplayName => "Pasta";
}