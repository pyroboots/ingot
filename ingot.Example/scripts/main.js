import { world } from "@minecraft/server";

world.afterEvents.worldLoad.subscribe(() => {
    console.warn("[ingot example] lasagna pack loaded");
});