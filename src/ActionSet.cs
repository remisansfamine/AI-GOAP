using GOAP;

class MoveToWeapon : GOAP.Action
{
    public override int cost => 5;

    public override bool IsValid(WorldState worldState) => true;
    public override WorldState ApplyEffects(in WorldState worldState)
    {
        WorldState newWorldstate = worldState;
        newWorldstate.isNearWeapon = true;
        newWorldstate.isNearEnemy = false;
        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Moving to the weapon");
    }
}

class PickUpWeapon : GOAP.Action
{
    public override int cost => 1;

    public override bool IsValid(WorldState worldState) => worldState.isNearWeapon;
    public override WorldState ApplyEffects(in WorldState worldState)
    {
        WorldState newWorldstate = worldState;
        newWorldstate.hasWeapon = true;
        newWorldstate.isNearWeapon = false;
        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Picking up the weapon");
    }
}

class MoveToEnemy : GOAP.Action
{
    public override int cost => 5;

    public override bool IsValid(WorldState worldState) => true;
    public override WorldState ApplyEffects(in WorldState worldState)
    {
        WorldState newWorldstate = worldState;
        newWorldstate.isNearWeapon = false;
        newWorldstate.isNearEnemy = true;
        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Moving to the enemy");
    }
}

class KillEnemy : GOAP.Action
{
    public override int cost => 15;

    public override bool IsValid(WorldState worldState) => !worldState.isHurt && worldState.isNearEnemy && worldState.hasWeapon;
    public override WorldState ApplyEffects(in WorldState worldState)
    {
        WorldState newWorldstate = worldState;
        newWorldstate.isEnemyDead = true;
        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Killing the enemy");
    }
}

class HealSelf : GOAP.Action
{
    public override int cost => 20;

    public override bool IsValid(WorldState worldState) => worldState.isHurt && !worldState.isNearEnemy;
    public override WorldState ApplyEffects(in WorldState worldState)
    {
        WorldState newWorldstate = worldState;
        newWorldstate.isHurt = false;
        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Healing self up");
    }
}