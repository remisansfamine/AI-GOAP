using GOAP;
using System.Collections;

[Flags]
public enum EEnemyStates : ulong
{
    IS_NEAR_ENEMY   = 1 << 0,
    IS_NEAR_WEAPON  = 1 << 1,
    IS_HURT         = 1 << 2,
    HAS_WEAPON      = 1 << 3,
    IS_ENEMY_DEAD   = 1 << 4,
}

class MoveToWeapon : GOAP.Action<EEnemyStates>
{
    public override int cost => 5;

    public override bool IsValid(EEnemyStates worldState) => true;
    public override EEnemyStates ApplyEffects(in EEnemyStates worldState)
    {
        EEnemyStates newWorldstate = worldState;
        newWorldstate |= EEnemyStates.IS_NEAR_WEAPON;
        newWorldstate &= ~EEnemyStates.IS_NEAR_ENEMY;

        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Moving to the weapon");
    }
}

class PickUpWeapon : GOAP.Action<EEnemyStates>
{
    public override int cost => 1;

    public override bool IsValid(EEnemyStates worldState) => (worldState & EEnemyStates.IS_NEAR_WEAPON) != 0;
    //public override bool IsValid(WorldState<EEnemyStates> worldState) => worldState.isNearWeapon;
    public override EEnemyStates ApplyEffects(in EEnemyStates worldState)
    {
        EEnemyStates newWorldstate = worldState;
        newWorldstate |= EEnemyStates.HAS_WEAPON;
        newWorldstate &= ~EEnemyStates.IS_NEAR_WEAPON;

        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Picking up the weapon");
    }
}

class MoveToEnemy : GOAP.Action<EEnemyStates>
{
    public override int cost => 5;

    public override bool IsValid(EEnemyStates worldState) => true;
    public override EEnemyStates ApplyEffects(in EEnemyStates worldState)
    {
        EEnemyStates newWorldstate = worldState;
        newWorldstate &= ~EEnemyStates.IS_NEAR_WEAPON;
        newWorldstate |= EEnemyStates.IS_NEAR_ENEMY;

        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Moving to the enemy");
    }
}

class KillEnemy : GOAP.Action<EEnemyStates>
{
    public override int cost => 15;

    //public override bool IsValid(WorldState<EEnemyStates> worldState) => !worldState.isHurt && worldState.isNearEnemy && worldState.hasWeapon;
    public override bool IsValid(EEnemyStates worldState)
    {
        bool hurt = (worldState & EEnemyStates.IS_HURT) != 0;
        bool nearenemy = (worldState & EEnemyStates.IS_NEAR_ENEMY) != 0;
        bool nearweapon = (worldState & EEnemyStates.IS_NEAR_WEAPON) != 0;
        bool hasweapon = (worldState & EEnemyStates.HAS_WEAPON) != 0;
        bool enemydead = (worldState & EEnemyStates.IS_ENEMY_DEAD) != 0;

        return (worldState & EEnemyStates.IS_HURT) == 0 && ((EEnemyStates.IS_NEAR_ENEMY) & worldState) != 0 && ((EEnemyStates.HAS_WEAPON) & worldState) != 0;
    }
    public override EEnemyStates ApplyEffects(in EEnemyStates worldState)
    {
        EEnemyStates newWorldstate = worldState; 
        newWorldstate |= EEnemyStates.IS_ENEMY_DEAD;

        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Killing the enemy");
    }
}

class HealSelf : GOAP.Action<EEnemyStates>
{
    public override int cost => 20;

    //public override bool IsValid(WorldState<EEnemyStates> worldState) => worldState.isHurt && !worldState.isNearEnemy;
    public override bool IsValid(EEnemyStates worldState) => (worldState & EEnemyStates.IS_HURT) != 0;
    public override EEnemyStates ApplyEffects(in EEnemyStates worldState)
    {
        EEnemyStates newWorldstate = worldState;

        newWorldstate &= ~EEnemyStates.IS_HURT;

        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Healing self up");
    }
}

class Test : GOAP.Action<EEnemyStates>
{
    public override int cost => 70;

    public override bool IsValid(EEnemyStates worldState) => true;
    public override EEnemyStates ApplyEffects(in EEnemyStates worldState)
    {
        EEnemyStates newWorldstate = worldState;
        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Test");
    }
}