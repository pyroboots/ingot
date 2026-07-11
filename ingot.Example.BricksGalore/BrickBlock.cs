using ingot.Core.Behaviour.Block;
using ingot.Core.Common;
using ingot.Core.TraitSystem.Traits.Block;

namespace ingot.Example.BricksGalore;

/// <summary>
/// Parameterised brick block. Each closed <typeparamref name="TToken"/> type carries its own
/// static <see cref="Spec"/> so ingot's type-based compile path yields unique JSON per combo.
/// </summary>
public class BrickBlock<TToken> : Block, IDestructibleByMining, IDestructibleByExplosion
    where TToken : class
{
    /// <summary>
    /// Must be assigned on the closed generic type before the pack is compiled.
    /// </summary>
    public static BlockSpec Spec { get; set; } = null!;

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
