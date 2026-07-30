using ingot.Core;
using ingot.Core.Behaviour.Block;
using ingot.Core.Behaviour.Item;
using ingot.Core.Common;
using ingot.Core.TraitSystem;
using ingot.Core.TraitSystem.Traits;
using ingot.Core.TraitSystem.Traits.Item;
using Version = ingot.Core.Common.Version;
using TraitAttr = ingot.Core.TraitSystem.TraitAttribute;

namespace ingot.Tests.Traits;

public class ValueConstraintAndWarningTest
{
    [Fact]
    public void OneOf_ValidAnimation_ReflectsSuccessfully()
    {
        List<Trait> traits = TraitSystem.GetTraits(new EatAnimationItem(), TraitSystem.TraitType.Item);
        Trait useAnim = Assert.Single(traits, t => t.Identifier.ToString() == "minecraft:use_animation");
        TraitProperty value = Assert.Single(useAnim.Properties, p => p.Name == "Value");
        Assert.Equal("eat", value.Value);
    }

    [Fact]
    public void OneOf_InvalidAnimation_Throws()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            TraitSystem.GetTraits(new InvalidAnimationItem(), TraitSystem.TraitType.Item));

        Assert.Contains("must be one of", ex.Message);
        Assert.Contains("not_a_real_animation", ex.Message);
    }

    [Fact]
    public void OneOf_BrokenAnimation_EmitsWarningAndReflects()
    {
        CompilerState.Reset();
        CompilerState.Push("test");

        List<Trait> traits = TraitSystem.GetTraits(new BrokenAnimationItem(), TraitSystem.TraitType.Item);
        Trait useAnim = Assert.Single(traits, t => t.Identifier.ToString() == "minecraft:use_animation");
        Assert.Equal("bow", Assert.Single(useAnim.Properties).Value);

        List<string> logs = CompilerState.GetLogs();
        Assert.Contains(logs, l => l.Contains("animation 'bow' is broken"));

        CompilerState.Pop();
        CompilerState.Reset();
    }

    [Fact]
    public void OneOf_ValidAnimation_DoesNotEmitBrokenWarning()
    {
        CompilerState.Reset();
        CompilerState.Push("test");

        _ = TraitSystem.GetTraits(new EatAnimationItem(), TraitSystem.TraitType.Item);

        List<string> logs = CompilerState.GetLogs();
        Assert.DoesNotContain(logs, l => l.Contains("is broken"));

        CompilerState.Pop();
        CompilerState.Reset();
    }

    [Fact]
    public void GreaterThan_RejectsValueAtOrBelowThreshold()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            TraitSystem.GetTraits(new LowSecondsBlock(), TraitSystem.TraitType.Block));

        Assert.Contains("must be greater than", ex.Message);
    }

    [Fact]
    public void GreaterThan_AcceptsValueAboveThreshold()
    {
        List<Trait> traits = TraitSystem.GetTraits(new OkSecondsBlock(), TraitSystem.TraitType.Block);
        Trait mining = Assert.Single(traits, t => t.Identifier.ToString() == "test:constrained_mining");
        Assert.Contains(mining.Properties, p => p.Name == "SecondsToDestroy" && Equals(p.Value, 2f));
    }

    [Fact]
    public void NotEqual_RejectsForbiddenValue()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            TraitSystem.GetTraits(new ForbiddenStackItem(), TraitSystem.TraitType.Item));

        Assert.Contains("must not equal", ex.Message);
    }

    [Fact]
    public void FormatVersionAttribute_RejectsWhenContentVersionTooLow()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            TraitSystem.GetTraits(new OldFormatTraitItem(), TraitSystem.TraitType.Item));

        Assert.Contains("requires minimum format version", ex.Message);
        Assert.Contains("1.26.0", ex.Message);
    }

    [Fact]
    public void FormatVersionAttribute_AcceptsWhenContentVersionMeetsRequirement()
    {
        List<Trait> traits = TraitSystem.GetTraits(new NewFormatTraitItem(), TraitSystem.TraitType.Item);
        Assert.Contains(traits, t => t.Identifier.ToString() == "test:format_gated_trait");
    }

    [Fact]
    public void Version_ComparisonOperators_UseComponentOrder()
    {
        Version a = new(1, 20, 0);
        Version b = new(1, 21, 0);
        Version c = new(1, 21, 0);

        Assert.True(a < b);
        Assert.True(b > a);
        Assert.True(b >= c);
        Assert.True(b <= c);
        Assert.True(b == c);
        Assert.False(a > b);
        Assert.False(b < a);
        Assert.Equal(0, b.CompareTo(c));
    }

    private abstract class TestItem : Item
    {
        public override string Texture => "test";
    }

    private abstract class TestBlock : Block
    {
        public override MaterialInstances MaterialInstances => new()
        {
            All = new MaterialInstance("stone", MaterialInstance.RenderMethods.Opaque)
        };
    }

    private sealed class EatAnimationItem : TestItem, IUseAnimation
    {
        public override Identifier Identifier => new("test:eat_anim");
        string IUseAnimation.Value => "eat";
    }

    private sealed class InvalidAnimationItem : TestItem, IUseAnimation
    {
        public override Identifier Identifier => new("test:bad_anim");
        string IUseAnimation.Value => "not_a_real_animation";
    }

    private sealed class BrokenAnimationItem : TestItem, IUseAnimation
    {
        public override Identifier Identifier => new("test:broken_anim");
        string IUseAnimation.Value => "bow";
    }

    [TraitAttr("test:constrained_mining", TraitSystem.TraitType.Block)]
    private interface IConstrainedMining : IBlockTrait
    {
        [TraitProperty]
        [TraitPropertyConstraint(TraitPropertyConstraint.Constraint.GreaterThan, 0)]
        float SecondsToDestroy { get; }
    }

    private sealed class LowSecondsBlock : TestBlock, IConstrainedMining
    {
        public override Identifier Identifier => new("test:low_seconds");
        float IConstrainedMining.SecondsToDestroy => 0f;
    }

    private sealed class OkSecondsBlock : TestBlock, IConstrainedMining
    {
        public override Identifier Identifier => new("test:ok_seconds");
        float IConstrainedMining.SecondsToDestroy => 2f;
    }

    [TraitAttr("test:constrained_stack", TraitSystem.TraitType.Item)]
    private interface IConstrainedStack : IItemTrait
    {
        [TraitProperty]
        [TraitPropertyConstraint(TraitPropertyConstraint.Constraint.NotEqual, 0)]
        int Value { get; }
    }

    private sealed class ForbiddenStackItem : TestItem, IConstrainedStack
    {
        public override Identifier Identifier => new("test:forbidden_stack");
        int IConstrainedStack.Value => 0;
    }

    [TraitAttr("test:format_gated_trait", TraitSystem.TraitType.Item)]
    [TraitFormatVersion("1.26.0")]
    private interface IFormatGatedTrait : IItemTrait
    {
        [TraitProperty]
        [TraitPropertyConstraint(TraitPropertyConstraint.Constraint.OneOf, "eat", "drink")]
        string Value { get; }
    }

    private sealed class OldFormatTraitItem : TestItem, IFormatGatedTrait
    {
        public override Identifier Identifier => new("test:old_fmt");
        public override Version FormatVersion => new("1.20.0");
        string IFormatGatedTrait.Value => "eat";
    }

    private sealed class NewFormatTraitItem : TestItem, IFormatGatedTrait
    {
        public override Identifier Identifier => new("test:new_fmt");
        public override Version FormatVersion => new("1.26.0");
        string IFormatGatedTrait.Value => "eat";
    }
}
