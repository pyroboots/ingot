import { system, world } from "@minecraft/server";

let tick = 0;

system.runInterval(() => {
    tick++;
    if (tick % 200 !== 0)
        return;

    for (const player of world.getAllPlayers())
        player.onScreenDisplay.setActionBar("Dense lasagna watches...");
}, 1);