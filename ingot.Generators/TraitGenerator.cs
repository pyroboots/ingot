using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using ingot.Core.Common;

namespace ingot.Generators;

public record TraitProperty(
    string Name,
    string DefaultValue,
    string RawType,
    string Description
);

public sealed class TraitGenerator : IAsyncDisposable
{
    private static readonly HttpClient SharedHttpClient = new();
    private static readonly Dictionary<string, string> MappedTypes = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> WarnedUnmappedTypes = new(StringComparer.OrdinalIgnoreCase);

    // convert ms doc types to c# types
    // NOTE: works on a contains basis, order of precedence.
    private static readonly Dictionary<string, string> TypeMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Minecraft Event Reference"] = "Identifier",
        ["Array of Riders items"] = "EntityRider[]",
        ["Array of Feed Items items"] = "EntityFeedItem[]",
        ["Minecraft filter"] = "EntityFilter",
        ["Filter item"] = "EntityFilter",
        ["Range of floats"] = "FloatRange",
        ["Minecraft Event Trigger"] = "EntityEventTrigger",
        ["Array of Break items"] = "EntityBlockBreakEntry[]",
        
        ["x, y, z coordinate array"] = "Vector3",
        ["Array of numbers"] = "int[]",
        ["Array of strings"] = "string[]",
        ["Boolean"] = "bool",
        ["bool"] = "bool",
        ["Integer"] = "int",
        ["integer"] = "int",
        ["Decimal"] = "float",
        ["decimal"] = "float",
        ["String"] = "string",
        ["string"] = "string",
        ["Object"] = "dynamic",
        ["keyed set of strings"] = "Dictionary<string, string>",
    };

    // formatters
    private static readonly Dictionary<string, Func<string, string>> ValueTransformers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["bool"] = v => bool.TryParse(v, out bool b) ? b.ToString().ToLowerInvariant() : "false",
        ["int"] = v => int.TryParse(v, out int i) ? i.ToString() : "0",
        ["float"] = v =>
        {
            if (string.IsNullOrEmpty(v)) return "0f";
            if (!v.EndsWith("f", StringComparison.OrdinalIgnoreCase) && float.TryParse(v, out _))
                return v + "f";
            return v;
        },
        ["string"] = v =>
        {
            if (string.IsNullOrEmpty(v) || v == "{}") return "\"\"";
            return $"\"{v.Replace("\"", "\\\"")}\"";
        },
        ["string[]"] = v =>
        {
            if (string.IsNullOrWhiteSpace(v) || v == "[]") return "Array.Empty<string>()";
            string[] parts = v.Split(',');
            string joined = string.Join(", ", parts.Select(s => $"\"{s.Trim()}\""));
            return $"new[] {{ {joined} }}";
        },
        ["int[]"] = v => v,
        ["Vector3"] = v => $"new Vector3({v})",
        ["Dictionary<string, string>"] = v => string.IsNullOrWhiteSpace(v) ? "new Dictionary<string, string>()" : v,

        ["dynamic"] = v => "null",
    };

    public static string GenerateTraitInterfaceFromMsDoc(string html, string interfaceName, string componentName, string constraint, string? @namespace = null)
    {
        string description = ParseComponentDescription(html);
        List<TraitProperty> properties = ParseHtmlToProperties(html);
        return GenerateInterfaceCode(interfaceName, componentName, constraint, description, properties, @namespace);
    }

    private static string ParseComponentDescription(string html)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);

        HtmlNode? paragraph = doc.DocumentNode.SelectSingleNode("//div[@class='content']/p");
        if (paragraph == null)
            return "";

        string description = paragraph.InnerText.Trim();
        if (description.Equals("Note", StringComparison.OrdinalIgnoreCase))
            return "";

        return Regex.Replace(description, @"\s+", " ").Trim();
    }

    private static List<TraitProperty> ParseHtmlToProperties(string html)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);

        HtmlNode? table = doc.DocumentNode.SelectSingleNode("//table");
        if (table == null)
            return new List<TraitProperty>();

        Dictionary<string, TraitProperty> propertiesByName = new(StringComparer.OrdinalIgnoreCase);
        HtmlNodeCollection? rows = table.SelectNodes(".//tr");

        if (rows == null)
            return [];

        bool isHeader = true;

        foreach (HtmlNode row in rows)
        {
            HtmlNodeCollection? cells = row.SelectNodes("th|td");
            if (cells == null || cells.Count < 4)
                continue;

            string name = cells[0].InnerText.Trim();
            string defaultVal = cells[1].InnerText.Trim();
            string type = cells[2].InnerText.Trim();
            string desc = cells[3].InnerText.Trim();

            if (isHeader && name.Equals("Name", StringComparison.OrdinalIgnoreCase))
            {
                isHeader = false;
                continue;
            }
            isHeader = false;

            // TODO: add deprecated attrib
            if (desc.Contains("Deprecated", StringComparison.OrdinalIgnoreCase))
                continue;

            string cleanName = CleanPropertyName(name);
            if (string.IsNullOrEmpty(cleanName))
                continue;

            string cleanDesc = Regex.Replace(desc, @"\s+", " ").Trim();
            string cleanDefault = defaultVal.Equals("not set", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrEmpty(defaultVal)
                ? "" : defaultVal;

            TraitProperty candidate = new(cleanName, cleanDefault, type, cleanDesc);
            if (!propertiesByName.TryGetValue(cleanName, out TraitProperty? existing)
                || ShouldReplaceProperty(existing, candidate))
            {
                propertiesByName[cleanName] = candidate;
            }
        }

        return propertiesByName.Values.ToList();
    }

    private static string CleanPropertyName(string name)
    {
        name = Regex.Replace(name, @"\s*\([^)]*\)", "", RegexOptions.IgnoreCase);
        return name.Trim().ToLowerInvariant().Replace(" ", "_");
    }

    private static string ToInterfaceName(string pageName, bool isBehaviour = false)
    {
        string normalized = pageName.Replace('.', '_');
        return $"I{(isBehaviour ? "Behavior" : "")}{Formatting.SnakeToPascalCase(normalized)}";
    }

    private static int ScoreProperty(TraitProperty prop)
    {
        int score = 0;
        string csharpType = MapType(prop.RawType);
        if (csharpType != "dynamic")
            score += 10;
        if (!string.IsNullOrEmpty(prop.DefaultValue))
            score += 5;
        if (!string.IsNullOrEmpty(prop.Description))
            score += 1;
        return score;
    }

    private static bool ShouldReplaceProperty(TraitProperty existing, TraitProperty candidate) =>
        ScoreProperty(candidate) > ScoreProperty(existing);

    private static string GenerateInterfaceCode(string interfaceName, string componentName, string constraint, string description, List<TraitProperty> properties, string? nspace)
    {
        nspace ??= $"namespace ingot.Core.TraitSystem.Traits.{constraint};";

        StringBuilder sb = new StringBuilder();
        HashSet<string> seen = new HashSet<string>();
        HashSet<string> requiredUsings = new(StringComparer.Ordinal);

        foreach (TraitProperty prop in properties)
        {
            string csharpType = MapType(prop.RawType);
            if (csharpType == "Vector3")
                requiredUsings.Add("System.Numerics");
            if (csharpType.Contains("Dictionary", StringComparison.Ordinal))
                requiredUsings.Add("System.Collections.Generic");
        }

        sb.AppendLine(nspace);
        foreach (string usingDirective in requiredUsings.OrderBy(static u => u, StringComparer.Ordinal))
            sb.AppendLine($"using {usingDirective};");
        sb.AppendLine("using ingot.Core.Common;");
        sb.AppendLine("using ingot.Core.Behaviour.Entity;");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(description))
        {
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {description}");
            sb.AppendLine("/// </summary>");
        }

        sb.AppendLine($"[Trait(\"{componentName}\", TraitSystem.TraitType.{constraint})]");
        sb.AppendLine($"public interface {interfaceName} : I{constraint}Trait");
        sb.AppendLine("{");

        foreach (TraitProperty prop in properties)
        {
            string pascalName = Formatting.SnakeToPascalCase(prop.Name);
            if (!seen.Add(pascalName))
                continue;

            string csharpType = MapType(prop.RawType);
            bool isAbstract = string.IsNullOrEmpty(prop.DefaultValue);
            string defaultExpr = isAbstract ? "" : FormatDefaultValue(csharpType, prop.DefaultValue);

            if (!string.IsNullOrEmpty(prop.Description))
            {
                sb.AppendLine("    /// <summary>");
                sb.AppendLine($"    /// {prop.Description}");
                sb.AppendLine("    /// </summary>");
            }
            sb.AppendLine("    [TraitProperty]");

            if (isAbstract)
            {
                if (pascalName.Contains("Identifier", StringComparison.Ordinal)) csharpType = "Identifier";
                sb.AppendLine($"    public abstract {csharpType} {pascalName} {{ get; }}");
            }
            else
                sb.AppendLine($"    public virtual {csharpType} {pascalName} => {defaultExpr};");

            sb.AppendLine();
        }

        if (properties.Count > 0)
            sb.Length -= Environment.NewLine.Length;

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string MapType(string rawType)
    {
        if (MappedTypes.TryGetValue(rawType, out string? cached))
            return cached;

        foreach (KeyValuePair<string, string> mapping in TypeMappings)
        {
            if (rawType.Contains(mapping.Key, StringComparison.OrdinalIgnoreCase))
            {
                MappedTypes[rawType] = mapping.Value;
                return mapping.Value;
            }
        }

        if (WarnedUnmappedTypes.Add(rawType))
            Console.WriteLine($"/!\\ could not map type '{rawType}', fell back to dynamic");

        MappedTypes[rawType] = "dynamic";
        return "dynamic";
    }

    private static string FormatDefaultValue(string csharpType, string rawValue)
    {
        if (ValueTransformers.TryGetValue(csharpType, out Func<string, string>? transformer))
            return transformer(rawValue);

        if (csharpType == "string")
            return $"\"{rawValue.Replace("\"", "\\\"")}\"";

        return rawValue;
    }

    public Task GenerateAllItemTraitsAsync(string outputDir) =>
        GenerateTraitsForComponentsAsync(
            [
                "minecraft:block_placer",
                "minecraft:bundle_interaction",
                "minecraft:can_destroy_in_creative",
                "minecraft:compostable",
                "minecraft:cooldown",
                "minecraft:damage",
                "minecraft:damage_absorption",
                "minecraft:digger",
                "minecraft:durability",
                "minecraft:durability_sensor",
                "minecraft:dyeable",
                "minecraft:enchantable",
                "minecraft:fire_resistant",
                "minecraft:food",
                "minecraft:fuel",
                "minecraft:glint",
                "minecraft:hand_equipped",
                "minecraft:hover_text_color",
                "minecraft:interact_button",
                "minecraft:kinetic_weapon",
                "minecraft:liquid_clipped",
                "minecraft:piercing_weapon",
                "minecraft:projectile",
                "minecraft:rarity",
                "minecraft:record",
                "minecraft:seed",
                "minecraft:shooter",
                "minecraft:should_despawn",
                "minecraft:stacked_by_data",
                "minecraft:storage_item",
                "minecraft:storage_weight_limit",
                "minecraft:storage_weight_modifier",
                "minecraft:swing_duration",
                "minecraft:swing_sounds",
                "minecraft:tags",
                "minecraft:throwable",
                "minecraft:use_animation",
                "minecraft:use_modifiers",
                "minecraft:wearable",
            ],
            outputDir,
            "Item",
            "minecraft_");

    public Task GenerateAllBlockTraitsAsync(string outputDir) =>
        GenerateTraitsForComponentsAsync(
            [
                "minecraft:chest_obstruction",
                "minecraft:collision_box",
                "minecraft:connection_rule",
                "minecraft:crafting_table",
                "minecraft:destructible_by_explosion",
                "minecraft:destructible_by_mining",
                "minecraft:destruction_particles",
                "minecraft:entity_fall_on",
                "minecraft:flammable",
                "minecraft:geometry",
                "minecraft:instrument_sound",
                "minecraft:liquid_detection",
                "minecraft:map_color",
                "minecraft:movable",
                "minecraft:placement_filter",
                "minecraft:precipitation_interactions",
                "minecraft:random_offset",
                "minecraft:redstone_conductivity",
                "minecraft:redstone_consumer",
                "minecraft:redstone_producer",
                "minecraft:selection_box",
                "minecraft:support",
                "minecraft:tick",
            ],
            outputDir,
            "Block",
            "minecraftblock_");
    
    public Task GenerateAllEntityTraitsAsync(string outputDir) =>
        GenerateTraitsForComponentsAsync(
            [
                "minecraft:addrider",
                "minecraft:admire_item",
                "minecraft:ageable",
                "minecraft:ambient_sound_interval",
                "minecraft:anger_level",
                "minecraft:angry",
                "minecraft:annotation.break_door",
                "minecraft:annotation.open_door",
                "minecraft:apply_knockback_rules",
                "minecraft:apply_knockback_rules_instance",
                "minecraft:area_attack",
                "minecraft:attack",
                "minecraft:attack_cooldown",
                "minecraft:balloonable",
                "minecraft:barter",
                "minecraft:block_climber",
                "minecraft:block_sensor",
                "minecraft:body_rotation_always_follows_head",
                "minecraft:body_rotation_axis_aligned",
                "minecraft:body_rotation_blocked",
                "minecraft:body_rotation_locked_to_vehicle",
                "minecraft:boostable",
                "minecraft:boss",
                "minecraft:break_blocks",
                "minecraft:breathable",
                "minecraft:breedable",
                "minecraft:bribeable",
                "minecraft:buoyant",
                "minecraft:burns_in_daylight",
                "minecraft:cannot_be_attacked",
                "minecraft:can_climb",
                "minecraft:can_fly",
                "minecraft:can_join_raid",
                "minecraft:can_power_jump",
                "minecraft:celebrate_hunt",
                "minecraft:collision_box",
                "minecraft:color",
                "minecraft:color2",
                "minecraft:combat_regeneration",
                "minecraft:conditional_bandwidth_optimization",
                "minecraft:custom_hit_test",
                "minecraft:damage_over_time",
                "minecraft:damage_sensor",
                "minecraft:dash",
                "minecraft:dash_action",
                "minecraft:default_look_angle",
                "minecraft:despawn",
                "minecraft:dimension_bound",
                "minecraft:drying_out_timer",
                "minecraft:dweller",
                "minecraft:economy_trade_table",
                "minecraft:entity_armor_equipment_slot_mapping",
                "minecraft:entity_sensor",
                "minecraft:environment_sensor",
                "minecraft:equipment",
                "minecraft:equippable",
                "minecraft:equip_item",
                "minecraft:exhaustion_values",
                "minecraft:experience_reward",
                "minecraft:explode",
                "minecraft:fire_immune",
                "minecraft:floats_in_liquid",
                "minecraft:flocking",
                "minecraft:flying_speed",
                "minecraft:follow_range",
                "minecraft:free_camera_controlled",
                "minecraft:friction_modifier",
                "minecraft:game_event_movement_tracking",
                "minecraft:genetics",
                "minecraft:giveable",
                "minecraft:ground_offset",
                "minecraft:group_size",
                "minecraft:grows_crop",
                "minecraft:healable",
                "minecraft:heartbeat",
                "minecraft:hide",
                "minecraft:home",
                "minecraft:horse.jump_strength",
                "minecraft:hurt_on_condition",
                "minecraft:ignore_cannot_be_attacked",
                "minecraft:input_air_controlled",
                "minecraft:input_ground_controlled",
                "minecraft:inside_block_notifier",
                "minecraft:insomnia",
                "minecraft:instant_despawn",
                "minecraft:interact",
                "minecraft:inventory",
                "minecraft:is_baby",
                "minecraft:is_charged",
                "minecraft:is_chested",
                "minecraft:is_collidable",
                "minecraft:is_dyeable",
                "minecraft:is_hidden_when_invisible",
                "minecraft:is_ignited",
                "minecraft:is_illager_captain",
                "minecraft:is_pregnant",
                "minecraft:is_saddled",
                "minecraft:is_shaking",
                "minecraft:is_sheared",
                "minecraft:is_stackable",
                "minecraft:is_stunned",
                "minecraft:is_tamed",
                "minecraft:item_controllable",
                "minecraft:item_hopper",
                "minecraft:jump.dynamic",
                "minecraft:jump.static",
                "minecraft:knockback_resistance",
                "minecraft:lava_movement",
                "minecraft:leashable",
                "minecraft:leashable_to",
                "minecraft:looked_at",
                "minecraft:loot",
                "minecraft:managed_wandering_trader",
                "minecraft:mark_variant",
                "minecraft:mob_effect",
                "minecraft:mob_effect_immunity",
                "minecraft:movement",
                "minecraft:movement.amphibious",
                "minecraft:movement.basic",
                "minecraft:movement.fly",
                "minecraft:movement.generic",
                "minecraft:movement.hover",
                "minecraft:movement.jump",
                "minecraft:movement.skip",
                "minecraft:movement.sound_distance_offset",
                "minecraft:movement.sway",
                "minecraft:nameable",
                "minecraft:navigation.climb",
                "minecraft:navigation.float",
                "minecraft:navigation.fly",
                "minecraft:navigation.generic",
                "minecraft:navigation.hover",
                "minecraft:navigation.swim",
                "minecraft:navigation.walk",
                "minecraft:offspring",
                "minecraft:out_of_control",
                "minecraft:peek",
                "minecraft:persistent",
                "minecraft:physics",
                "minecraft:player.exhaustion",
                "minecraft:player.experience",
                "minecraft:player.level",
                "minecraft:player.saturation",
                "minecraft:preferred_path",
                "minecraft:projectile",
                "minecraft:pushable",
                "minecraft:pushable_by_block",
                "minecraft:pushable_by_entity",
                "minecraft:push_through",
                "minecraft:raid_trigger",
                "minecraft:rail_movement",
                "minecraft:rail_sensor",
                "minecraft:ravager_blocked",
                "minecraft:reflect_projectiles",
                "minecraft:remove_in_peaceful",
                "minecraft:renders_when_invisible",
                "minecraft:rideable",
                "minecraft:rotation_axis_aligned",
                "minecraft:rotation_locked_to_vehicle",
                "minecraft:scale",
                "minecraft:scale_by_age",
                "minecraft:scheduler",
                "minecraft:shareables",
                "minecraft:shooter",
                "minecraft:sittable",
                "minecraft:skin_id",
                "minecraft:sound_volume",
                "minecraft:spawn_egg_interaction",
                "minecraft:spawn_entity",
                "minecraft:spawn_on_death",
                "minecraft:spell_effects",
                "minecraft:strength",
                "minecraft:suspect_tracking",
                "minecraft:tameable",
                "minecraft:tamemount",
                "minecraft:target_nearby_sensor",
                "minecraft:teleport",
                "minecraft:tick_world",
                "minecraft:timer",
                "minecraft:trade_table",
                "minecraft:trail",
                "minecraft:transformation",
                "minecraft:transient",
                "minecraft:trusting",
                "minecraft:type_family",
                "minecraft:underwater_mount_breathing",
                "minecraft:underwater_movement",
                "minecraft:uses_legacy_friction",
                "minecraft:variable_max_auto_step",
                "minecraft:variant",
                "minecraft:vertical_movement_action",
                "minecraft:vibration_damper",
                "minecraft:vibration_listener",
                "minecraft:walk_animation_speed",
                "minecraft:wants_jockey",
                "minecraft:water_movement",
                "minecraft:wither_target_highest_damage",
            ],
            outputDir,
            "Entity",
            "minecraftcomponent_");
    
    public Task GenerateAllEntityBehaviourTraitsAsync(string outputDir) =>
        GenerateTraitsForComponentsAsync(
            [
                "minecraft:behavior.nearest_attackable_target",
                "minecraft:behavior.melee_attack",
                "minecraft:behavior.panic",
                "minecraft:behavior.hurt_by_target",
                "minecraft:behavior.look_at_player",
                "minecraft:behavior.random_stroll",
                "minecraft:behavior.random_look_around",
                "minecraft:behavior.float",
                "minecraft:behavior.follow_owner",
                "minecraft:behavior.breed",
                "minecraft:behavior.tempt",
                "minecraft:behavior.avoid_mob_type",
                "minecraft:behavior.move_towards_target",
                "minecraft:behavior.leap_at_target",
                "minecraft:behavior.owner_hurt_by_target",
                "minecraft:behavior.owner_hurt_target",
                "minecraft:behavior.follow_parent",
                "minecraft:behavior.look_at_target",
                "minecraft:behavior.open_door",
                "minecraft:behavior.break_door",
                "minecraft:behavior.move_to_water",
                "minecraft:behavior.move_to_land",
                "minecraft:behavior.go_home",
                "minecraft:behavior.move_towards_restriction",
                "minecraft:behavior.move_indoors",
                "minecraft:behavior.find_cover",
                "minecraft:behavior.flee_sun",
                "minecraft:behavior.follow_mob",
                "minecraft:behavior.mingle",
                "minecraft:behavior.make_love",
                "minecraft:behavior.lay_egg",
                "minecraft:behavior.harvest_farm_block",
                "minecraft:behavior.fertilize_farm_block",
                "minecraft:behavior.eat_block",
                "minecraft:behavior.equip_item",
                "minecraft:behavior.hold_ground",
                "minecraft:behavior.knockback_roar",
                "minecraft:behavior.charge_attack",
                "minecraft:behavior.delayed_attack",
                "minecraft:behavior.admire_item",
                "minecraft:behavior.aquatic_charge_attack",
                "minecraft:behavior.avoid_block",
                "minecraft:behavior.barter",
                "minecraft:behavior.beg",
                "minecraft:behavior.celebrate",
                "minecraft:behavior.celebrate_survive",
                "minecraft:behavior.charge_held_item",
                "minecraft:behavior.circle_around_anchor",
                "minecraft:behavior.controlled_by_player",
                "minecraft:behavior.croak",
                "minecraft:behavior.defend_trusted_target",
                "minecraft:behavior.defend_village_target",
                "minecraft:behavior.dig",
                "minecraft:behavior.door_interact",
                "minecraft:behavior.drink_milk",
                "minecraft:behavior.drink_potion",
                "minecraft:behavior.drop_item_for",
                "minecraft:behavior.eat_carried_item",
                "minecraft:behavior.eat_mob",
                "minecraft:behavior.emerge",
                "minecraft:behavior.enderman_leave_block",
                "minecraft:behavior.enderman_take_block",
                "minecraft:behavior.explore_outskirts",
                "minecraft:behavior.find_mount",
                "minecraft:behavior.find_underwater_treasure",
                "minecraft:behavior.fire_at_target",
                "minecraft:behavior.float_tempt",
                "minecraft:behavior.float_wander",
                "minecraft:behavior.follow_caravan",
                "minecraft:behavior.follow_target_captain",
                "minecraft:behavior.follow_target_leader",
                "minecraft:behavior.go_and_give_items_to_noteblock",
                "minecraft:behavior.go_and_give_items_to_owner",
                "minecraft:behavior.guardian_attack",
                "minecraft:behavior.hide",
                "minecraft:behavior.hover",
                "minecraft:behavior.inspect_bookshelf",
                "minecraft:behavior.investigate_suspicious_location",
                "minecraft:behavior.jump_around_target",
                "minecraft:behavior.jump_to_block",
                "minecraft:behavior.lay_down",
                "minecraft:behavior.look_at_entity",
                "minecraft:behavior.look_at_trading_player",
                "minecraft:behavior.melee_box_attack",
                "minecraft:behavior.mount_pathing",
                "minecraft:behavior.move_around_target",
                "minecraft:behavior.move_outdoors",
                "minecraft:behavior.move_through_village",
                "minecraft:behavior.move_towards_dwelling_restriction",
                "minecraft:behavior.move_towards_home_restriction",
                "minecraft:behavior.move_to_block",
                "minecraft:behavior.move_to_liquid",
                "minecraft:behavior.move_to_poi",
                "minecraft:behavior.move_to_random_block",
                "minecraft:behavior.move_to_village",
                "minecraft:behavior.nap",
                "minecraft:behavior.nearest_prioritized_attackable_target",
                "minecraft:behavior.ocelotattack",
                "minecraft:behavior.ocelot_sit_on_block",
                "minecraft:behavior.offer_flower",
                "minecraft:behavior.pet_sleep_with_owner",
            ],
            outputDir,
            "Entity",
            "minecraftbehavior_", true);

    private static async Task GenerateTraitsForComponentsAsync(
        string[] components,
        string outputDir,
        string constraint,
        string urlPrefix,
        bool isBehavior = false)
    {
        Stopwatch sw = Stopwatch.StartNew();
        int success = 0;

        foreach (string component in components)
        {
            Console.WriteLine($"generating '{component}' interface...");

            string[] parts = component.Split(':');
            string pageName = isBehavior ? parts[1].Replace("behavior.", "") : parts[1];
            string url = $"https://learn.microsoft.com/en-us/minecraft/creator/reference/content/{constraint.ToLowerInvariant()}reference/examples/{constraint.ToLowerInvariant()}{(isBehavior ? "goals" : "components")}/{urlPrefix}{pageName}?view=minecraft-bedrock-stable";
            try
            {
                string html = await SharedHttpClient.GetStringAsync(url);

                string ifaceName = ToInterfaceName(pageName, isBehavior);

                string code = GenerateTraitInterfaceFromMsDoc(html, ifaceName, component, constraint);
                string fullCode = $"// autogenerated by ingot trait generator from\n// {url}\n\n{code}";

                string path = Path.Combine(outputDir, $"{ifaceName}.cs");
                await File.WriteAllTextAsync(path, fullCode);

                success++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"failed to generate {component}: {ex.Message}");
                if (ex is HttpRequestException)
                    Console.WriteLine(url);
            }
        }

        sw.Stop();
        Console.WriteLine($"completed {success}/{components.Length} traits in {sw.ElapsedMilliseconds}ms");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}