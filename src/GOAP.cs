
namespace GOAP
{

    struct WorldState
    {
        public WorldState() { }

        public bool isEnemyDead = false;
        public bool hasWeapon = false;
        public bool isNearWeapon = false;
        public bool isNearEnemy = false;
        public bool isHurt = false;
    }

    class Node
    {
        public WorldState worldState = new WorldState();

        public Node parent = null;

        public List<Node> children = new List<Node>();

        public Action action = null;

        public int runningCost = 0;
    }

    abstract class Action
    {

        public virtual bool IsValid(WorldState worldState) => false;

        public virtual WorldState ApplyEffects(in WorldState worldState) => worldState;

        public abstract int cost { get; }

        public abstract void Execute();
    }

    class Planner
    {
        static private List<Action> CreateActionSubSet(List<Action> usableActions, Action toRemoveAction)
        {
            return usableActions.Where(action => action != toRemoveAction).ToList();
        }

        static private Node CreateNode(Node parent, WorldState newWorldState, Action action)
        {
            Node newNode = new Node();
            newNode.action = action;
            newNode.worldState = newWorldState;
            newNode.parent = parent;
            parent.children.Add(newNode);
            newNode.runningCost = parent.runningCost + action.cost;

            return newNode;
        }

        static public void BuildGraph(Node parent, List<Node> leaves, List<Action> availableActions, WorldState goal, Func<WorldState, WorldState, bool> goalAchived)
        {
            foreach (Action action in availableActions)
            {
                if (!action.IsValid(parent.worldState)) // check preconditions
                    continue;

                WorldState newWorldState = action.ApplyEffects(parent.worldState);
                Node node = CreateNode(parent, newWorldState, action); // node.cost = parent.cost + action.cost

                if (goalAchived(newWorldState, goal))
                {
                    leaves.Add(node);
                    continue;
                }

                // used action is removed
                List<Action> actionsSubSet = CreateActionSubSet(availableActions, action);

                // recursive call on the new node
                BuildGraph(node, leaves, actionsSubSet, goal, goalAchived);
            }
        }

        static public List<Node> GetBestLeaves(List<Node> leaves) => leaves.Where((ws) => ws.runningCost == leaves.Min(ws => ws.runningCost)).ToList();
        static public List<Node> UnrollLeaf(Node toUnroll)
        {
            List<Node> bestBranch = new List<Node>();

            Node currentNode = toUnroll;

            while (currentNode.parent is not null)
            {
                bestBranch.Add(currentNode);
                currentNode = currentNode.parent;
            }

            return bestBranch.Reverse<Node>().ToList();
        }
    }
}
