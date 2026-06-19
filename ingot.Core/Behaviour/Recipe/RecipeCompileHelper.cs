using Newtonsoft.Json;

using Formatting = Newtonsoft.Json.Formatting;

namespace ingot.Core.Behaviour.Recipe;

internal static class RecipeCompileHelper
{
    internal static (StringWriter StringWriter, JsonTextWriter Writer) CreateWriter()
    {
        StringWriter sw = new();
        JsonTextWriter w = new(sw)
        {
            Formatting = Formatting.Indented,
            Indentation = 4,
        };
        return (sw, w);
    }

    internal static T CreateInstance<T>(Type type) where T : class =>
        (Activator.CreateInstance(type) as T)!;
}