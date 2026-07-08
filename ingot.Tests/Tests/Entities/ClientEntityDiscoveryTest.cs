using ingot.Core.Behaviour.Entity;
using ingot.Core.Common;
using ingot.Tests.Content.Entities;
using ingot.Tests.Support;

namespace ingot.Tests.Entities;

public class ClientEntityDiscoveryTest
{
    [Fact]
    public void AddEntity_DiscoversNestedClientEntity()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();

        PackTestBuilder.Create()
            .AddEntity<DiscoveryTestEntity>()
            .Compile(output.Path, verbose: false);

        string path = Path.Combine(output.Path, "rp", "entity", "discovery_test.json");
        Assert.True(File.Exists(path), "expected nested Client to be auto-registered");
        Assert.Contains("textures/entity/discovery", File.ReadAllText(path));
    }

    [Fact]
    public void AddEntity_DiscoverClientFalse_SkipsClient()
    {
        using TempOutputDirectory output = CompileTestHelper.CreateTempDirectory();

        PackTestBuilder.Create()
            .AddEntity<DiscoveryTestEntity>(discoverClient: false)
            .Compile(output.Path, verbose: false);

        string path = Path.Combine(output.Path, "rp", "entity", "discovery_test.json");
        Assert.False(File.Exists(path));
    }
}

internal class DiscoveryTestEntity : Entity
{
    public override Identifier Identifier => new("test", "discovery_test");

    public class Client : ClientEntity<DiscoveryTestEntity>
    {
        public override string DefaultTexture => "textures/entity/discovery";
        public override string DefaultGeometry => "geometry.discovery";
    }
}
