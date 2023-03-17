using System;
using System.Diagnostics;
using System.Numerics;
using GOAP;
using static System.Collections.Specialized.BitVector32;

internal class Program
{
    static bool RunAction(GOAP.Action<EEnemyStates> action)
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

    static void ExecutePlan(List<Node<EEnemyStates>> plan, bool useFowarding)
    {
        Queue<GOAP.Action<EEnemyStates>> finalPlan = new Queue<GOAP.Action<EEnemyStates>>(plan.Select(node => node.action));

        foreach (GOAP.Action<EEnemyStates> action in finalPlan)
        {
            bool actionSucceed = RunAction(action);

            if (!actionSucceed)
                break;
        }
    }

    static void DisplayActions(List<GOAP.Action<EEnemyStates>> actions)
    {
        foreach (var action in actions)
            Utils.TimedWrite($"- {action.GetType().Name}\n", 10);
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

    static void AskToAddGarbageActions(List<GOAP.Action<EEnemyStates>> actions)
    {
        Utils.TimedWrite("Do you want to add GarbageActions to try the performance ? (true/false)\n", 10);

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

        Utils.TimedWrite("How many GarbageAction do you want to add ? (max = 15)\n");

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

        int garbageActionCount = Math.Min(countInput.Value, 15);

        for (int i = 0; i < garbageActionCount; i++)
            actions.Add(new GarbageAction(5));

        Utils.TimedWrite($"{garbageActionCount} GarbageAction(s) have been added");
    }

    static string PlanAsString(List<Node<EEnemyStates>> branch) => string.Join(" -> ", branch.Select(node => node.action.GetType().Name));

    static void Main(string[] args)
    {
        Utils.TimedWrite("Welcome to [GAME_NAME], NPC-936DA01F-9ABD-4d9d-80C7-02AF85C822A8.\n");
        Utils.TimedWrite("Your goal is to elaborate a plan to kill [PLAYER_NAME].\n");
        Utils.TimedWrite("Heres the different actions you can make:\n");

        List<GOAP.Action<EEnemyStates>> availableActions = new List<GOAP.Action<EEnemyStates>>() { new MoveToWeapon(), new KillEnemy(), new PickUpWeapon(), new MoveToEnemy(), new HealSelf() };

        DisplayActions(availableActions);
        AskToAddGarbageActions(availableActions);

        EEnemyStates initialState = EEnemyStates.IS_HURT;
        EEnemyStates goalActiveState = EEnemyStates.IS_ENEMY_DEAD;

        Utils.TimedWrite($"Your initial state is : {initialState}\n");
        Utils.TimedWrite($"And you have to meet these following criteria: {goalActiveState}\n");

        bool useForwardPlanning = AskForForwardPlanning();

        Utils.TimedWrite($"Your plan will be elaborated using {(useForwardPlanning ? "forward" : "backward")} planning...\n", 25);
        Utils.TimedWrite($"Good luck.\n", 25);

        List<Node<EEnemyStates>> leaves = new List<Node<EEnemyStates>>();


        Stopwatch stopWatch = new Stopwatch();

        var goalChecker = (EEnemyStates state, EnemyGoal goal) => (goal.activeStates == EEnemyStates.NONE || (state & goal.activeStates) == goal.activeStates) && (goal.inactiveStates == EEnemyStates.NONE || (state & goal.inactiveStates) == 0);

        stopWatch.Start();

        if (useForwardPlanning)
        {
            var fowardGoal = new EnemyGoal(goalActiveState);
            Planner<EnemyGoal>.BuildGraph(initialState, leaves, availableActions, fowardGoal, goalChecker);
        }
        else
        {
            var initialStateGoal = new EnemyGoal(initialState, EEnemyStates.IS_ENEMY_DEAD | EEnemyStates.HAS_WEAPON | EEnemyStates.IS_NEAR_WEAPON | EEnemyStates.IS_NEAR_ENEMY);
            Planner<EnemyGoal>.BuildReversedGraph(EEnemyStates.IS_ENEMY_DEAD | EEnemyStates.IS_NEAR_ENEMY | EEnemyStates.HAS_WEAPON, leaves, availableActions, initialStateGoal, goalChecker);
        }

        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;

        // No plan possible
        if (!leaves.Any())
        {
            Utils.TimedWrite($"You miserably failed to make a plan !\n", 50);

            return;
        }

        List<Node<EEnemyStates>> bestLeaves = Planner<EnemyGoal>.GetBestLeaves(leaves);
        List<Node<EEnemyStates>> bestBranch = Planner<EnemyGoal>.UnrollLeaf(bestLeaves.First(), useForwardPlanning);

        Utils.TimedWrite($"Your plan is the following: ", 10);

        string planAsString = PlanAsString(bestBranch);

        Utils.TimedWrite($"{planAsString} \n", 10);

        Utils.TimedWrite($"And you took {ts} to elaborate it.\n", 10);

        ExecutePlan(bestBranch, useForwardPlanning);
    }
}
