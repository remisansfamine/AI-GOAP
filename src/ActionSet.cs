using GOAP;

[Flags]
public enum ENPCStates : ulong
{
    NONE            = 1 << 0,
    IS_NEAR_PLAYER   = 1 << 1,
    IS_NEAR_WEAPON  = 1 << 2,
    IS_HURT         = 1 << 3,
    HAS_WEAPON      = 1 << 4,
    IS_PLAYER_DEAD   = 1 << 5,
}

class EnemyGoal : Goal<ENPCStates>
{
    public EnemyGoal(ENPCStates activeStates, ENPCStates inactiveStates = ENPCStates.NONE) : base(activeStates, inactiveStates)
    {

    }
}

class MoveToWeapon : GOAP.Action<ENPCStates>
{
    public override int GetCost(in ENPCStates worldState) => 2;

    public override bool IsForwardValid(ENPCStates worldState) => true;

    public override bool IsBackwardValid(ENPCStates worldState) => (ENPCStates.HAS_WEAPON & worldState) == 0;

    public override ENPCStates ApplyEffects(in ENPCStates worldState)
    {
        ENPCStates newWorldstate = worldState;
        newWorldstate |= ENPCStates.IS_NEAR_WEAPON;
        newWorldstate &= ~ENPCStates.IS_NEAR_PLAYER;

        return newWorldstate;
    }

    public override ENPCStates ApplyReversedEffects(in ENPCStates worldState)
    {
        ENPCStates newWorldstate = worldState;
        newWorldstate &= ~ENPCStates.IS_NEAR_WEAPON;

        return newWorldstate;
    }

    int currentStep = 0;
    const int maxStep = 20;

    public override EActionState Execute()
    {
        currentStep++;

        if (currentStep >= maxStep)
        {
            Console.Write("\r" + new string(' ', Console.BufferWidth));
            Utils.TimedWrite("\r-> Moved to the weapon\n", 0);

            return EActionState.SUCCESS;
        }

        Utils.DrawProgressBar("-> Moving to the weapon ", currentStep, maxStep);

        Thread.Sleep(100);

        return EActionState.RUNNING;
    }
}

class PickUpWeapon : GOAP.Action<ENPCStates>
{
    public override int GetCost(in ENPCStates worldState) => (ENPCStates.IS_NEAR_PLAYER & worldState) != 0 ? 2 : 1;

    public override bool IsForwardValid(ENPCStates worldState) => (worldState & ENPCStates.IS_NEAR_WEAPON) != 0;

    public override bool IsBackwardValid(ENPCStates worldState) => true;

    public override ENPCStates ApplyEffects(in ENPCStates worldState)
    {
        ENPCStates newWorldstate = worldState;
        newWorldstate |= ENPCStates.HAS_WEAPON;

        return newWorldstate;
    }

    public override ENPCStates ApplyReversedEffects(in ENPCStates worldState)
    {
        ENPCStates newWorldstate = worldState;
        newWorldstate &= ~ENPCStates.HAS_WEAPON;
        newWorldstate |= ENPCStates.IS_NEAR_WEAPON;

        return newWorldstate;
    }

    public override EActionState Execute()
    {
        Utils.TimedWrite("-> Picked up the weapon\n");

        return EActionState.SUCCESS;
    }
}

class MoveToEnemy : GOAP.Action<ENPCStates>
{
    public override int GetCost(in ENPCStates worldState) => (ENPCStates.IS_NEAR_WEAPON & worldState) != 0 ? 2 : 1;

    public override bool IsForwardValid(ENPCStates worldState) => true;

    public override bool IsBackwardValid(ENPCStates worldState) => (ENPCStates.IS_PLAYER_DEAD & worldState) == 0;

    public override ENPCStates ApplyEffects(in ENPCStates worldState)
    {
        ENPCStates newWorldstate = worldState;
        newWorldstate &= ~ENPCStates.IS_NEAR_WEAPON;
        newWorldstate |= ENPCStates.IS_NEAR_PLAYER;

        return newWorldstate;
    }

    public override ENPCStates ApplyReversedEffects(in ENPCStates worldState)
    {
        ENPCStates newWorldstate = worldState;
        newWorldstate &= ~ENPCStates.IS_NEAR_PLAYER;

        return newWorldstate;
    }

    int currentStep = 0;
    const int maxStep = 20;

    public override EActionState Execute()
    {
        currentStep++;

        if (currentStep >= maxStep)
        {
            Console.Write("\r" + new string(' ', Console.BufferWidth));
            Utils.TimedWrite("\r-> Moved to [PLAYER_NAME]\n", 0);

            return EActionState.SUCCESS;
        }

        Utils.DrawProgressBar("-> Moving to [PLAYER_NAME] ", currentStep, maxStep);

        Thread.Sleep(100);

        return EActionState.RUNNING;
    }
}

class KillEnemy : GOAP.Action<ENPCStates>
{
    public override int GetCost(in ENPCStates worldState) => (ENPCStates.HAS_WEAPON & worldState) != 0 ? 1 : 10;

    public override bool IsForwardValid(ENPCStates worldState)
    {
        return (worldState & ENPCStates.IS_HURT) == 0 && (ENPCStates.IS_NEAR_PLAYER & worldState) != 0;
    }

    public override bool IsBackwardValid(ENPCStates worldState) => true;

    public override ENPCStates ApplyEffects(in ENPCStates worldState)
    {
        ENPCStates newWorldstate = worldState;
        newWorldstate |= ENPCStates.IS_PLAYER_DEAD;

        return newWorldstate;
    }

    public override ENPCStates ApplyReversedEffects(in ENPCStates worldState)
    {
        ENPCStates newWorldstate = worldState;
        newWorldstate &= ~ENPCStates.IS_PLAYER_DEAD;

        return newWorldstate;
    }

    public override EActionState Execute()
    {
        Utils.TimedWrite("-> Killed [PLAYER_NAME]\n");

        return EActionState.SUCCESS;
    }
}

class HealSelf : GOAP.Action<ENPCStates>
{
    public override int GetCost(in ENPCStates worldState) => 5;

    public override bool IsForwardValid(ENPCStates worldState) => (worldState & ENPCStates.IS_HURT) != 0;

    public override bool IsBackwardValid(ENPCStates worldState) => (worldState & ENPCStates.IS_PLAYER_DEAD) == 0;

    public override ENPCStates ApplyEffects(in ENPCStates worldState)
    {
        ENPCStates newWorldstate = worldState;

        newWorldstate &= ~ENPCStates.IS_HURT;

        return newWorldstate;
    }

    public override ENPCStates ApplyReversedEffects(in ENPCStates worldState)
    {
        ENPCStates newWorldstate = worldState;

        newWorldstate |= ENPCStates.IS_HURT;

        return newWorldstate;
    }

    public override EActionState Execute()
    {
        Utils.TimedWrite("-> Healed self up\n");

        return EActionState.SUCCESS;
    }
}

class GarbageAction : GOAP.Action<ENPCStates>
{
    private int internalCost = 0;
    public GarbageAction(int cost)
    {
        this.internalCost = cost;
    }
    public override int GetCost(in ENPCStates worldState) => internalCost;

    public override bool IsForwardValid(ENPCStates worldState) => true;

    public override EActionState Execute()
    {
        Utils.TimedWrite("-> Test\n");

        return EActionState.SUCCESS;
    }
}