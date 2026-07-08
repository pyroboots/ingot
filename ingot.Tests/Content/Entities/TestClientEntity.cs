using ingot.Core.Behaviour.Entity;
using ingot.Tests.Content;

namespace ingot.Tests.Content.Entities;

internal class TestClientEntity : ClientEntity<TestEntity>
{
    public override string DefaultMaterial => "entity_alphatest";
    public override string DefaultTexture => "textures/entity/test_entity";
    public override string DefaultGeometry => "geometry.test_entity";

    [ClientEntityMaterial("invisible")]
    public string InvisibleMaterial => "entity_invisible";

    [ClientEntityTexture("alt")]
    public string AltTexture => "textures/entity/test_entity_alt";

    public override ClientEntitySpawnEgg? SpawnEgg => new()
    {
        BaseColor = "#ff0000",
        OverlayColor = "#00ff00",
    };

    public override ClientEntityScripts? Scripts => new()
    {
        Scale = "1",
        Animate = ["idle"],
    };

    public override Dictionary<string, string>? Animations => new()
    {
        ["idle"] = "animation.test_entity.idle",
    };

    public override ClientEntitySounds? EntitySounds => new()
    {
        Volume = 1f,
        Pitch = [0.8f, 1.2f],
        Events = new Dictionary<string, object>
        {
            ["ambient"] = "mob.test.say",
            ["hurt"] = "mob.test.hurt",
            ["death"] = "mob.test.death",
            ["step"] = "mob.test.step",
        },
    };
}

/// <summary>
/// Client entity that auto-registers a default texture PNG via <see cref="ClientEntity.DefaultTexturePath"/>.
/// </summary>
internal class TestClientEntityWithTexturePath : ClientEntity<TestEntity>
{
    public override string DefaultMaterial => "entity_alphatest";
    public override string DefaultTexture => "textures/entity/test_entity";
    public override string? DefaultTexturePath => FixturePaths.Resolve("auto.png");
    public override string DefaultGeometry => "geometry.test_entity";
}

/// <summary>
/// Client entity whose default texture path is nested under <c>textures/entity/</c>.
/// </summary>
internal class TestClientEntityWithNestedTexturePath : ClientEntity<TestEntity>
{
    public override string DefaultMaterial => "entity_alphatest";
    public override string DefaultTexture => "textures/entity/mobs/test_entity";
    public override string? DefaultTexturePath => FixturePaths.Resolve("auto.png");
    public override string DefaultGeometry => "geometry.test_entity";
}

internal class TestRenderController : RenderController
{
    public override string ControllerId => "controller.render.test_entity_custom";

    public override string Geometry => "Geometry.default";

    public override string[] Textures => ["Texture.default", "Texture.alt"];

    public override Dictionary<string, string[]>? TextureArrays => new()
    {
        ["Array.skins"] = ["Texture.default", "Texture.alt"],
    };
}
