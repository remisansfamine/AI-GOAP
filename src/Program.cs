using System;
using GOAP;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");

        Planner planner = new Planner();

        Node rootNode = new Node();
        List<Node> leaves = new List<Node>();

        WorldState goal = new WorldState();
        goal.hasAxe = true;
        goal.hasWood = true;

        List<GOAP.Action> availableActions = new List<GOAP.Action>() { new GetAxeAction(), new GetPickaxeAction(), new GetWoodAction(), new GetRockAction() };

        planner.BuildGraph(rootNode, leaves, availableActions, goal);
    }
}
