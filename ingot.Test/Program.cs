using ingot.Core;
using ingot.Core.Components;
using ingot.Core.Components.Entity;
using ingot.Core.Content;
using ingot.Core.TraitSystem;
using ingot.Core.Types;
using Item = ingot.Core.TraitSystem.Item;

namespace ingot.Test;

class Program
{
    static void Main(string[] args)
    {
        /*Entity cow = new("minecraft:cow");
        
        ComponentGroup babyGroup = cow.AddComponentGroup(new("minecraft:cow_baby"));
        babyGroup.AddComponent(new EntityIsBabyComponent());
        babyGroup.AddComponent(new EntityScaleComponent { Scale = 0.5 });
        
        ComponentGroup adultGroup = cow.AddComponentGroup(new("minecraft:cow_adult"));
        adultGroup.AddComponent(new EntityScaleComponent { Scale = 0 });
        
        cow.AddComponent(new EntityCollisionBoxComponent
        {
            Height = -1,
            Width = -1
        });
        cow.AddComponent(new EntityHealthComponent
        {
            Max = 0,
            Value = 0
        });
        cow.AddComponent(new DynamicComponent(new("minecraft:ageable"), new()
        {
            ["duration"] = 1200,
            ["feed_items"] = new[] { "wheat" }
        }));
        cow.AddComponent(new EntityAttackComponent
        {
            Damage = [1],
            EffectName = "poison",
            EffectAmplifier = 2,
            EffectDuration = 5
        });
        
        Console.WriteLine(cow.Compile());*/
        
        Item.Compile<Lasagna>();
    }
}