function round2sf(num) {
    if (num === 0) return 0;
    return parseFloat(num.toPrecision(2));
}

world.getPlayers().forEach(player => {
    const inv = player.getComponent("minecraft:inventory");
    const cont = inv.container;
    
    for (let i = 0; i < cont.size; i++) {
        const item = cont.getItem(i);
        if (item === undefined) return;
        if (item.typeId.includes("combinationcooking") === false) return;
        
        const food = item.getComponent("minecraft:food");
        const nutrition = food.nutrition;
        const saturation = food.saturationModifier;
        
        item.setLore(["Nutrition: " + nutrition, "Saturation: " + round2sf(saturation)]);
        cont.setItem(i, item);
    }
})