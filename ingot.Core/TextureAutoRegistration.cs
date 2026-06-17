using ingot.Core.Behaviour.Block;
using Newtonsoft.Json;

namespace ingot.Core;

internal static class TextureAutoRegistration
{
    public static bool IsCustomTextureKey(string key) =>
        !string.IsNullOrWhiteSpace(key) && !key.Contains(':');

    public static void RegisterMaterialInstances(MaterialInstances instances, ref JsonTextWriter? warnWriter)
    {
        if (CompilerState.CurrentPack is null)
            return;

        ResourcePack rp = CompilerState.CurrentPack.ResourcePack;

        foreach ((string key, string? sourcePath) in instances.EnumerateTextures())
        {
            if (!IsCustomTextureKey(key))
                continue;

            if (rp.TryAddBlockTexture(key, sourcePath))
            {
                CompilerState.Info($"auto-registered block texture '{key}'");
                continue;
            }

            if (string.IsNullOrWhiteSpace(sourcePath))
                continue;

            CompilerState.Warn(
                ref warnWriter,
                $"block texture '{key}' was not auto-registered because it is already defined on the resource pack");
        }
    }

    public static void RegisterItemTexture(string key, string? sourcePath, ref JsonTextWriter? warnWriter)
    {
        if (CompilerState.CurrentPack is null || !IsCustomTextureKey(key))
            return;

        ResourcePack rp = CompilerState.CurrentPack.ResourcePack;

        if (rp.TryAddItemTexture(key, sourcePath))
        {
            CompilerState.Info($"auto-registered item texture '{key}'");
            return;
        }

        if (string.IsNullOrWhiteSpace(sourcePath))
            return;

        CompilerState.Warn(
            ref warnWriter,
            $"item texture '{key}' was not auto-registered because it is already defined on the resource pack");
    }
}