using ingot.Core.Behaviour.Entity;
using ingot.Tests.Content.Entities;

namespace ingot.Tests.Entities;

public class ClientEntityCompileTest
{
    [Fact]
    public void Compile_ClientEntityJsonContainsCoreShortNames()
    {
        string json = ClientEntity.Compile(typeof(TestClientEntity));

        Assert.Contains("\"format_version\": \"1.10.0\"", json);
        Assert.Contains("minecraft:client_entity", json);
        Assert.Contains("\"identifier\": \"test:test_entity\"", json);
        Assert.Contains("\"materials\"", json);
        Assert.Contains("\"default\": \"entity_alphatest\"", json);
        Assert.Contains("\"invisible\": \"entity_invisible\"", json);
        Assert.Contains("\"textures\"", json);
        Assert.Contains("\"default\": \"textures/entity/test_entity\"", json);
        Assert.Contains("\"alt\": \"textures/entity/test_entity_alt\"", json);
        Assert.Contains("\"geometry\"", json);
        Assert.Contains("\"default\": \"geometry.test_entity\"", json);
        Assert.Contains("controller.render.test_entity", json);
    }

    [Fact]
    public void Compile_ClientEntityJsonContainsSpawnEggAndScripts()
    {
        string json = ClientEntity.Compile(typeof(TestClientEntity));

        Assert.Contains("\"spawn_egg\"", json);
        Assert.Contains("\"base_color\": \"#ff0000\"", json);
        Assert.Contains("\"overlay_color\": \"#00ff00\"", json);
        Assert.Contains("\"scripts\"", json);
        Assert.Contains("\"scale\": \"1\"", json);
        Assert.Contains("\"animate\"", json);
        Assert.Contains("\"animations\"", json);
        Assert.Contains("animation.test_entity.idle", json);
    }
}
