using GOAP;

class GetAxeAction : GOAP.Action
{
    public GetAxeAction() : base(5)
    {
    }

    public override bool IsValid(WorldState worldState) => !worldState.hasPickaxe;
    public override WorldState ApplyEffects(in WorldState worldState)
    {
        WorldState newWorldstate = worldState;
        newWorldstate.hasAxe = true;
        return newWorldstate;
    }


    public override void Execute()
    {
        Console.WriteLine("Gettin axe");
    }
}

class ChopWoodAction : GOAP.Action
{
    public ChopWoodAction() : base(10)
    {
    }

    public override bool IsValid(WorldState worldState) => worldState.hasAxe;
    public override WorldState ApplyEffects(in WorldState worldState)
    {
        WorldState newWorldstate = worldState;
        newWorldstate.hasWood = true;
        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Gettin wood");
    }
}


class GetSticksAction : GOAP.Action
{
    public GetSticksAction() : base(20)
    {
    }

    public override bool IsValid(WorldState worldState) => true;
    public override WorldState ApplyEffects(in WorldState worldState)
    {
        WorldState newWorldstate = worldState;
        newWorldstate.hasWood = true;
        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Gettin sticks");
    }
}

class GetPickaxeAction : GOAP.Action
{
    public GetPickaxeAction() : base(5)
    {
    }

    public override bool IsValid(WorldState worldState) => !worldState.hasAxe;


    public override WorldState ApplyEffects(in WorldState worldState)
    {
        WorldState newWorldstate = worldState;
        newWorldstate.hasPickaxe = true;
        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Gettin pickaxe");
    }
}

class GetRockAction : GOAP.Action
{
    public GetRockAction() : base(10)
    {
    }

    public override bool IsValid(WorldState worldState) => worldState.hasPickaxe;
    public override WorldState ApplyEffects(in WorldState worldState)
    {
        WorldState newWorldstate = worldState;
        newWorldstate.hasRock = true;
        return newWorldstate;
    }


    public override void Execute()
    {
        Console.WriteLine("Gettin rock");
    }
}