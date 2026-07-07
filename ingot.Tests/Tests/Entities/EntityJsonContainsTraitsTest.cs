using ingot.Core.Behaviour.Entity;
using ingot.Tests.Content.Entities;

namespace ingot.Tests.Entities;

public class EntityJsonContainsTraitsTest
{
    [Fact]
    public void Compile_EntityJsonContainsHealthTrait()
    {
        string json = Entity.Compile(typeof(TraitTestEntity));

        Assert.Contains("minecraft:health", json);
        Assert.Contains("\"max\": 20", json);
    }

    [Fact]
    public void Compile_EntityJsonContainsTypeFamilyTrait()
    {
        string json = Entity.Compile(typeof(TraitTestEntity));

        Assert.Contains("minecraft:type_family", json);
        Assert.Contains("\"family\": \"test\"", json);
    }

    [Fact]
    public void Compile_EntityComponentGroupCompilesTraits()
    {
        string json = Entity.Compile(typeof(TraitComponentGroupTestEntity));

        Assert.Contains("\"test:trait_group\"", json);
        Assert.Contains("minecraft:health", json);
        Assert.Contains("\"max\": 10", json);
    }

    [Fact]
    public void Compile_EntityJsonContainsBasicPresetTraits()
    {
        string json = Entity.Compile(typeof(PresetTestEntity));

        Assert.Contains("minecraft:health", json);
        Assert.Contains("minecraft:type_family", json);
        Assert.Contains("minecraft:pushable", json);
        Assert.Contains("minecraft:physics", json);
        Assert.Contains("minecraft:collision_box", json);
        Assert.Contains("minecraft:despawn", json);
    }
}