using ingot.Core.Behaviour.Entity;
using ingot.Tests.Content.Entities;

namespace ingot.Tests.Entities;

public class RenderControllerCompileTest
{
    [Fact]
    public void Compile_SimpleRenderControllerHasDefaultBindings()
    {
        RenderController simple = RenderController.CreateSimple("controller.render.cow");
        string json = RenderController.CompileInstance(simple);

        Assert.Contains("\"format_version\": \"1.10.0\"", json);
        Assert.Contains("\"controller.render.cow\"", json);
        Assert.Contains("\"geometry\": \"Geometry.default\"", json);
        Assert.Contains("\"Material.default\"", json);
        Assert.Contains("\"Texture.default\"", json);
    }

    [Fact]
    public void Compile_CustomRenderControllerIncludesArraysAndLayers()
    {
        string json = RenderController.Compile(typeof(TestRenderController));

        Assert.Contains("\"controller.render.test_entity_custom\"", json);
        Assert.Contains("\"arrays\"", json);
        Assert.Contains("\"Array.skins\"", json);
        Assert.Contains("\"Texture.default\"", json);
        Assert.Contains("\"Texture.alt\"", json);
    }
}
