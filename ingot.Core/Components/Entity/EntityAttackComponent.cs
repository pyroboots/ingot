using ingot.Core.Common;
using Newtonsoft.Json;
using static ingot.Core.JsonHelper;
using Version = ingot.Core.Common.Version;

namespace ingot.Core.Components.Entity;

public class EntityAttackComponent : IComponent<Content.Entity>
{
    public required double[] Damage;
    public int EffectAmplifier = 0;
    public int EffectDuration = 0;
    public string? EffectName;
    
    public void Compile(ref JsonTextWriter writer)
    {
        Object(ref writer, Identifier.ToString(), writer =>
        {
            Property(ref writer, "damage", Damage);

            if (EffectName is not null)
            {
                Property(ref writer, "effect_name", EffectName);
                Property(ref writer, "effect_amplifier", EffectAmplifier);
                if (EffectDuration == 0)
                    CompileTimeLogging.Warn(ref writer, "effect duration is 0");
                Property(ref writer, "effect_duration", EffectDuration);
            }
        });
    }
    
    public Identifier Identifier => new("minecraft:attack");
    public Version MinimumFormatVersion => new(0, 0, 0);
}