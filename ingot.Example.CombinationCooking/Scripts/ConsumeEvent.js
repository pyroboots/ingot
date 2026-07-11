// Handler body for onConsume (ingot wraps this in the generated component).
// Magic soups are tagged "magic" (+ optional colour tag) by FoodItem/ItemGenerator.

const entity = event.source;
const item = event.itemStack;

if (!entity || !item || typeof entity.addEffect !== "function") {
    return;
}

const tags = typeof item.getTags === "function" ? item.getTags() : [];
const hasTag = (tag) =>
    (Array.isArray(tags) && tags.includes(tag))
    || (typeof item.hasTag === "function" && item.hasTag(tag));

// Only magic soups grant potion effects (pasta / normal soup do not).
const isMagic =
    hasTag("magic")
    || (typeof item.typeId === "string"
        && (item.typeId.includes("_magic_") || item.typeId.endsWith("_magic") || item.typeId.includes(":bowl_magic")));

if (!isMagic) {
    return;
}

const seconds = (s) => Math.floor(s * 20);

// base buffs
entity.addEffect("regeneration", seconds(8), { amplifier: 0, showParticles: true });
entity.addEffect("absorption", seconds(20), { amplifier: 0, showParticles: true });

// Colour of the soup tints the secondary effect
const colourEffects = {
    red: { id: "strength", duration: seconds(25), amplifier: 0 },
    orange: { id: "fire_resistance", duration: seconds(30), amplifier: 0 },
    yellow: { id: "haste", duration: seconds(25), amplifier: 0 },
    lime: { id: "jump_boost", duration: seconds(25), amplifier: 1 },
    green: { id: "poison", duration: seconds(10), amplifier: 0 }, // cursed green magic
    cyan: { id: "water_breathing", duration: seconds(40), amplifier: 0 },
    blue: { id: "speed", duration: seconds(25), amplifier: 0 },
    purple: { id: "resistance", duration: seconds(25), amplifier: 0 },
    magenta: { id: "slow_falling", duration: seconds(25), amplifier: 0 },
    pink: { id: "night_vision", duration: seconds(25), amplifier: 0 },
    white: { id: "invisibility", duration: seconds(15), amplifier: 0 },
    gray: { id: "blindness", duration: seconds(4), amplifier: 0 },
    black: { id: "wither", duration: seconds(5), amplifier: 0 },
};

for (const [colour, effect] of Object.entries(colourEffects)) {
    if (!hasTag(colour)) {
        continue;
    }

    entity.addEffect(effect.id, effect.duration, {
        amplifier: effect.amplifier,
        showParticles: false,
    });
    break; // one colour tag is enough
}

if (typeof entity.playSound === "function") {
    entity.playSound("random.orb");
} else if (entity.dimension && typeof entity.dimension.playSound === "function") {
    entity.dimension.playSound("random.orb", entity.location);
}
