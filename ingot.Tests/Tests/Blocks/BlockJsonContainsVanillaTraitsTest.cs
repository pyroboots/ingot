using System.Text.Json;

using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Block.BlockTraits;
using ingot.Core.Common;
using ingot.Core.Common.SharedConstructs;
using ingot.Tests.Content;
using ingot.Tests.Content.Blocks;

using Newtonsoft.Json;

using Version = ingot.Core.Common.Version;

namespace ingot.Tests.Blocks;

public class BlockJsonContainsVanillaTraitsTest
{
    [Fact]
    public void Compile_WrapsTraitsUnderDescriptionTraitsObject()
    {
        string json = Block.Compile(typeof(VanillaTraitsTestBlock));
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement traits = doc.RootElement
            .GetProperty("minecraft:block")
            .GetProperty("description")
            .GetProperty("traits");

        Assert.True(traits.TryGetProperty("minecraft:placement_direction", out _));
        Assert.True(traits.TryGetProperty("minecraft:placement_position", out _));
        Assert.True(traits.TryGetProperty("minecraft:connection", out _));
    }

    [Fact]
    public void Compile_EnabledStatesAreStringIdentifiers()
    {
        string json = Block.Compile(typeof(VanillaTraitsTestBlock));
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement traits = doc.RootElement
            .GetProperty("minecraft:block")
            .GetProperty("description")
            .GetProperty("traits");

        JsonElement placementDirection = traits.GetProperty("minecraft:placement_direction");
        JsonElement enabled = placementDirection.GetProperty("enabled_states");
        Assert.Equal(JsonValueKind.Array, enabled.ValueKind);
        Assert.Equal(1, enabled.GetArrayLength());
        Assert.Equal(JsonValueKind.String, enabled[0].ValueKind);
        Assert.Equal("minecraft:facing_direction", enabled[0].GetString());
        Assert.Equal(180, placementDirection.GetProperty("y_rotation_offset").GetInt32());

        JsonElement placementPosition = traits.GetProperty("minecraft:placement_position");
        JsonElement posEnabled = placementPosition.GetProperty("enabled_states");
        Assert.Equal(1, posEnabled.GetArrayLength());
        Assert.Equal("minecraft:vertical_half", posEnabled[0].GetString());
    }

    [Fact]
    public void Compile_MultiBlockWritesPartsAndDirection()
    {
        string json = Block.Compile(typeof(MultiBlockVanillaTraitsTestBlock));
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement multi = doc.RootElement
            .GetProperty("minecraft:block")
            .GetProperty("description")
            .GetProperty("traits")
            .GetProperty("minecraft:multi_block");

        Assert.Equal(3, multi.GetProperty("parts").GetInt32());
        Assert.Equal("up", multi.GetProperty("direction").GetString());
        Assert.Equal("minecraft:multi_block_part", multi.GetProperty("enabled_states")[0].GetString());
    }

    [Fact]
    public void Compile_ThrowsWhenFormatVersionTooLow()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => Block.Compile(typeof(StaleFormatVanillaTraitsTestBlock)));
        Assert.Contains("1.26.0", ex.Message);
        Assert.Contains("1.21.90", ex.Message);
    }

    [Fact]
    public void Compile_PlacementDirectionOmitsDefaultOptionalFields()
    {
        var block = new VanillaTraitsOptionalFieldsBlock();
        string json = Block.CompileFromInstance(block);
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement placement = doc.RootElement
            .GetProperty("minecraft:block")
            .GetProperty("description")
            .GetProperty("traits")
            .GetProperty("minecraft:placement_direction");

        Assert.False(placement.TryGetProperty("y_rotation_offset", out _));
        Assert.False(placement.TryGetProperty("blocks_to_corner_with", out _));
    }

    [Fact]
    public void Compile_BlocksToCornerWithSerializesAsIdentifierStrings()
    {
        var block = new VanillaTraitsCornerBlock();
        string json = Block.CompileFromInstance(block);
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement corners = doc.RootElement
            .GetProperty("minecraft:block")
            .GetProperty("description")
            .GetProperty("traits")
            .GetProperty("minecraft:placement_direction")
            .GetProperty("blocks_to_corner_with");

        Assert.Equal(JsonValueKind.Array, corners.ValueKind);
        Assert.Equal(1, corners.GetArrayLength());
        Assert.Equal(JsonValueKind.String, corners[0].ValueKind);
        Assert.Equal("minecraft:oak_stairs", corners[0].GetString());
    }

    [Fact]
    public void MultiBlock_RejectsInvalidDirection()
    {
        var trait = new MultiBlockVanillaBlockTrait { Direction = "north", Parts = 2 };
        JsonTextWriterSink.AssertCompileThrows(trait, typeof(ArgumentException));
    }

    [Fact]
    public void MultiBlock_ProvidedStatesAreZeroBasedPartIndices()
    {
        var trait = new MultiBlockVanillaBlockTrait { Direction = "up", Parts = 3 };
        object[] values = trait.ProvidedStates[0].Values;
        Assert.Equal(new object[] { 0, 1, 2 }, values);
    }

    [Fact]
    public void PlacementDirection_SixteenWayHasSixteenValues()
    {
        var trait = new PlacementDirectionVanillaBlockTrait();
        ProvidedState sixteen = trait.ProvidedStates.Single(s => s.State.ToString() == "minecraft:sixteen_way_rotation");
        Assert.Equal(16, sixteen.Values.Length);
        Assert.Equal(0, sixteen.Values[0]);
        Assert.Equal(15, sixteen.Values[15]);
    }

    [Fact]
    public void ProvidedState_RejectsInvalidValueTypes()
    {
        Assert.Throws<ArgumentException>(() =>
            new ProvidedState("minecraft:bad", [1.5d]));
    }

    private sealed class VanillaTraitsOptionalFieldsBlock : Block
    {
        public override Identifier Identifier => new("test:optional_fields_traits");
        public override Version FormatVersion => new("1.26.0");
        public override IVanillaBlockTrait[] BlockTraits =>
        [
            new PlacementDirectionVanillaBlockTrait(),
        ];
        public override MaterialInstances MaterialInstances => new()
        {
            All = new MaterialInstance("x", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
        };
    }

    private sealed class VanillaTraitsCornerBlock : Block
    {
        public override Identifier Identifier => new("test:corner_traits");
        public override Version FormatVersion => new("1.26.0");
        public override IVanillaBlockTrait[] BlockTraits =>
        [
            new PlacementDirectionVanillaBlockTrait
            {
                EnabledStates = [new("minecraft:corner_and_cardinal_direction")],
                BlocksToCornerWith = [new BlockTypeDescriptor { Name = "minecraft:oak_stairs" }],
            },
        ];
        public override MaterialInstances MaterialInstances => new()
        {
            All = new MaterialInstance("x", MaterialInstance.RenderMethods.Opaque, FixturePaths.Resolve("test_block.png"))
        };
    }
}

/// <summary>Minimal helper to invoke Compile without packing a full block.</summary>
file static class JsonTextWriterSink
{
    public static void AssertCompileThrows(IVanillaBlockTrait trait, Type exceptionType)
    {
        using StringWriter sw = new();
        JsonWriter writer = new JsonTextWriter(sw);
        Exception ex = Assert.Throws(exceptionType, () => trait.Compile(ref writer));
        Assert.NotNull(ex);
    }
}
