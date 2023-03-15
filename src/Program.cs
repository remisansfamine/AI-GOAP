using System;
using System.Diagnostics;
using GOAP;

internal class Program
{
    static void Main(string[] args)
    {
        Planner planner = new Planner();

        Node<EEnemyStates> rootNode = new Node<EEnemyStates>();
        rootNode.worldState = EEnemyStates.IS_HURT;

        List<Node<EEnemyStates>> leaves = new List<Node<EEnemyStates>>();

        EEnemyStates goal = new EEnemyStates();
        goal = EEnemyStates.IS_ENEMY_DEAD;

        List<GOAP.Action<EEnemyStates>> availableActions = new List<GOAP.Action<EEnemyStates>>() { new GarbageAction(70), new MoveToEnemy(), new GarbageAction(50), new PickUpWeapon(), new GarbageAction(50), new GarbageAction(50), new HealSelf(), new GarbageAction(50), new MoveToWeapon(), new KillEnemy(), new GarbageAction(50), new GarbageAction(50) };

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        Planner.BuildGraph(rootNode, leaves, availableActions, goal, (EEnemyStates currState, EEnemyStates goal) => (currState & EEnemyStates.IS_ENEMY_DEAD) != 0);

        stopWatch.Stop();

        // Get the elapsed time as a TimeSpan value.
        TimeSpan ts = stopWatch.Elapsed;

        Console.WriteLine("RunTime " + ts);


        // No plan possible
        if (!leaves.Any())
            return;

        List<Node<EEnemyStates>> bestNodes = Planner.GetBestLeaves(leaves);
        List<Node<EEnemyStates>> bestBranch = Planner.UnrollLeaf(bestNodes.First());

        Queue<GOAP.Action<EEnemyStates>> finalPlan = new Queue<GOAP.Action<EEnemyStates>>(bestBranch.Select(node => node.action));

        foreach (GOAP.Action<EEnemyStates> action in finalPlan)
            action.Execute();
    }
}
