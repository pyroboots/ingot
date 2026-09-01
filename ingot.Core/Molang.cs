using Newtonsoft.Json;

namespace ingot.Core;

internal class MolangJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        => writer.WriteValue(value is Molang molang ? molang.ToString() : null);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        if (reader.Value is string raw)
            return new Molang(raw);

        throw new JsonSerializationException($"cannot convert {reader.TokenType} to {nameof(Molang)}");
    }

    public override bool CanConvert(Type objectType) => objectType == typeof(Molang);
}

/// <summary>
/// Builder for molang expressions
/// </summary>
[JsonConverter(typeof(MolangJsonConverter))]
public partial class Molang
{
    private readonly List<string> _tokens = new();

    private static string FormatParams(object[] args)
    {
        List<string> formattedArgs = new();
        foreach (object arg in args)
        {
            if (arg is string s) formattedArgs.Add($"'{s}'");
            else if (arg is bool b) formattedArgs.Add($"{b.ToString().ToLower()}");
            else formattedArgs.Add($"{arg}");
        }

        return string.Join(", ", formattedArgs);
    }

    /// <summary/>
    public Molang(string? raw = null)
    {
        if (raw is null) return;
        
        _tokens = raw.Split(" ").ToList();
    }
    
    /// <summary/>
    public override string ToString() => string.Join(' ', _tokens.ToArray());
    /// <summary/>
    public static implicit operator string(Molang molang) => molang.ToString();
    /// <summary/>
    public static implicit operator Molang(string raw) => new(raw);

    /// <summary>
    /// Adds the raw <paramref name="molang"/> to the builder
    /// </summary>
    public Molang Raw(string molang)
    {
        _tokens.Add(molang);
        return this;
    }
    
    /// <summary>
    /// <c>... == rightExpr</c>
    /// </summary>
    public Molang Eq(object? rightExpr = null)
    {
        string s = rightExpr == null ? "" : FormatParams([rightExpr]);
        
        _tokens.Add($"== {s}".Trim());
        return this;
    }
    /// <summary>
    /// <c>... != rightExpr</c>
    /// </summary>
    public Molang NotEq(object? rightExpr = null)
    {
        string s = rightExpr == null ? "" : FormatParams([rightExpr]);
        
        _tokens.Add($"!= {s}".Trim());
        return this;
    }
    
    /// <summary>
    /// <c>... && rightExpr</c>
    /// </summary>
    public Molang And()
    {
        _tokens.Add("&& ".Trim());
        return this;
    }
    /// <summary>
    /// <c>... || rightExpr</c>
    /// </summary>
    public Molang Or()
    {
        _tokens.Add("|| ".Trim());
        return this;
    }
    
    /// <summary>
    /// <c>... + rightExpr</c>
    /// </summary>
    public Molang Add(object? rightExpr = null)
    {
        string s = rightExpr == null ? "" : FormatParams([rightExpr]);
        
        _tokens.Add($"+ {s}".Trim());
        return this;
    }
    /// <summary>
    /// <c>... - rightExpr</c>
    /// </summary>
    public Molang Sub(object? rightExpr = null)
    {
        string s = rightExpr == null ? "" : FormatParams([rightExpr]);
        
        _tokens.Add($"- {s}".Trim());
        return this;
    }
    /// <summary>
    /// <c>... / rightExpr</c>
    /// </summary>
    public Molang Div(object? rightExpr = null)
    {
        string s = rightExpr == null ? "" : FormatParams([rightExpr]);
        
        _tokens.Add($"/ {s}".Trim());
        return this;
    }
    /// <summary>
    /// <c>... * rightExpr</c>
    /// </summary>
    public Molang Mul(object? rightExpr = null)
    {
        string s = rightExpr == null ? "" : FormatParams([rightExpr]);
        
        _tokens.Add($"* {s}".Trim());
        return this;
    }
    
    /// <summary>
    /// <c>... &lt; rightExpr</c>
    /// </summary>
    public Molang Lt(object? rightExpr = null)
    {
        string s = rightExpr == null ? "" : FormatParams([rightExpr]);
        
        _tokens.Add($"< {s}".Trim());
        return this;
    }
    /// <summary>
    /// <c>... > rightExpr</c>
    /// </summary>
    public Molang Gt(object? rightExpr = null)
    {
        string s = rightExpr == null ? "" : FormatParams([rightExpr]);
        
        _tokens.Add($"> {s}".Trim());
        return this;
    }
    /// <summary>
    /// <c>... &lt;= rightExpr</c>
    /// </summary>
    public Molang LtEq(object? rightExpr = null)
    {
        string s = rightExpr == null ? "" : FormatParams([rightExpr]);
        
        _tokens.Add($"<= {s}".Trim());
        return this;
    }
    /// <summary>
    /// <c>... >= rightExpr</c>
    /// </summary>
    public Molang GtEq(object? rightExpr = null)
    {
        string s = rightExpr == null ? "" : FormatParams([rightExpr]);
        
        _tokens.Add($">= {s}".Trim());
        return this;
    }
}

// queries: 315
// math funcs: 61
public partial class Molang
{
    #region queries
    /// <summary>Returns the height of the block immediately above the highest solid block at the input (x,z) position</summary>
    /// <returns><c>float</c></returns>
    public Molang AboveTopSolid(params object[] args)
    {
        _tokens.Add($"query.above_top_solid({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the number of actors rendered in the last frame.</summary>
    /// <returns><c>float</c></returns>
    public Molang ActorCount(params object[] args)
    {
        _tokens.Add($"query.actor_count({FormatParams(args)})");
        return this;
    }
    /// <summary>Requires at least 3 arguments. Evaluates the first argument, then returns 1.0 if all of the following arguments evaluate to the same value as the first. Otherwise it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang All(params object[] args)
    {
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"query.all({FormatParams(args)})");
        return this;
    }
    /// <summary>Only valid in an animation controller.  Returns 1.0 if all animations in the current animation controller state have played through at least once, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang AllAnimationsFinished(params object[] args)
    {
        _tokens.Add($"query.all_animations_finished({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns if the item or block has all of the tags specified.</summary>
    /// <returns><c>float</c></returns>
    public Molang AllTags(params object[] args)
    {
        _tokens.Add($"query.all_tags({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the anger level of the actor [0,n). On errors or if the actor has no anger level, returns 0. Available on the Server only.</summary>
    /// <returns><c>float</c></returns>
    public Molang AngerLevel(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.anger_level({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the time in seconds since the current animation started, else 0.0 if not called within an animation.</summary>
    /// <returns><c>float</c></returns>
    public Molang AnimTime(params object[] args)
    {
        _tokens.Add($"query.anim_time({FormatParams(args)})");
        return this;
    }
    /// <summary>Requires at least 3 arguments. Evaluates the first argument, then returns 1.0 if any of the following arguments evaluate to the same value as the first. Otherwise it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang Any(params object[] args)
    {
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"query.any({FormatParams(args)})");
        return this;
    }
    /// <summary>Only valid in an animation controller.  Returns 1.0 if any animation in the current animation controller state has played through at least once, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang AnyAnimationFinished(params object[] args)
    {
        _tokens.Add($"query.any_animation_finished({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns if the item or block has any of the tags specified.</summary>
    /// <returns><c>float</c></returns>
    public Molang AnyTag(params object[] args)
    {
        _tokens.Add($"query.any_tag({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if all of the arguments are within 0.000000 of each other, else 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang ApproxEq(params object[] args)
    {
        if (args.Length < 2) throw new ArgumentException("min argument count of 2");
        _tokens.Add($"query.approx_eq({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes the armor slot index as a parameter, and returns the color of the armor in the requested slot. The valid values for the armor slot index are 0 (head), 1 (chest), 2 (legs), 3 (feet) and 4 (body).</summary>
    /// <returns><c>float</c></returns>
    public Molang ArmorColorSlot(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 2) throw new ArgumentException("min argument count of 2");
        _tokens.Add($"query.armor_color_slot({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes the armor slot index as a parameter, and returns the damage value of the requested slot. The valid values for the armor slot index are 0 (head), 1 (chest), 2 (legs), 3 (feet) and 4 (body). Support for entities other than players may be limited, as the damage value is not always available on clients.</summary>
    /// <returns><c>float</c></returns>
    public Molang ArmorDamageSlot(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.armor_damage_slot({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes the armor slot index as a parameter, and returns the armor material type in the requested armor slot. The valid values for the armor slot index are 0 (head), 1 (chest), 2 (legs) and 3 (feet).</summary>
    /// <returns><c>float</c></returns>
    public Molang ArmorMaterialSlot(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.armor_material_slot({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes the armor slot index as a parameter, and returns the texture type of the requested slot. The valid values for the armor slot index are 0 (head), 1 (chest), 2 (legs), 3 (feet) and 4 (body).</summary>
    /// <returns><c>float</c></returns>
    public Molang ArmorTextureSlot(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.armor_texture_slot({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the time in *seconds* of the average frame time over the last 'n' frames.  If an argument is passed, it is assumed to be the number of frames in the past that you wish to query.  'query.average_frame_time' (or the equivalent 'query.average_frame_time(0)') will return the frame time of the frame before the current one.  'query.average_frame_time(1)' will return the average frame time of the previous two frames.  Currently we store the history of the last 0 frames, although note that this may change in the future.  Asking for more frames will result in only sampling the number of frames stored.</summary>
    /// <returns><c>float</c></returns>
    public Molang AverageFrameTime(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        _tokens.Add($"query.average_frame_time({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the duration of the mob's swing/attack animation, determined by the carried item and unmodified by effects applied on the mob. To access the swing/attack animation progress, use "variable.attack_time" instead.</summary>
    /// <returns><c>float</c></returns>
    public Molang BaseSwingDuration(params object[] args)
    {
        _tokens.Add($"query.base_swing_duration({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the block face for this (only valid for certain triggers such as placing blocks, or interacting with block) (Down=0.0, Up=1.0, North=2.0, South=3.0, West=4.0, East=5.0, Undefined=6.0).</summary>
    /// <returns><c>float</c></returns>
    public Molang BlockFace(params object[] args)
    {
        _tokens.Add($"query.block_face({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes a world-origin-relative position and one or more tag names, and returns either 0 or 1 based on if the block at that position has all of the tags provided.</summary>
    /// <returns><c>bool</c></returns>
    public Molang BlockHasAllTags(params object[] args)
    {
        if (args.Length < 4) throw new ArgumentException("min argument count of 4");
        _tokens.Add($"query.block_has_all_tags({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes a world-origin-relative position and one or more tag names, and returns either 0 or 1 based on if the block at that position has any of the tags provided.</summary>
    /// <returns><c>bool</c></returns>
    public Molang BlockHasAnyTag(params object[] args)
    {
        if (args.Length < 4) throw new ArgumentException("min argument count of 4");
        _tokens.Add($"query.block_has_any_tag({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes a block-relative position and one or more tag names, and returns either 0 or 1 based on if the block at that position has all of the tags provided.</summary>
    /// <returns><c>bool</c></returns>
    public Molang BlockNeighborHasAllTags(params object[] args)
    {
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.block_neighbor_has_all_tags({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes a block-relative position and one or more tag names, and returns either 0 or 1 based on if the block at that position has any of the tags provided.</summary>
    /// <returns><c>bool</c></returns>
    public Molang BlockNeighborHasAnyTag(params object[] args)
    {
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.block_neighbor_has_any_tag({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the value of the associated block's Block State.</summary>
    /// <returns><c>bool</c></returns>
    public Molang BlockProperty(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.block_property({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the value of the associated block's Block State.</summary>
    /// <returns><c>bool</c></returns>
    public Molang BlockState(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.block_state({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is blocking, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang Blocking(params object[] args)
    {
        _tokens.Add($"query.blocking({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the body pitch rotation if called on an actor, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang BodyXRotation(params object[] args)
    {
        _tokens.Add($"query.body_x_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the body yaw rotation if called on an actor, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang BodyYRotation(params object[] args)
    {
        _tokens.Add($"query.body_y_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the axis aligned bounding box of a bone as a struct with members '.min', '.max', along with '.x', '.y', and '.z' values for each.</summary>
    /// <returns><c>matrix</c></returns>
    public Molang BoneAabb(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.bone_aabb({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes the name of the bone as an argument. Returns the bone orientation (as a matrix) of the desired bone provided it exists in the queryable geometry of the mob, else this returns the identity matrix and throws a content error.</summary>
    /// <returns><c>matrix</c></returns>
    public Molang BoneOrientationMatrix(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.bone_orientation_matrix({FormatParams(args)})");
        return this;
    }
    /// <summary>TRS stands for Translate/Rotate/Scale.  Takes the name of the bone as an argument.  Returns the bone orientation matrix decomposed into the component translation/rotation/scale parts of the desired bone provided it exists in the queryable geometry of the mob, else this returns the identity matrix and throws a content error.  The returned value is returned as a variable of type 'struct' with members '.t', '.r', and '.s', each with members '.x', '.y', and '.z', and can be accessed as per the following example: v.my_variable = q.bone_orientation_trs('rightarm'); return v.my_variable.r.x;</summary>
    /// <returns><c>matrix</c></returns>
    public Molang BoneOrientationTrs(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.bone_orientation_trs({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the initial (from the .geo) pivot of a bone as a struct with members '.x', '.y', and '.z'.</summary>
    /// <returns><c>matrix</c></returns>
    public Molang BoneOrigin(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.bone_origin({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the initial (from the .geo) rotation of a bone as a struct with members '.x', '.y', and '.z' in degrees.</summary>
    /// <returns><c>matrix</c></returns>
    public Molang BoneRotation(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.bone_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes two distances (any order) and return a number from 0 to 1 based on the camera distance between the two ranges clamped to that range.  For example, 'query.camera_distance_range_lerp(10, 20)' will return 0 for any distance less than or equal to 10, 0.2 for a distance of 12, 0.5 for 15, and 1 for 20 or greater.  If you pass in (20, 10), a distance of 20 will return 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang CameraDistanceRangeLerp(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 2) throw new ArgumentException("min argument count of 2");
        _tokens.Add($"query.camera_distance_range_lerp({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the rotation of the camera.  Requires one argument representing the rotation axis you would like (0 for x, 1 for y).</summary>
    /// <returns><c>float</c></returns>
    public Molang CameraRotation(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.camera_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity can climb, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang CanClimb(params object[] args)
    {
        _tokens.Add($"query.can_climb({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity can damage nearby mobs, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang CanDamageNearbyMobs(params object[] args)
    {
        _tokens.Add($"query.can_damage_nearby_mobs({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity can dash, else it returns 0.0</summary>
    /// <returns><c>bool</c></returns>
    public Molang CanDash(params object[] args)
    {
        _tokens.Add($"query.can_dash({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity can fly, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang CanFly(params object[] args)
    {
        _tokens.Add($"query.can_fly({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity can power jump, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang CanPowerJump(params object[] args)
    {
        _tokens.Add($"query.can_power_jump({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity can swim, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang CanSwim(params object[] args)
    {
        _tokens.Add($"query.can_swim({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity can walk, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang CanWalk(params object[] args)
    {
        _tokens.Add($"query.can_walk({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns value between 0.0 and 1.0 with 0.0 meaning cape is fully down and 1.0 is cape is fully up.</summary>
    /// <returns><c>float</c></returns>
    public Molang CapeFlapAmount(params object[] args)
    {
        _tokens.Add($"query.cape_flap_amount({FormatParams(args)})");
        return this;
    }
    /// <summary>DEPRECATED (please use query.block_face instead) Returns the block face for this (only valid for on_placed_by_player trigger) (Down=0.0, Up=1.0, North=2.0, South=3.0, West=4.0, East=5.0, Undefined=6.0).</summary>
    /// <returns><c>float</c></returns>
    public Molang CardinalBlockFacePlacedOn(params object[] args)
    {
        _tokens.Add($"query.cardinal_block_face_placed_on({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the current facing of the player (Down=0.0, Up=1.0, North=2.0, South=3.0, West=4.0, East=5.0, Undefined=6.0).</summary>
    /// <returns><c>float</c></returns>
    public Molang CardinalFacing(params object[] args)
    {
        _tokens.Add($"query.cardinal_facing({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the current facing of the player ignoring up/down part of the direction (North=2.0, South=3.0, West=4.0, East=5.0, Undefined=6.0).</summary>
    /// <returns><c>float</c></returns>
    public Molang CardinalFacing2d(params object[] args)
    {
        _tokens.Add($"query.cardinal_facing_2d({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the current facing of the player (Down=0.0, Up=1.0, North=2.0, South=3.0, West=4.0, East=5.0, Undefined=6.0).</summary>
    /// <returns><c>float</c></returns>
    public Molang CardinalPlayerFacing(params object[] args)
    {
        _tokens.Add($"query.cardinal_player_facing({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the max render distance in chunks of the current client. Available on the Client (Resource Packs) only.</summary>
    /// <returns><c>float</c></returns>
    public Molang ClientMaxRenderDistance(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.client_max_render_distance({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns a number representing the client RAM memory tier, 0 = 'SuperLow', 1 = 'Low', 2 = 'Mid', 3 = 'High', or 4 = 'SuperHigh'. Available on the Client (Resource Packs) only.</summary>
    /// <returns><c>float</c></returns>
    public Molang ClientMemoryTier(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.client_memory_tier({FormatParams(args)})");
        return this;
    }
    /// <summary>Combines any valid entity references from all arguments into a single array.  Note that order is not preserved, and duplicates and invalid values are removed.</summary>
    /// <returns><c>actor_array</c></returns>
    public Molang CombineEntities(params object[] args)
    {
        _tokens.Add($"query.combine_entities({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the total cooldown time in seconds for the item held or worn by the specified equipment slot name (and if required second numerical slot id), otherwise returns 0. Uses the same name and id that the replaceitem command takes when querying entities.</summary>
    /// <returns><c>float</c></returns>
    public Molang CooldownTime(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.cooldown_time({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the cooldown time remaining in seconds for specified cooldown type or the item held or worn by the specified equipment slot name (and if required second numerical slot id), otherwise returns 0. Uses the same name and id that the replaceitem command takes when querying entities. Returns highest cooldown if no parameters are supplied.</summary>
    /// <returns><c>float</c></returns>
    public Molang CooldownTimeRemaining(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        _tokens.Add($"query.cooldown_time_remaining({FormatParams(args)})");
        return this;
    }
    /// <summary>Counts the number of things passed to it (arrays are counted as the number of elements they contain; non-arrays count as 1).</summary>
    /// <returns><c>float</c></returns>
    public Molang Count(params object[] args)
    {
        _tokens.Add($"query.count({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the squish value for the current entity, or 0.0 if this doesn't make sense.</summary>
    /// <returns><c>float</c></returns>
    public Molang CurrentSquishValue(params object[] args)
    {
        _tokens.Add($"query.current_squish_value({FormatParams(args)})");
        return this;
    }
    /// <summary>DEPRECATED. DO NOT USE AFTER 1.20.40. Please see camel.entity.json script.pre_animation for example of how to now process dash cooldown. Returns dash cooldown progress if the entity can dash, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang DashCooldownProgress(params object[] args)
    {
        _tokens.Add($"query.dash_cooldown_progress({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the day of the current level.</summary>
    /// <returns><c>float</c></returns>
    public Molang Day(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.day({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the elapsed ticks since the mob started dying.</summary>
    /// <returns><c>float</c></returns>
    public Molang DeathTicks(params object[] args)
    {
        _tokens.Add($"query.death_ticks({FormatParams(args)})");
        return this;
    }
    /// <summary>debug log a value to the output debug window for builds that have one</summary>
    /// <returns><c>float</c></returns>
    public Molang DebugOutput(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.debug_output({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the time in seconds since the previous frame.</summary>
    /// <returns><c>float</c></returns>
    public Molang DeltaTime(params object[] args)
    {
        _tokens.Add($"query.delta_time({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the distance of the root of this actor or particle emitter from the camera.</summary>
    /// <returns><c>float</c></returns>
    public Molang DistanceFromCamera(params object[] args)
    {
        _tokens.Add($"query.distance_from_camera({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the total number of active emitters of the callee's particle effect type.</summary>
    /// <returns><c>float</c></returns>
    public Molang EffectEmitterCount(params object[] args)
    {
        _tokens.Add($"query.effect_emitter_count({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the total number of active particles of the callee's particle effect type.</summary>
    /// <returns><c>float</c></returns>
    public Molang EffectParticleCount(params object[] args)
    {
        _tokens.Add($"query.effect_particle_count({FormatParams(args)})");
        return this;
    }
    /// <summary>Compares the biome the entity is standing in with one or more tag names, and returns either 0 or 1 based on if all of the tag names match. Only supported in resource packs (client-side).</summary>
    /// <returns><c>bool</c></returns>
    public Molang EntityBiomeHasAllTags(params object[] args)
    {
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.entity_biome_has_all_tags({FormatParams(args)})");
        return this;
    }
    /// <summary>Compares the biome the entity is standing in with one or more identifier names, and returns either 0 or 1 based on if any of the identifier names match. Only supported in resource packs (client-side).</summary>
    /// <returns><c>bool</c></returns>
    public Molang EntityBiomeHasAnyIdentifier(params object[] args)
    {
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.entity_biome_has_any_identifier({FormatParams(args)})");
        return this;
    }
    /// <summary>Compares the biome the entity is standing in with one or more tag names, and returns either 0 or 1 based on if any of the tag names match. Only supported in resource packs (client-side).</summary>
    /// <returns><c>bool</c></returns>
    public Molang EntityBiomeHasAnyTags(params object[] args)
    {
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.entity_biome_has_any_tags({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the number of equipped armor pieces for an actor from 0 to 5, not counting items held in hands. (To query for hand slots, use query.is_item_equipped or query.is_item_name_any).</summary>
    /// <returns><c>float</c></returns>
    public Molang EquipmentCount(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.equipment_count({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes a slot name followed by any tag you want to check for in the form of 'tag_name' and returns 1 if all of the tags are on that equipped item, 0 otherwise.</summary>
    /// <returns><c>bool</c></returns>
    public Molang EquippedItemAllTags(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        _tokens.Add($"query.equipped_item_all_tags({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes a slot name followed by any tag you want to check for in the form of 'tag_name' and returns 0 if none of the tags are on that equipped item or 1 if at least 1 tag exists.</summary>
    /// <returns><c>bool</c></returns>
    public Molang EquippedItemAnyTag(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        _tokens.Add($"query.equipped_item_any_tag({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes the desired hand slot as a parameter (0 or 'main_hand' for main hand, 1 or 'off_hand' for off hand), and returns whether the item is an attachable or not.</summary>
    /// <returns><c>actor_single</c></returns>
    public Molang EquippedItemIsAttachable(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.equipped_item_is_attachable({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the X eye rotation of the entity if it makes sense, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang EyeTargetXRotation(params object[] args)
    {
        _tokens.Add($"query.eye_target_x_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the Y eye rotation of the entity if it makes sense, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang EyeTargetYRotation(params object[] args)
    {
        _tokens.Add($"query.eye_target_y_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is attacking from range (i.e. minecraft:behavior.ranged_attack), else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang FacingTargetToRangeAttack(params object[] args)
    {
        _tokens.Add($"query.facing_target_to_range_attack({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the ratio (from 0 to 1) of how much between AI ticks this frame is being rendered.</summary>
    /// <returns><c>float</c></returns>
    public Molang FrameAlpha(params object[] args)
    {
        _tokens.Add($"query.frame_alpha({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the remaining fuse time of the entity. Returns -1 if the entity doesn't have a "minecraft:explode" component.</summary>
    /// <returns><c>float</c></returns>
    public Molang FuseTime(params object[] args)
    {
        _tokens.Add($"query.fuse_time({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the integer id of an actor by its string name.</summary>
    /// <returns><c>float</c></returns>
    public Molang GetActorInfoId(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.get_actor_info_id({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the current texture of the item</summary>
    /// <returns><c>float</c></returns>
    public Molang GetAnimationFrame(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.get_animation_frame({FormatParams(args)})");
        return this;
    }
    /// <summary>Gets specified axis of the specified bone orientation pivot.</summary>
    /// <returns><c>float</c></returns>
    public Molang GetDefaultBonePivot(params object[] args)
    {
        _tokens.Add($"query.get_default_bone_pivot({FormatParams(args)})");
        return this;
    }
    /// <summary>DEPRECATED (Use query.is_item_name_any instead if possible so names can be changed later without breaking content.) Takes one optional hand slot as a parameter (0 or 'main_hand' for main hand, 1 or 'off_hand' for off hand), and a second parameter (0=default) if you would like the equipped item or any non-zero number for the currently rendered item, and returns the name of the item in the requested slot (defaulting to the main hand if no parameter is supplied) if there is one, otherwise returns ''.</summary>
    /// <returns><c>hash_type_64</c></returns>
    public Molang GetEquippedItemName(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        _tokens.Add($"query.get_equipped_item_name({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns a value in range [0.0, 1.0] based on the level seed.</summary>
    /// <returns><c>float</c></returns>
    public Molang GetLevelSeedBasedFraction(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.get_level_seed_based_fraction({FormatParams(args)})");
        return this;
    }
    /// <summary>Gets specified axis of the specified locator offset.</summary>
    /// <returns><c>float</c></returns>
    public Molang GetLocatorOffset(params object[] args)
    {
        _tokens.Add($"query.get_locator_offset({FormatParams(args)})");
        return this;
    }
    /// <summary>DEPRECATED (Use query.is_name_any instead if possible so names can be changed later without breaking content.)Get the name of the mob if there is one, otherwise return ''.</summary>
    /// <returns><c>hash_type_64</c></returns>
    public Molang GetName(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        _tokens.Add($"query.get_name({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns value of Pack Setting slider, parameter is name of slider. Available on the Client (Resource Packs) only.</summary>
    /// <returns><c>float</c></returns>
    public Molang GetPackSetting(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.get_pack_setting({FormatParams(args)})");
        return this;
    }
    /// <summary>Gets specified axis of the specified locator offset of the root model.</summary>
    /// <returns><c>float</c></returns>
    public Molang GetRootLocatorOffset(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 2) throw new ArgumentException("min argument count of 2");
        _tokens.Add($"query.get_root_locator_offset({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes in one or more arguments ('simple', 'fancy', 'deferred', 'raytraced'). If the graphics mode of the client matches any of the arguments, return 1.0. Available on the Client (Resource Packs) only.</summary>
    /// <returns><c>float</c></returns>
    public Molang GraphicsModeIsAny(params object[] args)
    {
        if (args.Length > 255) throw new ArgumentException("max argument count of 255");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.graphics_mode_is_any({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the ground speed of the entity in meters/second.</summary>
    /// <returns><c>float</c></returns>
    public Molang GroundSpeed(params object[] args)
    {
        _tokens.Add($"query.ground_speed({FormatParams(args)})");
        return this;
    }
    /// <summary>Usable only in behavior packs when determining the default value for an entity's Property. Requires one string argument. If the entity is being loaded from data that was last saved with a component_group with the specified name, returns 1.0, otherwise returns 0.0. The purpose of this query is to allow entity definitions to change and still be able to load the correct state of entities. </summary>
    /// <returns><c>bool</c></returns>
    public Molang HadComponentGroup(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.had_component_group({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1 if the entity has any of the specified families, else 0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang HasAnyFamily(params object[] args)
    {
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.has_any_family({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns whether or not the entity is currently leashing other entities of the designated types.</summary>
    /// <returns><c>bool</c></returns>
    public Molang HasAnyLeashedEntityOfType(params object[] args)
    {
        _tokens.Add($"query.has_any_leashed_entity_of_type({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes the armor slot index as a parameter, and returns 1.0 if the entity has armor in the requested slot, else it returns 0.0. The valid values for the armor slot index are 0 (head), 1 (chest), 2 (legs) and 3 (feet).</summary>
    /// <returns><c>float</c></returns>
    public Molang HasArmorSlot(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.has_armor_slot({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns whether or not a Block Placement Target has a specific biome tag</summary>
    /// <returns><c>bool</c></returns>
    public Molang HasBiomeTag(params object[] args)
    {
        _tokens.Add($"query.has_biome_tag({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the associated block has the given block state or 0.0 if not.</summary>
    /// <returns><c>bool</c></returns>
    public Molang HasBlockProperty(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.has_block_property({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the associated block has the given block state or 0.0 if not.</summary>
    /// <returns><c>bool</c></returns>
    public Molang HasBlockState(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.has_block_state({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the player has a cape, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang HasCape(params object[] args)
    {
        _tokens.Add($"query.has_cape({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity has collisions enabled, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang HasCollision(params object[] args)
    {
        _tokens.Add($"query.has_collision({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity has cooldown on its dash, else it returns 0.0</summary>
    /// <returns><c>bool</c></returns>
    public Molang HasDashCooldown(params object[] args)
    {
        _tokens.Add($"query.has_dash_cooldown({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is affected by gravity, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang HasGravity(params object[] args)
    {
        _tokens.Add($"query.has_gravity({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns boolean whether an Actor has an item in their head armor slot or not, or false if no actor in current context</summary>
    /// <returns><c>bool</c></returns>
    public Molang HasHeadGear(params object[] args)
    {
        _tokens.Add($"query.has_head_gear({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns true if the entity has an owner ID else it returns false</summary>
    /// <returns><c>bool</c></returns>
    public Molang HasOwner(params object[] args)
    {
        _tokens.Add($"query.has_owner({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1 if the entity has a player riding it in any seat, else it returns 0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang HasPlayerRider(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.has_player_rider({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes one argument: the name of the property on the Actor. Returns 1.0 if a property with the given name exists, 0 otherwise.</summary>
    /// <returns><c>float</c></returns>
    public Molang HasProperty(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.has_property({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity has a rider, else it returns 0.0</summary>
    /// <returns><c>bool</c></returns>
    public Molang HasRider(params object[] args)
    {
        _tokens.Add($"query.has_rider({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity has a target, else it returns 0.0</summary>
    /// <returns><c>bool</c></returns>
    public Molang HasTarget(params object[] args)
    {
        _tokens.Add($"query.has_target({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the roll angle of the head of the entity if it makes sense, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang HeadRollAngle(params object[] args)
    {
        _tokens.Add($"query.head_roll_angle({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes one argument as a parameter.  Returns the nth head x rotation of the entity if it makes sense, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang HeadXRotation(params object[] args)
    {
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.head_x_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes one argument as a parameter.  Returns the nth head y rotation of the entity if it makes sense, else it returns 0.0. Horses, zombie horses, skeleton horses, donkeys and mules require a second parameter that clamps rotation in degrees.</summary>
    /// <returns><c>float</c></returns>
    public Molang HeadYRotation(params object[] args)
    {
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.head_y_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the health of the entity, or 0.0 if it doesn't make sense to call on this entity.</summary>
    /// <returns><c>float</c></returns>
    public Molang Health(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.health({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the heartbeat interval of the actor in seconds. Returns 0 when the actor has no heartbeat.</summary>
    /// <returns><c>float</c></returns>
    public Molang HeartbeatInterval(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.heartbeat_interval({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the heartbeat phase of the actor. 0.0 if at start of current heartbeat, 1.0 if at the end. Returns 0 on errors or when the actor has no heartbeat. Available on the Client (Resource Packs) only.</summary>
    /// <returns><c>float</c></returns>
    public Molang HeartbeatPhase(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.heartbeat_phase({FormatParams(args)})");
        return this;
    }
    /// <summary>Queries Height Map</summary>
    /// <returns><c>float</c></returns>
    public Molang Heightmap(params object[] args)
    {
        _tokens.Add($"query.heightmap({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the hurt direction for the actor, otherwise returns 0.</summary>
    /// <returns><c>float</c></returns>
    public Molang HurtDirection(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.hurt_direction({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the hurt time for the actor, otherwise returns 0.</summary>
    /// <returns><c>float</c></returns>
    public Molang HurtTime(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.hurt_time({FormatParams(args)})");
        return this;
    }
    /// <summary>Requires 3 numerical arguments: some value, a minimum, and a maximum. If the first argument is between the minimum and maximum (inclusive), returns 1.0. Otherwise returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang InRange(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"query.in_range({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the number of ticks of invulnerability the entity has left if it makes sense, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang InvulnerableTicks(params object[] args)
    {
        _tokens.Add($"query.invulnerable_ticks({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is admiring, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsAdmiring(params object[] args)
    {
        _tokens.Add($"query.is_admiring({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is alive, and 0.0 if it's dead.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsAlive(params object[] args)
    {
        _tokens.Add($"query.is_alive({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is angry, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsAngry(params object[] args)
    {
        _tokens.Add($"query.is_angry({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is attached to another entity (such as being held or worn), else it will return 0.0. Available only with resource packs.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsAttached(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.is_attached({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the actor is attached to an entity, else it will return 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsAttachedToEntity(params object[] args)
    {
        _tokens.Add($"query.is_attached_to_entity({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is fleeing from a block, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsAvoidingBlock(params object[] args)
    {
        _tokens.Add($"query.is_avoiding_block({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is fleeing from mobs, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsAvoidingMobs(params object[] args)
    {
        _tokens.Add($"query.is_avoiding_mobs({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is a baby, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsBaby(params object[] args)
    {
        _tokens.Add($"query.is_baby({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is breathing, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsBreathing(params object[] args)
    {
        _tokens.Add($"query.is_breathing({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity has been bribed, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsBribed(params object[] args)
    {
        _tokens.Add($"query.is_bribed({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is carrying a block, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsCarryingBlock(params object[] args)
    {
        _tokens.Add($"query.is_carrying_block({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is casting, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsCasting(params object[] args)
    {
        _tokens.Add($"query.is_casting({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is celebrating, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsCelebrating(params object[] args)
    {
        _tokens.Add($"query.is_celebrating({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is doing a special celebration, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsCelebratingSpecial(params object[] args)
    {
        _tokens.Add($"query.is_celebrating_special({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is charged, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsCharged(params object[] args)
    {
        _tokens.Add($"query.is_charged({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is charging, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsCharging(params object[] args)
    {
        _tokens.Add($"query.is_charging({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity has chests attached to it, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsChested(params object[] args)
    {
        _tokens.Add($"query.is_chested({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the specified held or worn item has the specified cooldown category, otherwise returns 0.0. First argument is the cooldown name to check for, second argument is the equipment slot name, and if required third argument is the numerical slot id. For second and third arguments, uses the same name and id that the replaceitem command takes when querying entities.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsCooldownCategory(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.is_cooldown_category({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is crawling, else it returns 0.0</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsCrawling(params object[] args)
    {
        _tokens.Add($"query.is_crawling({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is critical, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsCritical(params object[] args)
    {
        _tokens.Add($"query.is_critical({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is croaking, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsCroaking(params object[] args)
    {
        _tokens.Add($"query.is_croaking({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is dancing, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsDancing(params object[] args)
    {
        _tokens.Add($"query.is_dancing({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is attacking using the delayed attack, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsDelayedAttacking(params object[] args)
    {
        _tokens.Add($"query.is_delayed_attacking({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is digging, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsDigging(params object[] args)
    {
        _tokens.Add($"query.is_digging({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is eating, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsEating(params object[] args)
    {
        _tokens.Add($"query.is_eating({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is eating a mob, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsEatingMob(params object[] args)
    {
        _tokens.Add($"query.is_eating_mob({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is an elder version of it, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsElder(params object[] args)
    {
        _tokens.Add($"query.is_elder({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is emerging, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsEmerging(params object[] args)
    {
        _tokens.Add($"query.is_emerging({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is emoting, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsEmoting(params object[] args)
    {
        _tokens.Add($"query.is_emoting({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is enchanted, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsEnchanted(params object[] args)
    {
        _tokens.Add($"query.is_enchanted({FormatParams(args)})");
        return this;
    }
    /// <summary>DEPRECATED after 1.20.40. Returns 1.0 if behavior.timer_flag_2 is running, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsFeelingHappy(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.is_feeling_happy({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is immune to fire, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsFireImmune(params object[] args)
    {
        _tokens.Add($"query.is_fire_immune({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is being rendered in first person mode, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsFirstPerson(params object[] args)
    {
        _tokens.Add($"query.is_first_person({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if an entity is a ghost, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsGhost(params object[] args)
    {
        _tokens.Add($"query.is_ghost({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is gliding, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsGliding(params object[] args)
    {
        _tokens.Add($"query.is_gliding({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is grazing, or 0.0 if not.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsGrazing(params object[] args)
    {
        _tokens.Add($"query.is_grazing({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is idling, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsIdling(params object[] args)
    {
        _tokens.Add($"query.is_idling({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is ignited, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsIgnited(params object[] args)
    {
        _tokens.Add($"query.is_ignited({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is an illager captain, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsIllagerCaptain(params object[] args)
    {
        _tokens.Add($"query.is_illager_captain({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is in contact with any water (water, rain, splash water bottle), else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsInContactWithWater(params object[] args)
    {
        _tokens.Add($"query.is_in_contact_with_water({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is in lava, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsInLava(params object[] args)
    {
        _tokens.Add($"query.is_in_lava({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is in love, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsInLove(params object[] args)
    {
        _tokens.Add($"query.is_in_love({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is rendered as part of the UI, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsInUi(params object[] args)
    {
        _tokens.Add($"query.is_in_ui({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is in water, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsInWater(params object[] args)
    {
        _tokens.Add($"query.is_in_water({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is in water or rain, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsInWaterOrRain(params object[] args)
    {
        _tokens.Add($"query.is_in_water_or_rain({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is interested, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsInterested(params object[] args)
    {
        _tokens.Add($"query.is_interested({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is invisible, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsInvisible(params object[] args)
    {
        _tokens.Add($"query.is_invisible({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes one optional hand slot as a parameter (0 or 'main_hand' for main hand, 1 or 'off_hand' for off hand), and returns 1.0 if there is an item in the requested slot (defaulting to the main hand if no parameter is supplied), otherwise returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsItemEquipped(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        _tokens.Add($"query.is_item_equipped({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes an equipment slot name (see the replaceitem command) and an optional slot index value. (The slot index is required for slot names that have multiple slots, for example 'slot.hotbar'.) After that, takes one or more full name (with 'namespace:') strings to check for. Returns 1.0 if an item in the specified slot has any of the specified names, otherwise returns 0.0. An empty string '' can be specified to check for an empty slot. Note that querying slot.enderchest, slot.saddle, slot.armor, or slot.chest will only work in behavior packs. A preferred query to query.get_equipped_item_name, as it can be adjusted by Mojang to avoid breaking content if names are changed.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsItemNameAny(params object[] args)
    {
        if (args.Length < 2) throw new ArgumentException("min argument count of 2");
        _tokens.Add($"query.is_item_name_any({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is doing a jump goal jump, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsJumpGoalJumping(params object[] args)
    {
        _tokens.Add($"query.is_jump_goal_jumping({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is jumping, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsJumping(params object[] args)
    {
        _tokens.Add($"query.is_jumping({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is laying down, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsLayingDown(params object[] args)
    {
        _tokens.Add($"query.is_laying_down({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is laying an egg, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsLayingEgg(params object[] args)
    {
        _tokens.Add($"query.is_laying_egg({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is leashed to something, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsLeashed(params object[] args)
    {
        _tokens.Add($"query.is_leashed({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is levitating, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsLevitating(params object[] args)
    {
        _tokens.Add($"query.is_levitating({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is lingering, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsLingering(params object[] args)
    {
        _tokens.Add($"query.is_lingering({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes no arguments. Returns 1.0 if the entity is the local player for the current game window, else it returns 0.0. In splitscreen returns 0.0 for the other local players for other views. Always returns 0.0 if used in a behavior pack.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsLocalPlayer(params object[] args)
    {
        _tokens.Add($"query.is_local_player({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is moving, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsMoving(params object[] args)
    {
        _tokens.Add($"query.is_moving({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes one or more arguments. If the entity's name is any of the specified string values, returns 1.0. Otherwise returns 0.0. A preferred query to query.get_name, as it can be adjusted by Mojang to avoid breaking content if names are changed.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsNameAny(params object[] args)
    {
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.is_name_any({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is on fire, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsOnFire(params object[] args)
    {
        _tokens.Add($"query.is_on_fire({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is on the ground, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsOnGround(params object[] args)
    {
        _tokens.Add($"query.is_on_ground({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is on fire, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsOnfire(params object[] args)
    {
        _tokens.Add($"query.is_onfire({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is orphaned, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsOrphaned(params object[] args)
    {
        _tokens.Add($"query.is_orphaned({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes one or more arguments. Returns whether the root actor identifier is any of the specified strings. A preferred query to query.owner_identifier, as it can be adjusted by Mojang to avoid breaking content if names are changed.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsOwnerIdentifierAny(params object[] args)
    {
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.is_owner_identifier_any({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the Pack Setting toggle is enabled, parameter is name of toggle. Available on the Client (Resource Packs) only.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsPackSettingEnabled(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.is_pack_setting_enabled({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the Pack Setting dropdown (first parameter) matches the string value of the second parameter (selection). Available on the Client (Resource Packs) only.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsPackSettingSelected(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 2) throw new ArgumentException("min argument count of 2");
        _tokens.Add($"query.is_pack_setting_selected({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the player has a persona or premium skin, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsPersonaOrPremiumSkin(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.is_persona_or_premium_skin({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is playing dead, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsPlayingDead(params object[] args)
    {
        _tokens.Add($"query.is_playing_dead({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is powered, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsPowered(params object[] args)
    {
        _tokens.Add($"query.is_powered({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is pregnant, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsPregnant(params object[] args)
    {
        _tokens.Add($"query.is_pregnant({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is using a ram attack, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsRamAttacking(params object[] args)
    {
        _tokens.Add($"query.is_ram_attacking({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is resting, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsResting(params object[] args)
    {
        _tokens.Add($"query.is_resting({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is riding, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsRiding(params object[] args)
    {
        _tokens.Add($"query.is_riding({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns whether or not the entity is currently riding an entity of any of the designated types.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsRidingAnyEntityOfType(params object[] args)
    {
        _tokens.Add($"query.is_riding_any_entity_of_type({FormatParams(args)})");
        return this;
    }
    /// <summary>DEPRECATED after 1.20.40. Returns 1.0 if behavior.timer_flag_2 is running, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsRising(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.is_rising({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is currently roaring, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsRoaring(params object[] args)
    {
        _tokens.Add($"query.is_roaring({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is rolling, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsRolling(params object[] args)
    {
        _tokens.Add($"query.is_rolling({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity has a saddle, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsSaddled(params object[] args)
    {
        _tokens.Add($"query.is_saddled({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is scared, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsScared(params object[] args)
    {
        _tokens.Add($"query.is_scared({FormatParams(args)})");
        return this;
    }
    /// <summary>DEPRECATED after 1.20.40. Returns 1.0 if behavior.timer_flag_1 is running, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsScenting(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.is_scenting({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is searching, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsSearching(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.is_searching({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns true if the player has selected an item in the inventory, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsSelectedItem(params object[] args)
    {
        _tokens.Add($"query.is_selected_item({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is casting, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsShaking(params object[] args)
    {
        _tokens.Add($"query.is_shaking({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is shaking water off, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang IsShakingWetness(params object[] args)
    {
        _tokens.Add($"query.is_shaking_wetness({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is able to be sheared and is sheared, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsSheared(params object[] args)
    {
        _tokens.Add($"query.is_sheared({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0f if the entity has an active powered shield if it makes sense, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsShieldPowered(params object[] args)
    {
        _tokens.Add($"query.is_shield_powered({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is silent, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsSilent(params object[] args)
    {
        _tokens.Add($"query.is_silent({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is sitting, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsSitting(params object[] args)
    {
        _tokens.Add($"query.is_sitting({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is sleeping, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsSleeping(params object[] args)
    {
        _tokens.Add($"query.is_sleeping({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is sneaking, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsSneaking(params object[] args)
    {
        _tokens.Add($"query.is_sneaking({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is sneezing, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsSneezing(params object[] args)
    {
        _tokens.Add($"query.is_sneezing({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is sniffing, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsSniffing(params object[] args)
    {
        _tokens.Add($"query.is_sniffing({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is using sonic boom, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsSonicBoom(params object[] args)
    {
        _tokens.Add($"query.is_sonic_boom({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is spectator, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsSpectator(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.is_spectator({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is sprinting, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsSprinting(params object[] args)
    {
        _tokens.Add($"query.is_sprinting({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is stackable, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsStackable(params object[] args)
    {
        _tokens.Add($"query.is_stackable({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is stalking, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsStalking(params object[] args)
    {
        _tokens.Add($"query.is_stalking({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is standing, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsStanding(params object[] args)
    {
        _tokens.Add($"query.is_standing({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is currently stunned, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsStunned(params object[] args)
    {
        _tokens.Add($"query.is_stunned({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is swimming, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsSwimming(params object[] args)
    {
        _tokens.Add($"query.is_swimming({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is tamed, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsTamed(params object[] args)
    {
        _tokens.Add($"query.is_tamed({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is transforming, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsTransforming(params object[] args)
    {
        _tokens.Add($"query.is_transforming({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is using an item, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsUsingItem(params object[] args)
    {
        _tokens.Add($"query.is_using_item({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is climbing a wall, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang IsWallClimbing(params object[] args)
    {
        _tokens.Add($"query.is_wall_climbing({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the amount of time an item has been in use in seconds up to the maximum duration, else 0.0 if it doesn't make sense.</summary>
    /// <returns><c>float</c></returns>
    public Molang ItemInUseDuration(params object[] args)
    {
        _tokens.Add($"query.item_in_use_duration({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes one optional hand slot as a parameter (0 or 'main_hand' for main hand, 1 or 'off_hand' for off hand), and returns 1.0 if the item is charged in the requested slot (defaulting to the main hand if no parameter is supplied), otherwise returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang ItemIsCharged(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        _tokens.Add($"query.item_is_charged({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the maximum amount of time the item can be used, else 0.0 if it doesn't make sense.</summary>
    /// <returns><c>float</c></returns>
    public Molang ItemMaxUseDuration(params object[] args)
    {
        _tokens.Add($"query.item_max_use_duration({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the amount of time an item has left to use, else 0.0 if it doesn't make sense. Item queried is specified by the slot name 'main_hand' or 'off_hand'. Time remaining is normalized using the normalization value, only if one is given, else it is returned in seconds.</summary>
    /// <returns><c>float</c></returns>
    public Molang ItemRemainingUseDuration(params object[] args)
    {
        _tokens.Add($"query.item_remaining_use_duration({FormatParams(args)})");
        return this;
    }
    /// <summary>query.item_slot_to_bone_name requires one parameter: the name of the equipment slot.  This function returns the name of the bone this entity has mapped to that slot.</summary>
    /// <returns><c>hash_type_64</c></returns>
    public Molang ItemSlotToBoneName(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.item_slot_to_bone_name({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the ratio between the previous and next key frames.</summary>
    /// <returns><c>float</c></returns>
    public Molang KeyFrameLerpTime(params object[] args)
    {
        _tokens.Add($"query.key_frame_lerp_time({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the "max_duration" value of "damage_conditions" from the main-hand item's "minecraft:kinetic_weapon" component, or 0 if the component is not present.</summary>
    /// <returns><c>float</c></returns>
    public Molang KineticWeaponDamageDuration(params object[] args)
    {
        _tokens.Add($"query.kinetic_weapon_damage_duration({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the "delay" value from the main-hand item's "minecraft:kinetic_weapon" component, or 0 if the component is not present.</summary>
    /// <returns><c>float</c></returns>
    public Molang KineticWeaponDelay(params object[] args)
    {
        _tokens.Add($"query.kinetic_weapon_delay({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the "max_duration" value of "dismount_conditions" from the main-hand item's "minecraft:kinetic_weapon" component, or 0 if the component is not present.</summary>
    /// <returns><c>float</c></returns>
    public Molang KineticWeaponDismountDuration(params object[] args)
    {
        _tokens.Add($"query.kinetic_weapon_dismount_duration({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the "max_duration" value of "knockback_conditions" from the main-hand item's "minecraft:kinetic_weapon" component, or 0 if the component is not present.</summary>
    /// <returns><c>float</c></returns>
    public Molang KineticWeaponKnockbackDuration(params object[] args)
    {
        _tokens.Add($"query.kinetic_weapon_knockback_duration({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the time in *seconds* of the last frame.  If an argument is passed, it is assumed to be the number of frames in the past that you wish to query.  'query.last_frame_time' (or the equivalent 'query.last_frame_time(0)') will return the frame time of the frame before the current one.  'query.last_frame_time(1)' will return the frame time of two frames ago.  Currently we store the history of the last 0 frames, although note that this may change in the future.  Passing an index more than the available data will return the oldest frame stored.</summary>
    /// <returns><c>float</c></returns>
    public Molang LastFrameTime(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        _tokens.Add($"query.last_frame_time({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity was last hit by the player, else it returns 0.0. If called by the client always returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang LastHitByPlayer(params object[] args)
    {
        _tokens.Add($"query.last_hit_by_player({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes one or more arguments ('keyboard_and_mouse', 'touch', or 'gamepad'). If the last input used is any of the specified string values, returns 1.0. Otherwise returns 0.0. Available on the Client (Resource Packs) only.</summary>
    /// <returns><c>float</c></returns>
    public Molang LastInputModeIsAny(params object[] args)
    {
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.last_input_mode_is_any({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the number of entities for which this entity is the leash holder.</summary>
    /// <returns><c>float</c></returns>
    public Molang LeashedEntityCount(params object[] args)
    {
        _tokens.Add($"query.leashed_entity_count({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the lie down amount for the entity.</summary>
    /// <returns><c>float</c></returns>
    public Molang LieAmount(params object[] args)
    {
        _tokens.Add($"query.lie_amount({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the limited life span of an entity, or 0.0 if it lives forever</summary>
    /// <returns><c>float</c></returns>
    public Molang LifeSpan(params object[] args)
    {
        _tokens.Add($"query.life_span({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the time in seconds since the current animation started, else 0.0 if not called within an animation.</summary>
    /// <returns><c>float</c></returns>
    public Molang LifeTime(params object[] args)
    {
        _tokens.Add($"query.life_time({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes an array of distances and returns the zero - based index of which range the actor is in based on distance from the camera. For example, 'query.lod_index(10, 20, 30)' will return 0, 1, or 2 based on whether the mob is less than 10, 20, or 30 units away from the camera, or it will return 3 if it is greater than 30.</summary>
    /// <returns><c>float</c></returns>
    public Molang LodIndex(params object[] args)
    {
        _tokens.Add($"query.lod_index({FormatParams(args)})");
        return this;
    }
    /// <summary>debug log a value to the content log</summary>
    /// <returns><c>float</c></returns>
    public Molang Log(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.log({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the use time maximum duration for the main hand item if it makes sense, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang MainHandItemMaxDuration(params object[] args)
    {
        _tokens.Add($"query.main_hand_item_max_duration({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the use time for the main hand item.</summary>
    /// <returns><c>float</c></returns>
    public Molang MainHandItemUseDuration(params object[] args)
    {
        _tokens.Add($"query.main_hand_item_use_duration({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the entity's mark variant</summary>
    /// <returns><c>float</c></returns>
    public Molang MarkVariant(params object[] args)
    {
        _tokens.Add($"query.mark_variant({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the max durability an item can take.</summary>
    /// <returns><c>float</c></returns>
    public Molang MaxDurability(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.max_durability({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the maximum health of the entity, or 0.0 if it doesn't make sense to call on this entity.</summary>
    /// <returns><c>float</c></returns>
    public Molang MaxHealth(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.max_health({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the maximum trade tier of the entity if it makes sense, else it returns 0.0</summary>
    /// <returns><c>float</c></returns>
    public Molang MaxTradeTier(params object[] args)
    {
        _tokens.Add($"query.max_trade_tier({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the time in *seconds* of the most expensive frame over the last 'n' frames.  If an argument is passed, it is assumed to be the number of frames in the past that you wish to query.  'query.maximum_frame_time' (or the equivalent 'query.maximum_frame_time(0)') will return the frame time of the frame before the current one.  'query.maximum_frame_time(1)' will return the maximum frame time of the previous two frames.  Currently we store the history of the last 0 frames, although note that this may change in the future.  Asking for more frames will result in only sampling the number of frames stored.</summary>
    /// <returns><c>float</c></returns>
    public Molang MaximumFrameTime(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        _tokens.Add($"query.maximum_frame_time({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the time in *seconds* of the least expensive frame over the last 'n' frames.  If an argument is passed, it is assumed to be the number of frames in the past that you wish to query.  'query.minimum_frame_time' (or the equivalent 'query.minimum_frame_time(0)') will return the frame time of the frame before the current one.  'query.minimum_frame_time(1)' will return the minimum frame time of the previous two frames.  Currently we store the history of the last 0 frames, although note that this may change in the future.  Asking for more frames will result in only sampling the number of frames stored.</summary>
    /// <returns><c>float</c></returns>
    public Molang MinimumFrameTime(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        _tokens.Add($"query.minimum_frame_time({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the scale of the current entity.</summary>
    /// <returns><c>float</c></returns>
    public Molang ModelScale(params object[] args)
    {
        _tokens.Add($"query.model_scale({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the total distance the entity has moved horizontally in meters (since the entity was last loaded, not necessarily since it was originally created) modified along the way by status flags such as is_baby or on_fire.</summary>
    /// <returns><c>float</c></returns>
    public Molang ModifiedDistanceMoved(params object[] args)
    {
        _tokens.Add($"query.modified_distance_moved({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the current walk speed of the entity modified by status flags such as is_baby or on_fire.</summary>
    /// <returns><c>float</c></returns>
    public Molang ModifiedMoveSpeed(params object[] args)
    {
        _tokens.Add($"query.modified_move_speed({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the duration of the mob's swing/attack animation, determined by the carried item and modified by effects applied on the mob. To access the swing/attack animation progress, use "variable.attack_time" instead.</summary>
    /// <returns><c>float</c></returns>
    public Molang ModifiedSwingDuration(params object[] args)
    {
        _tokens.Add($"query.modified_swing_duration({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the brightness of the moon (FULL_MOON=1.0, WANING_GIBBOUS=0.75, FIRST_QUARTER=0.5, WANING_CRESCENT=0.25, NEW_MOON=0.0, WAXING_CRESCENT=0.25, LAST_QUARTER=0.5, WAXING_GIBBOUS=0.75).</summary>
    /// <returns><c>float</c></returns>
    public Molang MoonBrightness(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.moon_brightness({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the phase of the moon (FULL_MOON=0, WANING_GIBBOUS=1, FIRST_QUARTER=2, WANING_CRESCENT=3, NEW_MOON=4, WAXING_CRESCENT=5, LAST_QUARTER=6, WAXING_GIBBOUS=7).</summary>
    /// <returns><c>float</c></returns>
    public Molang MoonPhase(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.moon_phase({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the specified axis of the normalized position delta of the entity.</summary>
    /// <returns><c>float</c></returns>
    public Molang MovementDirection(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.movement_direction({FormatParams(args)})");
        return this;
    }
    /// <summary>Queries Perlin Noise Map</summary>
    /// <returns><c>float</c></returns>
    public Molang Noise(params object[] args)
    {
        _tokens.Add($"query.noise({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the time that the entity is on fire, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang OnFireTime(params object[] args)
    {
        _tokens.Add($"query.on_fire_time({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the entity is out of control, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang OutOfControl(params object[] args)
    {
        _tokens.Add($"query.out_of_control({FormatParams(args)})");
        return this;
    }
    /// <summary>DEPRECATED (Do not use - this function is deprecated and will be removed).</summary>
    /// <returns><c>float</c></returns>
    public Molang OverlayAlpha(params object[] args)
    {
        _tokens.Add($"query.overlay_alpha({FormatParams(args)})");
        return this;
    }
    /// <summary>DEPRECATED (Use query.is_owner_identifier_any instead if possible so names can be changed later without breaking content.) Returns the root actor identifier.</summary>
    /// <returns><c>hash_type_64</c></returns>
    public Molang OwnerIdentifier(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.owner_identifier({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the players level if the actor is a player, otherwise returns 0.</summary>
    /// <returns><c>float</c></returns>
    public Molang PlayerLevel(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.player_level({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the absolute position of an actor.  Takes one argument that represents the desired axis (0 == x-axis, 1 == y-axis, 2 == z-axis).</summary>
    /// <returns><c>float</c></returns>
    public Molang Position(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.position({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the position delta for an actor.  Takes one argument that represents the desired axis (0 == x-axis, 1 == y-axis, 2 == z-axis).</summary>
    /// <returns><c>float</c></returns>
    public Molang PositionDelta(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.position_delta({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the previous squish value for the current entity, or 0.0 if this doesn't make sense.</summary>
    /// <returns><c>float</c></returns>
    public Molang PreviousSquishValue(params object[] args)
    {
        _tokens.Add($"query.previous_squish_value({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes one argument: the name of the property on the entity. Returns the value of that property if it exists, else 0.0 if not.</summary>
    /// <returns><c>float</c></returns>
    public Molang Property(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.property({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes an entity-relative position and one or more tag names, and returns either 0 or 1 based on if the block at that position has all of the tags provided.</summary>
    /// <returns><c>bool</c></returns>
    public Molang RelativeBlockHasAllTags(params object[] args)
    {
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.relative_block_has_all_tags({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes an entity-relative position and one or more tag names, and returns either 0 or 1 based on if the block at that position has any of the tags provided.</summary>
    /// <returns><c>bool</c></returns>
    public Molang RelativeBlockHasAnyTag(params object[] args)
    {
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.relative_block_has_any_tag({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns how much durability an item has remaining.</summary>
    /// <returns><c>float</c></returns>
    public Molang RemainingDurability(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.remaining_durability({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the body pitch world-rotation of the ride an entity, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang RideBodyXRotation(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.ride_body_x_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the body yaw world-rotation of the ride of on an entity, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang RideBodyYRotation(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.ride_body_y_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the head x world-rotation of the ride of an entity, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang RideHeadXRotation(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.ride_head_x_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes one optional argument as a parameter. Returns the head y world-rotation of the ride of an entity, else it returns 0.0. First parameter only for horses, zombie horses, skeleton horses, donkeys and mules that clamps rotation in degrees.</summary>
    /// <returns><c>float</c></returns>
    public Molang RideHeadYRotation(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        _tokens.Add($"query.ride_head_y_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the body pitch world-rotation of a valid rider at the provided index if called on an entity, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang RiderBodyXRotation(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.rider_body_x_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the body yaw world-rotation of a valid rider at the provided index if called on an entity, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang RiderBodyYRotation(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.rider_body_y_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes one argument as a parameter. Returns the head x world-rotation of the rider entity at the provided index, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang RiderHeadXRotation(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.rider_head_x_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes one or two arguments as parameters. Returns the head y world-rotation of the rider entity at the provided index, else it returns 0.0. Horses, zombie horses, skeleton horses, donkeys and mules require a second parameter that clamps rotation in degrees.</summary>
    /// <returns><c>float</c></returns>
    public Molang RiderHeadYRotation(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.rider_head_y_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the roll counter of the entity.</summary>
    /// <returns><c>float</c></returns>
    public Molang RollCounter(params object[] args)
    {
        _tokens.Add($"query.roll_counter({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the rotation required to aim at the camera.  Requires one argument representing the rotation axis you would like (0 for x, 1 for y).</summary>
    /// <returns><c>float</c></returns>
    public Molang RotationToCamera(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.rotation_to_camera({FormatParams(args)})");
        return this;
    }
    /// <summary>Takes one argument - the name of the scoreboard entry for this entity. Returns the specified scoreboard value for this entity. Available only with behavior packs.</summary>
    /// <returns><c>float</c></returns>
    public Molang Scoreboard(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"query.scoreboard({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns a number representing the server RAM memory tier, 0 = 'SuperLow', 1 = 'Low', 2 = 'Mid', 3 = 'High', or 4 = 'SuperHigh'. Available on the server side (Behavior Packs) only.</summary>
    /// <returns><c>float</c></returns>
    public Molang ServerMemoryTier(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.server_memory_tier({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the shaking angle of the entity if it makes sense, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang ShakeAngle(params object[] args)
    {
        _tokens.Add($"query.shake_angle({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the shake time of the entity.</summary>
    /// <returns><c>float</c></returns>
    public Molang ShakeTime(params object[] args)
    {
        _tokens.Add($"query.shake_time({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the how much the offhand shield should translate down when blocking and being hit.</summary>
    /// <returns><c>float</c></returns>
    public Molang ShieldBlockingBob(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.shield_blocking_bob({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if we render the entity's bottom, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang ShowBottom(params object[] args)
    {
        _tokens.Add($"query.show_bottom({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the current sit amount of the entity.</summary>
    /// <returns><c>float</c></returns>
    public Molang SitAmount(params object[] args)
    {
        _tokens.Add($"query.sit_amount({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the entity's skin ID</summary>
    /// <returns><c>float</c></returns>
    public Molang SkinId(params object[] args)
    {
        _tokens.Add($"query.skin_id({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the rotation of the bed the player is sleeping on.</summary>
    /// <returns><c>float</c></returns>
    public Molang SleepRotation(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.sleep_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the sneeze counter of the entity.</summary>
    /// <returns><c>float</c></returns>
    public Molang SneezeCounter(params object[] args)
    {
        _tokens.Add($"query.sneeze_counter({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns a struct representing the entity spell color for the specified entity. The struct contains '.r' '.g' '.b' and '.a' members, each 0.0 to 1.0. If no actor is specified, each member value will be 0.0.</summary>
    /// <returns><c>member_array</c></returns>
    public Molang Spellcolor(params object[] args)
    {
        _tokens.Add($"query.spellcolor({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the scale of how standing up the entity is.</summary>
    /// <returns><c>float</c></returns>
    public Molang StandingScale(params object[] args)
    {
        _tokens.Add($"query.standing_scale({FormatParams(args)})");
        return this;
    }
    /// <summary>Only valid in an animation controller. Returns the time in seconds in the current animation controller state.</summary>
    /// <returns><c>float</c></returns>
    public Molang StateTime(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.state_time({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the structural integrity for the actor, otherwise returns 0.</summary>
    /// <returns><c>float</c></returns>
    public Molang StructuralIntegrity(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.structural_integrity({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the particle color for the block located in the surface below the actor (scanned up to 10 blocks down). The struct contains '.r' '.g' '.b' and '.a' members, each 0.0 to 1.0. If no actor is specified or if no surface is found, each member value is set to 0.0. Available on the Client (Resource Packs) only.</summary>
    /// <returns><c>member_array</c></returns>
    public Molang SurfaceParticleColor(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.surface_particle_color({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the texture coordinate for generating particles for the block located in the surface below the actor (scanned up to 10 blocks down) in a struct with 'u' and 'v' keys. If no actor is specified or if no surface is found, u and v will be 0.0. Available on the Client (Resource Packs) only.</summary>
    /// <returns><c>member_array</c></returns>
    public Molang SurfaceParticleTextureCoordinate(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.surface_particle_texture_coordinate({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the texture size for generating particles for the block located in the surface below the actor (scanned up to 10 blocks down). If no actor is specified or if no surface is found, each member value will be 0.0. Available on the Client (Resource Packs) only.</summary>
    /// <returns><c>member_array</c></returns>
    public Molang SurfaceParticleTextureSize(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.surface_particle_texture_size({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns how swollen the entity is. Only works for "minecraft:creeper" and "minecraft:wither".</summary>
    /// <returns><c>float</c></returns>
    public Molang SwellAmount(params object[] args)
    {
        _tokens.Add($"query.swell_amount({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the swelling direction of the entity if it makes sense, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang SwellingDir(params object[] args)
    {
        _tokens.Add($"query.swelling_dir({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the amount the current entity is swimming.</summary>
    /// <returns><c>float</c></returns>
    public Molang SwimAmount(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.swim_amount({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the angle of the tail of the entity if it makes sense, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang TailAngle(params object[] args)
    {
        _tokens.Add($"query.tail_angle({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the x rotation required to aim at the entity's current target if it has one, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang TargetXRotation(params object[] args)
    {
        _tokens.Add($"query.target_x_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the y rotation required to aim at the entity's current target if it has one, else it returns 0.0.</summary>
    /// <returns><c>float</c></returns>
    public Molang TargetYRotation(params object[] args)
    {
        _tokens.Add($"query.target_y_rotation({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the icon index of the experience orb.</summary>
    /// <returns><c>float</c></returns>
    public Molang TextureFrameIndex(params object[] args)
    {
        _tokens.Add($"query.texture_frame_index({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the number of ticks elapsed since the user last hit something while using a kinetic weapon. Returns -1.0 if no kinetic weapon is being used or if nothing has been hit yet. Hits that occur while the user is unloaded are not counted.</summary>
    /// <returns><c>float</c></returns>
    public Molang TicksSinceLastKineticWeaponHit(params object[] args)
    {
        _tokens.Add($"query.ticks_since_last_kinetic_weapon_hit({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the time of day (midnight=0.0, sunrise=0.25, noon=0.5, sunset=0.75) of the dimension the entity is in.</summary>
    /// <returns><c>float</c></returns>
    public Molang TimeOfDay(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.time_of_day({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the time in seconds since the last vibration detected by the actor. On errors or if no vibration has been detected yet, returns -1. Available on the Client (Resource Packs) only.</summary>
    /// <returns><c>float</c></returns>
    public Molang TimeSinceLastVibrationDetection(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.time_since_last_vibration_detection({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the current time stamp of the level</summary>
    /// <returns><c>float</c></returns>
    public Molang TimeStamp(params object[] args)
    {
        _tokens.Add($"query.time_stamp({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if behavior.timer_flag_1 is running, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang TimerFlag1(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.timer_flag_1({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if behavior.timer_flag_2 is running, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang TimerFlag2(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.timer_flag_2({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if behavior.timer_flag_3 is running, else it returns 0.0.</summary>
    /// <returns><c>bool</c></returns>
    public Molang TimerFlag3(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.timer_flag_3({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the total number of active emitters in the world.</summary>
    /// <returns><c>float</c></returns>
    public Molang TotalEmitterCount(params object[] args)
    {
        _tokens.Add($"query.total_emitter_count({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the total number of active particles in the world.</summary>
    /// <returns><c>float</c></returns>
    public Molang TotalParticleCount(params object[] args)
    {
        _tokens.Add($"query.total_particle_count({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns 1.0 if the touch input only affects the touchbar, otherwise returns 0.0. Available on the Client (Resource Packs) only.</summary>
    /// <returns><c>float</c></returns>
    public Molang TouchOnlyAffectsHotbar(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"query.touch_only_affects_hotbar({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the trade tier of the entity if it makes sense, else it returns 0.0</summary>
    /// <returns><c>float</c></returns>
    public Molang TradeTier(params object[] args)
    {
        _tokens.Add($"query.trade_tier({FormatParams(args)})");
        return this;
    }
    /// <summary>Always returns zero. (Was originally meant to indicate Panda unhappiness but due to an early code change it has always only returned zero)</summary>
    /// <returns><c>float</c></returns>
    public Molang UnhappyCounter(params object[] args)
    {
        _tokens.Add($"query.unhappy_counter({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the entity's variant index</summary>
    /// <returns><c>float</c></returns>
    public Molang Variant(params object[] args)
    {
        _tokens.Add($"query.variant({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the speed of the entity up or down in meters/second, where positive is up.</summary>
    /// <returns><c>float</c></returns>
    public Molang VerticalSpeed(params object[] args)
    {
        _tokens.Add($"query.vertical_speed({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the total distance traveled by an entity while on the ground and not sneaking.</summary>
    /// <returns><c>float</c></returns>
    public Molang WalkDistance(params object[] args)
    {
        _tokens.Add($"query.walk_distance({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the wing flap position of the entity, or 0.0 if this doesn't make sense.</summary>
    /// <returns><c>float</c></returns>
    public Molang WingFlapPosition(params object[] args)
    {
        _tokens.Add($"query.wing_flap_position({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the wing flap speed of the entity, or 0.0 if this doesn't make sense.</summary>
    /// <returns><c>float</c></returns>
    public Molang WingFlapSpeed(params object[] args)
    {
        _tokens.Add($"query.wing_flap_speed({FormatParams(args)})");
        return this;
    }
    /// <summary>Returns the entity's yaw speed</summary>
    /// <returns><c>float</c></returns>
    public Molang YawSpeed(params object[] args)
    {
        _tokens.Add($"query.yaw_speed({FormatParams(args)})");
        return this;
    }
    #endregion
    #region math
    /// <summary>Absolute Value 'math.abs'</summary>
    public Molang Abs(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"math.abs({FormatParams(args)})");
        return this;
    }
    /// <summary>Arc Cosine 'math.acos'</summary>
    public Molang Acos(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"math.acos({FormatParams(args)})");
        return this;
    }
    /// <summary>Arc Sine 'math.asin'</summary>
    public Molang Asin(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"math.asin({FormatParams(args)})");
        return this;
    }
    /// <summary>Arc Tangent 'math.atan'</summary>
    public Molang Atan(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"math.atan({FormatParams(args)})");
        return this;
    }
    /// <summary>atan2 'math.atan2'</summary>
    public Molang Atan2(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 2) throw new ArgumentException("min argument count of 2");
        _tokens.Add($"math.atan2({FormatParams(args)})");
        return this;
    }
    /// <summary>Ceiling 'math.ceil'</summary>
    public Molang Ceil(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"math.ceil({FormatParams(args)})");
        return this;
    }
    /// <summary>Clamp 'math.clamp'</summary>
    public Molang Clamp(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.clamp({FormatParams(args)})");
        return this;
    }
    /// <summary>Copy Sign 'math.copy_sign'</summary>
    public Molang CopySign(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 2) throw new ArgumentException("min argument count of 2");
        _tokens.Add($"math.copy_sign({FormatParams(args)})");
        return this;
    }
    /// <summary>Cosine 'math.cos'</summary>
    public Molang Cos(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"math.cos({FormatParams(args)})");
        return this;
    }
    /// <summary>Die Roll 'math.die_roll'</summary>
    public Molang DieRoll(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.die_roll({FormatParams(args)})");
        return this;
    }
    /// <summary>Die Roll Integer 'math.die_roll_integer'</summary>
    public Molang DieRollInteger(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.die_roll_integer({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Back 'math.ease_in_back'</summary>
    public Molang EaseInBack(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_back({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Bounce 'math.ease_in_bounce'</summary>
    public Molang EaseInBounce(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_bounce({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Circ 'math.ease_in_circ'</summary>
    public Molang EaseInCirc(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_circ({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Cubic 'math.ease_in_cubic'</summary>
    public Molang EaseInCubic(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_cubic({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Elastic 'math.ease_in_elastic'</summary>
    public Molang EaseInElastic(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_elastic({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Expo 'math.ease_in_expo'</summary>
    public Molang EaseInExpo(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_expo({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Out Back 'math.ease_in_out_back'</summary>
    public Molang EaseInOutBack(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_out_back({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Out Bounce 'math.ease_in_out_bounce'</summary>
    public Molang EaseInOutBounce(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_out_bounce({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Out Circ 'math.ease_in_out_circ'</summary>
    public Molang EaseInOutCirc(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_out_circ({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Out Cubic 'math.ease_in_out_cubic'</summary>
    public Molang EaseInOutCubic(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_out_cubic({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Out Elastic 'math.ease_in_out_elastic'</summary>
    public Molang EaseInOutElastic(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_out_elastic({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Out Expo 'math.ease_in_out_expo'</summary>
    public Molang EaseInOutExpo(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_out_expo({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Out Quad 'math.ease_in_out_quad'</summary>
    public Molang EaseInOutQuad(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_out_quad({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Out Quart 'math.ease_in_out_quart'</summary>
    public Molang EaseInOutQuart(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_out_quart({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Out Quint 'math.ease_in_out_quint'</summary>
    public Molang EaseInOutQuint(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_out_quint({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Out Sine 'math.ease_in_out_sine'</summary>
    public Molang EaseInOutSine(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_out_sine({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Quad 'math.ease_in_quad'</summary>
    public Molang EaseInQuad(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_quad({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Quart 'math.ease_in_quart'</summary>
    public Molang EaseInQuart(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_quart({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Quint 'math.ease_in_quint'</summary>
    public Molang EaseInQuint(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_quint({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease In Sine 'math.ease_in_sine'</summary>
    public Molang EaseInSine(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_in_sine({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease Out Back 'math.ease_out_back'</summary>
    public Molang EaseOutBack(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_out_back({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease Out Bounce 'math.ease_out_bounce'</summary>
    public Molang EaseOutBounce(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_out_bounce({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease Out Circ 'math.ease_out_circ'</summary>
    public Molang EaseOutCirc(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_out_circ({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease Out Cubic 'math.ease_out_cubic'</summary>
    public Molang EaseOutCubic(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_out_cubic({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease Out Elastic 'math.ease_out_elastic'</summary>
    public Molang EaseOutElastic(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_out_elastic({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease Out Expo 'math.ease_out_expo'</summary>
    public Molang EaseOutExpo(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_out_expo({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease Out Quad 'math.ease_out_quad'</summary>
    public Molang EaseOutQuad(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_out_quad({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease Out Quart 'math.ease_out_quart'</summary>
    public Molang EaseOutQuart(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_out_quart({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease Out Quint 'math.ease_out_quint'</summary>
    public Molang EaseOutQuint(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_out_quint({FormatParams(args)})");
        return this;
    }
    /// <summary>Ease Out Sine 'math.ease_out_sine'</summary>
    public Molang EaseOutSine(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.ease_out_sine({FormatParams(args)})");
        return this;
    }
    /// <summary>Base-e Exponent 'math.exp'</summary>
    public Molang Exp(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"math.exp({FormatParams(args)})");
        return this;
    }
    /// <summary>Floor 'math.floor'</summary>
    public Molang Floor(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"math.floor({FormatParams(args)})");
        return this;
    }
    /// <summary>Hermite Blend 'math.hermite_blend'</summary>
    public Molang HermiteBlend(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"math.hermite_blend({FormatParams(args)})");
        return this;
    }
    /// <summary>Inverse Lerp 'math.inverse_lerp'</summary>
    public Molang InverseLerp(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.inverse_lerp({FormatParams(args)})");
        return this;
    }
    /// <summary>Lerp 'math.lerp'</summary>
    public Molang Lerp(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.lerp({FormatParams(args)})");
        return this;
    }
    /// <summary>Lerp Rotate 'math.lerprotate'</summary>
    public Molang Lerprotate(params object[] args)
    {
        if (args.Length > 3) throw new ArgumentException("max argument count of 3");
        if (args.Length < 3) throw new ArgumentException("min argument count of 3");
        _tokens.Add($"math.lerprotate({FormatParams(args)})");
        return this;
    }
    /// <summary>Natural Log 'math.ln'</summary>
    public Molang Ln(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"math.ln({FormatParams(args)})");
        return this;
    }
    /// <summary>Max 'math.max'</summary>
    public Molang Max(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 2) throw new ArgumentException("min argument count of 2");
        _tokens.Add($"math.max({FormatParams(args)})");
        return this;
    }
    /// <summary>Min 'math.min'</summary>
    public Molang Min(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 2) throw new ArgumentException("min argument count of 2");
        _tokens.Add($"math.min({FormatParams(args)})");
        return this;
    }
    /// <summary>Min Angle 'math.min_angle'</summary>
    public Molang MinAngle(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"math.min_angle({FormatParams(args)})");
        return this;
    }
    /// <summary>Mod 'math.mod'</summary>
    public Molang Mod(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 2) throw new ArgumentException("min argument count of 2");
        _tokens.Add($"math.mod({FormatParams(args)})");
        return this;
    }
    /// <summary>Pi</summary>
    public Molang Pi(params object[] args)
    {
        if (args.Length > 0) throw new ArgumentException("max argument count of 0");
        _tokens.Add($"math.pi({FormatParams(args)})");
        return this;
    }
    /// <summary>Power 'math.pow'</summary>
    public Molang Pow(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 2) throw new ArgumentException("min argument count of 2");
        _tokens.Add($"math.pow({FormatParams(args)})");
        return this;
    }
    /// <summary>Random 'math.random'</summary>
    public Molang Random(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 2) throw new ArgumentException("min argument count of 2");
        _tokens.Add($"math.random({FormatParams(args)})");
        return this;
    }
    /// <summary>Random Integer 'math.random_integer'</summary>
    public Molang RandomInteger(params object[] args)
    {
        if (args.Length > 2) throw new ArgumentException("max argument count of 2");
        if (args.Length < 2) throw new ArgumentException("min argument count of 2");
        _tokens.Add($"math.random_integer({FormatParams(args)})");
        return this;
    }
    /// <summary>Round 'math.round'</summary>
    public Molang Round(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"math.round({FormatParams(args)})");
        return this;
    }
    /// <summary>Sign 'math.sign'</summary>
    public Molang Sign(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"math.sign({FormatParams(args)})");
        return this;
    }
    /// <summary>Sine 'math.sin'</summary>
    public Molang Sin(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"math.sin({FormatParams(args)})");
        return this;
    }
    /// <summary>Square Root 'math.sqrt'</summary>
    public Molang Sqrt(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"math.sqrt({FormatParams(args)})");
        return this;
    }
    /// <summary>Truncate 'math.trunc'</summary>
    public Molang Trunc(params object[] args)
    {
        if (args.Length > 1) throw new ArgumentException("max argument count of 1");
        if (args.Length < 1) throw new ArgumentException("min argument count of 1");
        _tokens.Add($"math.trunc({FormatParams(args)})");
        return this;
    }
    #endregion
}