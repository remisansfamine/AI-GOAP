using System;
using GOAP;

internal class Program
{
    static void Main(string[] args)
    {
        Planner planner = new Planner();

        Node rootNode = new Node();
        List<Node> leaves = new List<Node>();

        WorldState goal = new WorldState();
        goal.hasAxe = true;
        goal.hasWood = true;

        List<GOAP.Action> availableActions = new List<GOAP.Action>() { new GetAxeAction(), new GetPickaxeAction(), new ChopWoodAction(), new GetRockAction(), new GetSticksAction() };

        planner.BuildGraph(rootNode, leaves, availableActions, goal, (WorldState currState, WorldState goal) => currState.hasWood);

        // No plan possible
        if (!leaves.Any())
            return;

        List<Node> bestNodes = leaves.Where((ws) => ws.cost == leaves.Min(ws => ws.cost)).ToList();

        Node firstBestNode = bestNodes.First();

        List<Node> bestBranch = new List<Node>();

        Node currentNode = firstBestNode;

        while (currentNode.parent is not null)
        {
            bestBranch.Add(currentNode);
            currentNode = currentNode.parent;
        }


        List<GOAP.Action> finalPlan = bestBranch.Select(node => node.action).ToList();

        foreach (GOAP.Action action in finalPlan)
            action.Execute();
    }
}
