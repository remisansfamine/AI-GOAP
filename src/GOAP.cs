
namespace GOAP
{

    struct WorldState
    {
        public WorldState() { }

        public bool hasAxe = false;
        public bool hasPickaxe = false;
        public bool hasWood = false;
        public bool hasRock = false;
    }

    class Node
    {
        public WorldState worldState = new WorldState();

        public Node parent = null;

        public List<Node> children = new List<Node>();

        public Action action = null;

        public int cost = 0;
    }

    abstract class Action
    {
        protected Action(int cost)
        {
            this.cost = cost;
        }

        public virtual bool IsValid(WorldState worldState) => false;

        public virtual WorldState ApplyEffects(in WorldState worldState) => worldState;

        public int cost { get; private set; } = 0;

        public abstract void Execute();
    }

    class Planner
    {
        private List<Action> CreateActionSubSet(List<Action> usableActions, Action toRemoveAction)
        {
            return usableActions.Where(action => action != toRemoveAction).ToList();
        }

        private Node CreateNode(Node parent, WorldState newWorldState, Action action)
        {
            Node newNode = new Node();
            newNode.action = action;
            newNode.worldState = newWorldState;
            newNode.parent = parent;
            parent.children.Add(newNode);
            newNode.cost = parent.cost + action.cost;

            return newNode;
        }

        public void BuildGraph(Node parent, List<Node> leaves, List<Action> availableActions, WorldState goal, Func<WorldState, WorldState, bool> goalAchived)
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
    }
}
