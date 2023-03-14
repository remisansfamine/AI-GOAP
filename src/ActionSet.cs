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
}

class GetWoodAction : GOAP.Action
{
    public GetWoodAction() : base(10)
    {
    }

    public override bool IsValid(WorldState worldState) => worldState.hasAxe;
    public override WorldState ApplyEffects(in WorldState worldState)
    {
        WorldState newWorldstate = worldState;
        newWorldstate.hasWood = true;
        return newWorldstate;
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
}