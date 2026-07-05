using ingot.Core.Common;

using Newtonsoft.Json;

using static ingot.Core.Common.JsonHelper;

using Formatting = Newtonsoft.Json.Formatting;
using Version = ingot.Core.Common.Version;

namespace ingot.Core;

internal static class ManifestWriter
{
    internal static void WriteBehaviourPackManifest(Pack pack, string outputPath)
    {
        using StringWriter sw = new();
        JsonTextWriter w = new(sw)
        {
            Formatting = Formatting.Indented,
            Indentation = 4,
        };

        JsonHelper json = new(ref w);

        w.WriteStartObject();

        json.Property("format_version", 2);
        json.Object("header", () =>
        {
            json.Property("name", pack.Name);
            json.Property("description", pack.Description);
            json.Property("uuid", pack.BehaviourPack.Uuid);
            json.Property("version", pack.PackVersion.AsArray());
            json.Property("min_engine_version", pack.MinEngineVersion.AsArray());
        });

        json.Array("modules", () =>
        {
            json.Object("", () =>
            {
                json.Property("description", $"{pack.Name} Behaviour");
                json.Property("type", "data");
                json.Property("uuid", Guid.NewGuid().ToString());
                json.Property("version", new Version(1, 0, 0).AsArray());
            });
            if (pack.ScriptsEnabled)
            {
                json.Object("", () =>
                {
                    json.Property("type", "script");
                    json.Property("language", "javascript");
                    json.Property("uuid", Guid.NewGuid().ToString());
                    json.Property("entry", pack.ScriptEntry);
                    json.Property("version", new Version(1, 0, 0).AsArray());
                });
            }
        });

        json.Array("dependencies", () =>
        {
            if (pack.LinkPacks)
            {
                json.Object("", () =>
                {
                    json.Property("uuid", pack.ResourcePack.Uuid);
                    json.Property("version", pack.ResourcePack.ResourcePackVersion.AsArray());
                });
            }

            if (pack.ScriptsEnabled)
            {
                foreach (KeyValuePair<string, Version> kvp in pack.ScriptApiModules)
                {
                    json.Object("", () =>
                    {
                        json.Property("module_name", kvp.Key);
                        json.Property("version", kvp.Value.AsArray());
                    });
                }
            }
        });

        if (pack.OmitMetadata == false) WriteMetadata(json, pack.Authors);

        w.WriteEndObject();
        File.WriteAllText(outputPath, sw.ToString());
    }

    internal static void WriteResourcePackManifest(Pack pack, string outputPath)
    {
        using StringWriter sw = new();
        JsonTextWriter w = new(sw)
        {
            Formatting = Formatting.Indented,
            Indentation = 4,
        };

        JsonHelper json = new(ref w);

        w.WriteStartObject();

        json.Property("format_version", 2);
        json.Object("header", () =>
        {
            json.Property("name", pack.Name);
            json.Property("description", pack.Description);
            json.Property("uuid", pack.ResourcePack.Uuid);
            json.Property("version", pack.PackVersion.AsArray());
            json.Property("min_engine_version", pack.MinEngineVersion.AsArray());
        });

        json.Array("modules", () =>
        {
            json.Object("", () =>
            {
                json.Property("description", $"{pack.Name} Resources");
                json.Property("type", "resources");
                json.Property("uuid", Guid.NewGuid().ToString());
                json.Property("version", new Version(1, 0, 0).AsArray());
            });
        });

        json.Array("dependencies", () =>
        {
            if (pack.LinkPacks)
            {
                json.Object("", () =>
                {
                    json.Property("uuid", pack.BehaviourPack.Uuid);
                    json.Property("version", pack.BehaviourPack.BehaviourPackVersion.AsArray());
                });
            }
        });

        WriteMetadata(json, pack.Authors);

        w.WriteEndObject();
        File.WriteAllText(outputPath, sw.ToString());
    }

    private static void WriteMetadata(JsonHelper json, string[] authors)
    {
        json.Object("metadata", () =>
        {
            json.Property("authors", authors);
            json.Object("generated_with", () =>
            {
                json.Property("ingot", new[] { "https://github.com/pyroboots/ingot", "1.0.1" });
            });
        });
    }
}