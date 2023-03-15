
namespace GOAP
{
    class Node<StateT>
    {
        public StateT worldState = default(StateT);

        public Node<StateT> parent = null;

        public List<Node<StateT>> children = new List<Node<StateT>>();

        public Action<StateT> action = null;

        public int runningCost = 0;
    }

    abstract class Action<StateT>
    {
        public virtual bool IsValid(StateT worldState) => false;

        public virtual StateT ApplyEffects(in StateT worldState) => worldState;

        public abstract int cost { get; }

        public abstract void Execute();
    }

    class Planner
    {
        static private List<Action<StateT>> CreateActionSubSet<StateT>(List<Action<StateT>> usableActions, Action<StateT> toRemoveAction)
        {
            return usableActions.Where(action => action != toRemoveAction).ToList();
        }

        static private Node<StateT> CreateNode<StateT>(Node<StateT> parent, StateT newWorldState, Action<StateT> action)
        {
            Node<StateT> newNode = new Node<StateT>();
            newNode.action = action;
            newNode.worldState = newWorldState;
            newNode.parent = parent;
            parent.children.Add(newNode);
            newNode.runningCost = parent.runningCost + action.cost;

            return newNode;
        }

        static public void BuildGraph<StateT>(Node<StateT> parent, List<Node<StateT>> leaves, List<Action<StateT>> availableActions, StateT goal, Func<StateT, StateT, bool> goalAchieved)
        {
            int cheapestCost = int.MaxValue;
            BuildGraph(parent, leaves, availableActions, goal, goalAchieved, ref cheapestCost);
        }

        static public void BuildGraph<StateT>(Node<StateT> parent, List<Node<StateT>> leaves, List<Action<StateT>> availableActions, StateT goal, Func<StateT, StateT, bool> goalAchieved, ref int cheapestCost)
        {
            foreach (Action<StateT> action in availableActions)
            {
                if (!action.IsValid(parent.worldState)) // Check preconditions
                    continue;

                StateT newWorldState = action.ApplyEffects(parent.worldState);
                Node<StateT> node = CreateNode(parent, newWorldState, action); // node.cost = parent.cost + action.cost

                // Prune branches that which are more expensive than the cheapest one
                if (node.runningCost > cheapestCost)
                    continue;

                if (goalAchieved(newWorldState, goal))
                {
                    cheapestCost = Math.Min(cheapestCost, node.runningCost);

                    leaves.Add(node);
                    continue;
                }

                // used action is removed
                List<Action<StateT>> actionsSubSet = CreateActionSubSet(availableActions, action);

                // recursive call on the new node
                BuildGraph(node, leaves, actionsSubSet, goal, goalAchieved, ref cheapestCost);
            }
        }

        static public List<Node<StateT>> GetBestLeaves<StateT>(List<Node<StateT>> leaves) => leaves.Where((ws) => ws.runningCost == leaves.Min(ws => ws.runningCost)).ToList();
        static public List<Node<StateT>> UnrollLeaf<StateT>(Node<StateT> toUnroll)
        {
            List<Node<StateT>> bestBranch = new List<Node<StateT>>();

            Node<StateT> currentNode = toUnroll;

            while (currentNode.parent is not null)
            {
                bestBranch.Add(currentNode);
                currentNode = currentNode.parent;
            }

            return bestBranch.Reverse<Node<StateT>>().ToList();
        }
    }
}
