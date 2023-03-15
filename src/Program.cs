using System;
using GOAP;

internal class Program
{
    static void Main(string[] args)
    {
        Planner planner = new Planner();

        Node rootNode = new Node();
        rootNode.worldState.isHurt = true;

        List<Node> leaves = new List<Node>();

        WorldState goal = new WorldState();
        goal.isEnemyDead = true;

        List<GOAP.Action> availableActions = new List<GOAP.Action>() { new MoveToWeapon(), new PickUpWeapon(), new MoveToEnemy(), new KillEnemy(), new HealSelf() };

        Planner.BuildGraph(rootNode, leaves, availableActions, goal, (WorldState currState, WorldState goal) => goal.isEnemyDead = currState.isEnemyDead);

        // No plan possible
        if (!leaves.Any())
            return;

        List<Node> bestNodes = Planner.GetBestLeaves(leaves);
        List<Node> bestBranch = Planner.UnrollLeaf(bestNodes.First());

        Queue<GOAP.Action> finalPlan = new Queue<GOAP.Action>(bestBranch.Select(node => node.action));

        foreach (GOAP.Action action in finalPlan)
            action.Execute();
    }
}
