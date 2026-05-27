using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.JsonHelper;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Components.Entity;

public class EntityNavigationGenericComponent : IComponent<Content.Entity>
{
    public bool AvoidDamageBlocks = false;
    public bool AvoidPortals = false;
    public bool AvoidSun = false;
    public bool AvoidWater = false;
    public string[] BlocksToAvoid = [];
    public bool CanBreach = false;
    public bool CanBreakDoors = false;
    public bool CanJump = true;
    public bool CanOpenDoors = false;
    public bool CanOpenIronDoors = false;
    public bool CanPassDoors = true;
    public bool CanPathFromAir = false;
    public bool CanPathOverLava = false;
    public bool CanPathOverWater = false;
    public bool CanSink = true;
    public bool CanSwim = false;
    public bool CanWalk = true;
    public bool CanWalkInLava = false;
    public bool IsAmphibious = false;
    public string? UsingDoorAnnotation;

    public void Compile(ref JsonTextWriter writer)
    {
        Object(ref writer, Identifier.ToString(), writer =>
        {
            Property(ref writer, "avoid_damage_blocks", AvoidDamageBlocks);
            Property(ref writer, "avoid_portals", AvoidPortals);
            Property(ref writer, "avoid_sun", AvoidSun);
            Property(ref writer, "avoid_water", AvoidWater);
            Property(ref writer, "blocks_to_avoid", BlocksToAvoid);
            Property(ref writer, "can_breach", CanBreach);
            Property(ref writer, "can_break_doors", CanBreakDoors);
            Property(ref writer, "can_jump", CanJump);
            Property(ref writer, "can_open_doors", CanOpenDoors);
            Property(ref writer, "can_open_iron_doors", CanOpenIronDoors);
            Property(ref writer, "can_pass_doors", CanPassDoors);
            Property(ref writer, "can_path_from_air", CanPathFromAir);
            Property(ref writer, "can_path_over_lava", CanPathOverLava);
            Property(ref writer, "can_path_over_water", CanPathOverWater);
            Property(ref writer, "can_sink", CanSink);
            Property(ref writer, "can_swim", CanSwim);
            Property(ref writer, "can_walk", CanWalk);
            Property(ref writer, "can_walk_in_lava", CanWalkInLava);
            Property(ref writer, "is_amphibious", IsAmphibious);
            Property(ref writer, "using_door_annotation", UsingDoorAnnotation);
        });
    }
    
    public Identifier Identifier => new("minecraft:navigation.generic");
    public Version MinimumFormatVersion => new(0, 0, 0);
}