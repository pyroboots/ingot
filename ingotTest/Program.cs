using ingot;
using ingot.Components;
using ingot.Components.Entity;
using ingot.Content;
using ingot.Types;

namespace ingotTest;

class Program
{
    static void Main(string[] args)
    {
        Entity cow = new("minecraft:cow");
        
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
        
        Console.WriteLine(cow.Compile());
    }
}