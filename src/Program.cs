using System;
using System.Diagnostics;
using System.Numerics;
using GOAP;

internal class Program
{
    static void Main(string[] args)
    {
        List<Node<EEnemyStates>> leaves = new List<Node<EEnemyStates>>();

        // List<GOAP.Action<EEnemyStates>> availableActions = new List<GOAP.Action<EEnemyStates>>() { new GarbageAction(70), new MoveToEnemy(), new GarbageAction(50), new PickUpWeapon(), new GarbageAction(50), new GarbageAction(50), new HealSelf(), new GarbageAction(50), new MoveToWeapon(), new KillEnemy(), new GarbageAction(50), new GarbageAction(50) };
        List<GOAP.Action<EEnemyStates>> availableActions = new List<GOAP.Action<EEnemyStates>>() { new KillEnemy(), new MoveToEnemy(), new PickUpWeapon(), new MoveToWeapon(), new HealSelf() };

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        var goalChecker = (EEnemyStates state, EnemyGoal goal) => (goal.activeStates == EEnemyStates.NONE || (state & goal.activeStates) == goal.activeStates) && (goal.inactiveStates == EEnemyStates.NONE || (state & goal.inactiveStates) != goal.inactiveStates);

        //var fowardGoal = new EnemyGoal(EEnemyStates.IS_ENEMY_DEAD);
        //Planner<Goal<EEnemyStates>>.BuildGraph(EEnemyStates.IS_HURT, leaves, availableActions, , goalChecker);

        var initialStateGoal = new EnemyGoal(EEnemyStates.IS_HURT, EEnemyStates.IS_ENEMY_DEAD | EEnemyStates.HAS_WEAPON);
        Planner<EnemyGoal>.BuildReversedGraph(EEnemyStates.IS_ENEMY_DEAD | EEnemyStates.HAS_WEAPON, leaves, availableActions, initialStateGoal, goalChecker);

        stopWatch.Stop();

        // Get the elapsed time as a TimeSpan value.
        TimeSpan ts = stopWatch.Elapsed;

        Console.WriteLine("RunTime " + ts);

        // No plan possible
        if (!leaves.Any())
            return;

        List<Node<EEnemyStates>> bestLeaves = Planner<EnemyGoal>.GetBestLeaves(leaves);
        List<Node<EEnemyStates>> bestBranch = Planner<EnemyGoal>.UnrollLeaf(bestLeaves.First(), false);

        Queue<GOAP.Action<EEnemyStates>> finalPlan = new Queue<GOAP.Action<EEnemyStates>>(bestBranch.Select(node => node.action));

        foreach (GOAP.Action<EEnemyStates> action in finalPlan)
            action.Execute();
    }
}
