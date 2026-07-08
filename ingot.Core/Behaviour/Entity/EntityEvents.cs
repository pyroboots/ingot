using ingot.Core.Common;

namespace ingot.Core.Behaviour.Entity;

/// <summary>
/// Fluent / factory helpers for building entity event action graphs with less boilerplate.
/// </summary>
public static class EntityEvents
{
    /// <summary>
    /// Adds one or more component groups.
    /// </summary>
    public static ComponentGroupAddEntityEventAction Add(params Identifier[] groups) =>
        new() { ComponentGroups = groups };

    /// <summary>
    /// Removes one or more component groups. Empty array emits an empty <c>remove</c> object.
    /// </summary>
    public static ComponentGroupRemoveEntityEventAction Remove(params Identifier[] groups) =>
        new() { ComponentGroups = groups };

    /// <summary>
    /// Removes <paramref name="remove"/> groups and adds <paramref name="add"/> groups.
    /// </summary>
    public static IEntityEventAction[] Swap(Identifier[] remove, Identifier[] add) =>
    [
        Remove(remove),
        Add(add),
    ];

    /// <summary>
    /// Removes a single group and adds a single group.
    /// </summary>
    public static IEntityEventAction[] Swap(Identifier remove, Identifier add) =>
        Swap([remove], [add]);

    /// <summary>
    /// Triggers another entity event by name (string form).
    /// </summary>
    public static TriggerEntityEventAction Trigger(string eventId, Enums.Target? target = null) =>
        new() { Event = eventId, Target = target };

    /// <summary>
    /// Ordered sequence of action steps (each step is one or more sibling actions in an object).
    /// </summary>
    public static SequenceEntityEventAction Sequence(params IEntityEventAction[] steps) =>
        new() { EventActions = steps };

    /// <summary>
    /// Weighted randomize pool.
    /// </summary>
    public static RandomizeEntityEventAction Randomize(
        params (float weight, IEntityEventAction[] actions)[] pools) =>
        new()
        {
            EventActions = pools
                .Select(p => new RandomizeEntityEventAction.EventActionPool(p.weight, p.actions))
                .ToArray(),
        };

    /// <summary>
    /// Common lifecycle: on spawn, roll adult vs baby; adult event triggers separately.
    /// </summary>
    /// <param name="adultWeight">Weight for the adult branch (e.g. 95).</param>
    /// <param name="babyWeight">Weight for the baby branch (e.g. 5).</param>
    /// <param name="spawnAdultEvent">Event id to trigger for adults (e.g. <c>test:spawn_adult</c>).</param>
    /// <param name="babyGroup">Component group id for babies.</param>
    public static IEntityEventAction[] SpawnedAdultOrBaby(
        float adultWeight,
        float babyWeight,
        string spawnAdultEvent,
        Identifier babyGroup) =>
    [
        Sequence(
            Randomize(
                (adultWeight, [Trigger(spawnAdultEvent)]),
                (babyWeight, [Add(babyGroup)]))),
    ];

    /// <summary>
    /// Grow-up style: remove baby group, add adult group.
    /// </summary>
    public static IEntityEventAction[] GrowUp(Identifier babyGroup, Identifier adultGroup) =>
        Swap(babyGroup, adultGroup);

    /// <summary>
    /// Builds an event dictionary from (event id, actions) pairs.
    /// </summary>
    public static Dictionary<Identifier, IEntityEventAction[]> Map(
        params (Identifier id, IEntityEventAction[] actions)[] entries) =>
        entries.ToDictionary(e => e.id, e => e.actions);
}

/// <summary>
/// Small helpers for ageable grow-up event objects.
/// </summary>
public static class EntityEventTargets
{
    /// <summary>
    /// <c>{ "event": "...", "target": "self" }</c> for ageable grow_up and similar fields.
    /// </summary>
    public static Dictionary<string, string> GrowUpSelf(string eventId) => new()
    {
        ["event"] = eventId,
        ["target"] = "self",
    };
}
