using GOAP;

[Flags]
public enum EEnemyStates : ulong
{
    NONE            = 1 << 0,
    IS_NEAR_ENEMY   = 1 << 1,
    IS_NEAR_WEAPON  = 1 << 2,
    IS_HURT         = 1 << 3,
    HAS_WEAPON      = 1 << 4,
    IS_ENEMY_DEAD   = 1 << 5,
}

class EnemyGoal : Goal<EEnemyStates>
{
    public EnemyGoal(EEnemyStates activeStates, EEnemyStates inactiveStates = EEnemyStates.NONE) : base(activeStates, inactiveStates)
    {

    }
}

class MoveToWeapon : GOAP.Action<EEnemyStates>
{
    public override int GetCost(in EEnemyStates worldState) => 2;

    public override bool IsForwardValid(EEnemyStates worldState) => true;

    public override bool IsBackwardValid(EEnemyStates worldState) => (EEnemyStates.HAS_WEAPON & worldState) == 0;

    public override EEnemyStates ApplyEffects(in EEnemyStates worldState)
    {
        EEnemyStates newWorldstate = worldState;
        newWorldstate |= EEnemyStates.IS_NEAR_WEAPON;
        newWorldstate &= ~EEnemyStates.IS_NEAR_ENEMY;

        return newWorldstate;
    }

    public override EEnemyStates ApplyReversedEffects(in EEnemyStates worldState)
    {
        EEnemyStates newWorldstate = worldState;
        newWorldstate &= ~EEnemyStates.IS_NEAR_WEAPON;

        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Moving to the weapon");
    }
}

class PickUpWeapon : GOAP.Action<EEnemyStates>
{
    public override int GetCost(in EEnemyStates worldState) => (EEnemyStates.IS_NEAR_ENEMY & worldState) != 0 ? 2 : 1;

    public override bool IsForwardValid(EEnemyStates worldState) => (worldState & EEnemyStates.IS_NEAR_WEAPON) != 0;

    public override bool IsBackwardValid(EEnemyStates worldState) => true;

    public override EEnemyStates ApplyEffects(in EEnemyStates worldState)
    {
        EEnemyStates newWorldstate = worldState;
        newWorldstate |= EEnemyStates.HAS_WEAPON;

        return newWorldstate;
    }

    public override EEnemyStates ApplyReversedEffects(in EEnemyStates worldState)
    {
        EEnemyStates newWorldstate = worldState;
        newWorldstate &= ~EEnemyStates.HAS_WEAPON;
        newWorldstate |= EEnemyStates.IS_NEAR_WEAPON;

        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Picking up the weapon");
    }
}

class MoveToEnemy : GOAP.Action<EEnemyStates>
{
    public override int GetCost(in EEnemyStates worldState) => (EEnemyStates.IS_NEAR_WEAPON & worldState) != 0 ? 2 : 1;

    public override bool IsForwardValid(EEnemyStates worldState) => true;

    public override bool IsBackwardValid(EEnemyStates worldState) => (EEnemyStates.IS_ENEMY_DEAD & worldState) == 0;

    public override EEnemyStates ApplyEffects(in EEnemyStates worldState)
    {
        EEnemyStates newWorldstate = worldState;
        newWorldstate &= ~EEnemyStates.IS_NEAR_WEAPON;
        newWorldstate |= EEnemyStates.IS_NEAR_ENEMY;

        return newWorldstate;
    }

    public override EEnemyStates ApplyReversedEffects(in EEnemyStates worldState)
    {
        EEnemyStates newWorldstate = worldState;
        newWorldstate &= ~EEnemyStates.IS_NEAR_ENEMY;

        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Moving to the enemy");
    }
}

class KillEnemy : GOAP.Action<EEnemyStates>
{
    public override int GetCost(in EEnemyStates worldState) => (EEnemyStates.HAS_WEAPON & worldState) != 0 ? 1 : 10;

    public override bool IsForwardValid(EEnemyStates worldState)
    {
        return (worldState & EEnemyStates.IS_HURT) == 0 && (EEnemyStates.IS_NEAR_ENEMY & worldState) != 0;
    }

    public override bool IsBackwardValid(EEnemyStates worldState) => true;

    public override EEnemyStates ApplyEffects(in EEnemyStates worldState)
    {
        EEnemyStates newWorldstate = worldState;
        newWorldstate |= EEnemyStates.IS_ENEMY_DEAD;

        return newWorldstate;
    }

    public override EEnemyStates ApplyReversedEffects(in EEnemyStates worldState)
    {
        EEnemyStates newWorldstate = worldState;
        newWorldstate &= ~EEnemyStates.IS_ENEMY_DEAD;

        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Killing the enemy");
    }
}

class HealSelf : GOAP.Action<EEnemyStates>
{
    public override int GetCost(in EEnemyStates worldState) => 5;

    public override bool IsForwardValid(EEnemyStates worldState) => (worldState & EEnemyStates.IS_HURT) != 0;

    public override bool IsBackwardValid(EEnemyStates worldState) => (worldState & EEnemyStates.IS_ENEMY_DEAD) == 0;

    public override EEnemyStates ApplyEffects(in EEnemyStates worldState)
    {
        EEnemyStates newWorldstate = worldState;

        newWorldstate &= ~EEnemyStates.IS_HURT;

        return newWorldstate;
    }

    public override EEnemyStates ApplyReversedEffects(in EEnemyStates worldState)
    {
        EEnemyStates newWorldstate = worldState;

        newWorldstate |= EEnemyStates.IS_HURT;

        return newWorldstate;
    }

    public override void Execute()
    {
        Console.WriteLine("Healing self up");
    }
}

class GarbageAction : GOAP.Action<EEnemyStates>
{
    private int internalCost = 0;
    public GarbageAction(int cost)
    {
        this.internalCost = cost;
    }
    public override int GetCost(in EEnemyStates worldState) => internalCost;

    public override bool IsForwardValid(EEnemyStates worldState) => true;

    public override void Execute()
    {
        Console.WriteLine("Test");
    }
}