using System;
using System.Diagnostics;
using System.Numerics;
using GOAP;
using static System.Collections.Specialized.BitVector32;

internal class Program
{
    static bool RunAction(GOAP.Action<ENPCStates> action)
    {
        bool actionIsRunning = true;
        while (actionIsRunning)
        {
            EActionState state = action.Execute();

            if (state == EActionState.FAILURE)
                return false;

            if (state == EActionState.SUCCESS)
                actionIsRunning = false;
        }

        return true;
    }

    static void ExecutePlan(List<Node<ENPCStates>> plan, bool useFowarding)
    {
        Queue<GOAP.Action<ENPCStates>> finalPlan = new Queue<GOAP.Action<ENPCStates>>(plan.Select(node => node.action));

        foreach (GOAP.Action<ENPCStates> action in finalPlan)
        {
            bool actionSucceed = RunAction(action);

            if (!actionSucceed)
                break;
        }
    }

    static void DisplayActions(List<GOAP.Action<ENPCStates>> actions)
    {
        foreach (var action in actions)
            Utils.TimedWrite($"- {action.GetType().Name}\n");
    }

    static bool AskForForwardPlanning()
    {
        Utils.TimedWrite("Do you want to use forward planning ? If not, your plan will be elaborated using backward planning. (true/false)\n");

        bool? shouldUseForward = null;

        while (shouldUseForward is null)
        {
            string? forwardResponse = Console.ReadLine();

            if (forwardResponse is not null && Boolean.TryParse(forwardResponse, out bool value))
            {
                shouldUseForward = value;
            }
            else
            {
                Utils.TimedWrite("Boolean value is not recognized, please try again. (true/false)\n");
            }
        }

        return shouldUseForward.Value;
    }
    static bool AskForPruning()
    {
        Utils.TimedWrite("Do you want to use pruning ? It can dramatically increase performance with forward planning. (true/false)\n");

        bool? shouldUseForward = null;

        while (shouldUseForward is null)
        {
            string? forwardResponse = Console.ReadLine();

            if (forwardResponse is not null && Boolean.TryParse(forwardResponse, out bool value))
            {
                shouldUseForward = value;
            }
            else
            {
                Utils.TimedWrite("Boolean value is not recognized, please try again. (true/false)\n");
            }
        }

        return shouldUseForward.Value;
    }

    static void AskToAddGarbageActions(List<GOAP.Action<ENPCStates>> actions)
    {
        Utils.TimedWrite("Do you want to add GarbageActions to try the performance ? (true/false)\n");

        bool? shouldAddGarbageAction = null;

        while (shouldAddGarbageAction is null)
        {
            string? garbageResponse = Console.ReadLine();

            if (garbageResponse is not null && Boolean.TryParse(garbageResponse, out bool value))
            {
                shouldAddGarbageAction = value;
            }
            else
            {
                Utils.TimedWrite("Boolean value is not recognized, please try again. (true/false)\n");
            }
        }

        if (!shouldAddGarbageAction.Value)
        {
            Utils.TimedWrite("No GarbageAction will be added.\n");
            return;
        }

        Utils.TimedWrite("How many GarbageAction do you want to add ? (max = 50)\n");

        int? countInput = null;

        while (countInput is null)
        {
            string? garbageResponse = Console.ReadLine();

            if (garbageResponse is not null && Int32.TryParse(garbageResponse, out int value))
            {
                countInput = value;
            }
            else
            {
                Utils.TimedWrite("Numeric value is not recognized, please try again.\n");
            }
        }

        int garbageActionCount = Math.Min(countInput.Value, 50);

        for (int i = 0; i < garbageActionCount; i++)
            actions.Add(new GarbageAction(5));

        Utils.TimedWrite($"{garbageActionCount} GarbageAction(s) have been added");
    }

    static string PlanAsString(List<Node<ENPCStates>> branch) => string.Join(" -> ", branch.Select(node => node.action.GetType().Name));

    static void Main(string[] args)
    {
        Utils.TimedWrite("Welcome to [GAME_NAME], NPC-936DA01F-9ABD-4d9d-80C7-02AF85C822A8.\n");
        Utils.TimedWrite("Your goal is to elaborate a plan to kill [PLAYER_NAME].\n");
        Utils.TimedWrite("Heres the different actions you can make:\n");

        List<GOAP.Action<ENPCStates>> availableActions = new List<GOAP.Action<ENPCStates>>() { new MoveToWeapon(), new KillEnemy(), new PickUpWeapon(), new MoveToEnemy(), new HealSelf() };

        DisplayActions(availableActions);
        AskToAddGarbageActions(availableActions);

        ENPCStates initialState = ENPCStates.IS_HURT;
        ENPCStates goalActiveState = ENPCStates.IS_PLAYER_DEAD;

        Utils.TimedWrite($"Your initial state is : {initialState}\n");
        Utils.TimedWrite($"And you have to meet these following criteria: {goalActiveState}\n");

        bool useForwardPlanning = AskForForwardPlanning();

        Utils.TimedWrite($"Your plan will be elaborated using {(useForwardPlanning ? "forward" : "backward")} planning...\n", 25);
        Utils.TimedWrite($"Good luck.\n", 25);

        bool usePruning = AskForPruning();
        Utils.TimedWrite($"Your plan will be elaborated {(usePruning ? "" : "without" )}using pruning...\n", 25);

        List<Node<ENPCStates>> leaves = new List<Node<ENPCStates>>();

        Stopwatch stopWatch = new Stopwatch();

        var goalChecker = (ENPCStates state, EnemyGoal goal) => (goal.activeStates == ENPCStates.NONE || (state & goal.activeStates) == goal.activeStates) && (goal.inactiveStates == ENPCStates.NONE || (state & goal.inactiveStates) == 0);

        stopWatch.Start();

        if (useForwardPlanning)
        {
            EnemyGoal fowardGoal = new EnemyGoal(goalActiveState);
            Planner<EnemyGoal>.BuildGraph(initialState, leaves, availableActions, fowardGoal, goalChecker, usePruning);
        }
        else
        {
            ENPCStates completeGoal = ENPCStates.IS_PLAYER_DEAD | ENPCStates.IS_NEAR_PLAYER | ENPCStates.HAS_WEAPON;
            EnemyGoal initialStateGoal = new EnemyGoal(initialState, ENPCStates.IS_PLAYER_DEAD | ENPCStates.HAS_WEAPON | ENPCStates.IS_NEAR_WEAPON | ENPCStates.IS_NEAR_PLAYER);
            Planner<EnemyGoal>.BuildReversedGraph(completeGoal, leaves, availableActions, initialStateGoal, goalChecker, usePruning);
        }

        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;

        // No plan possible
        if (!leaves.Any())
        {
            Utils.TimedWrite($"You miserably failed to make a plan !\n", 50);

            return;
        }

        List<Node<ENPCStates>> bestLeaves = Planner<EnemyGoal>.GetBestLeaves(leaves);
        List<Node<ENPCStates>> bestBranch = Planner<EnemyGoal>.UnrollLeaf(bestLeaves.First(), useForwardPlanning);

        Utils.TimedWrite($"Your plan is the following: ");

        string planAsString = PlanAsString(bestBranch);

        Utils.TimedWrite($"{planAsString} \n");

        Utils.TimedWrite($"And you took {ts} to elaborate it.\n");

        ExecutePlan(bestBranch, useForwardPlanning);

        Utils.TimedWrite($"Good job, you managed to kill [PLAYER_NAME]\n");
    }
}
