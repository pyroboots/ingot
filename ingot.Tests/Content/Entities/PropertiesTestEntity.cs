using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;

namespace ingot.Tests.Content.Entities;

internal class PropertiesTestEntity : Entity
{
    public override Identifier Identifier => new("test:properties_entity");

    public override Dictionary<Identifier, IEntityProperty> Properties => new()
    {
        [new("test:is_charged")] = new BooleanEntityProperty
        {
            Default = false,
        },
        [new("test:mood")] = new EnumEntityProperty
        {
            Values = ["calm", "alert", "angry"],
            Default = "calm",
        },
        [new("test:power")] = new FloatEntityProperty
        {
            Min = 0f,
            Max = 1f,
            Default = 0.5f,
        },
        [new("test:level")] = new IntEntityProperty
        {
            Min = 0,
            Max = 10,
            Default = 1,
            ClientSync = false,
        },
    };
}

internal class InvalidEnumDefaultPropertyEntity : Entity
{
    public override Identifier Identifier => new("test:invalid_enum_property_entity");

    public override Dictionary<Identifier, IEntityProperty> Properties => new()
    {
        [new("test:mood")] = new EnumEntityProperty
        {
            Values = ["calm", "alert"],
            Default = "angry",
        },
    };
}

internal class OutOfRangeFloatPropertyEntity : Entity
{
    public override Identifier Identifier => new("test:out_of_range_float_entity");

    public override Dictionary<Identifier, IEntityProperty> Properties => new()
    {
        [new("test:power")] = new FloatEntityProperty
        {
            Min = 0f,
            Max = 1f,
            Default = 2f,
        },
    };
}

internal class OutOfRangeIntPropertyEntity : Entity
{
    public override Identifier Identifier => new("test:out_of_range_int_entity");

    public override Dictionary<Identifier, IEntityProperty> Properties => new()
    {
        [new("test:level")] = new IntEntityProperty
        {
            Min = 0,
            Max = 10,
            Default = -1,
        },
    };
}
