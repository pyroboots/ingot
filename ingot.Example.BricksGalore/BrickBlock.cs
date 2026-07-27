using ingot.Core.Behaviour.Block;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Block;

namespace ingot.Example.BricksGalore;

/// <summary>
/// Brick block configured by an instance <see cref="Spec"/>. Registered with
/// <c>BehaviourPack.AddBlockFromInstance</c> so each combo keeps its own data without
/// per-combo runtime types.
/// </summary>
public class BrickBlock : Block, IDestructibleByMining, IDestructibleByExplosion
{
    /// <summary>
    /// Per-combo block data for this instance.
    /// </summary>
    public required BlockSpec Spec { get; init; }

    public override Identifier Identifier => Spec.Identifier;
    public override string DisplayName => Spec.DisplayName;
    public override string? Geometry => "minecraft:geometry.full_block";
    public override string? Sound => Spec.Sound;
    public override string? ResourceTexture => Spec.Texture;
    public override Enums.CatalogueCategory Category => Enums.CatalogueCategory.Construction;
    // bedrock requires a namespaced identifier here (namespace:name), not a bare lang key.
    public override string? Group => $"{BrickStats.Namespace}:{Spec.Material}";
    public override string[] Tags => Spec.Tags;

    public override MaterialInstances MaterialInstances => new()
    {
        All = new MaterialInstance(
            Spec.Texture,
            MaterialInstance.RenderMethods.Opaque,
            Spec.TexturePath),
    };

    dynamic? IDestructibleByMining.ItemSpecificSpeeds => null;
    float IDestructibleByMining.SecondsToDestroy => Spec.SecondsToDestroy;
    float IDestructibleByExplosion.ExplosionResistance => Spec.ExplosionResistance;
}
