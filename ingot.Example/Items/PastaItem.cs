using ingot.Core.Behaviour;
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.Resource;
using ingot.Core.Resource.Referencers;

namespace ingot.Example.Items;

public class PastaItem : Item
{
    public override Identifier Identifier => new("test:pasta");
    public override string Texture => new TextureReference<PastaItem>(Path.Combine(AppContext.BaseDirectory, "Data", "pasta.png"));

    public override string DisplayName => "Pasta";
}