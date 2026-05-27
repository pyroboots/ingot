using ingot.Core.Common;
using Newtonsoft.Json;
using Version = ingot.Core.Common.Version;
using static ingot.Core.JsonHelper;

namespace ingot.Core.Components.Item;

public class ItemBlockPlacerComponent : IComponent<Content.Item>
{
    public bool AlignedPlacement = false;
    public required Identifier Block;
    public bool ReplaceBlockItem = false;
    public string[] UseOn = [];
    
    public void Compile(ref JsonTextWriter writer)
    {
        Object(ref writer, Identifier.ToString(), w =>
        {
            Property(ref w, "aligned_placement", AlignedPlacement);
            Property(ref w, "block", Block.ToString());
            if (ReplaceBlockItem)
                CompileTimeLogging.Warn(ref w,$"item identifier must be the same as block identifier ({Block}) for replace_block_item to be valid");
            Property(ref w, "replace_block_item", ReplaceBlockItem);
            Property(ref w, "use_on", UseOn);
        });
    }

    public Identifier Identifier => new("minecraft:block_placer");
    public Version MinimumFormatVersion => new(1, 21, 50);
}