using ingot.Core;
using ingot.Example.Blocks;
using ingot.Example.Entities;
using ingot.Example.Items;

namespace ingot.Example;

public class ExamplePack : IPack
{
    public string BehaviourUuid => "a8f3c2e1-4b5d-6e7f-8091-a2b3c4d5e6f7";
    public string ResourceUuid => "b9e4d3c2-5a6b-7c8d-9e0f-b1c2d3e4f5a6";
    public string Name => "ingot example";
    public string Description => "Example pack made with ingot";

    public Type[] Items =>
    [
        typeof(LasagnaItem),
        typeof(CheeseItem),
        typeof(PastaItem),
        typeof(SauceItem),
    ];

    public Type[] Blocks => [typeof(DenseLasagnaBlock)];
    public Type[] Entities => [typeof(CowEntity)];
}